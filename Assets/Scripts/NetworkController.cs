using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class NetworkController : MonoBehaviourPunCallbacks
{
    public static NetworkController instance;
    Button connectBtn;
    TMPro.TMP_InputField roomNameInput;
    TMPro.TMP_InputField playerNameInput;
    public string nameToBlame = "";

    public List<int> DEBUGProfilerSent;
    public List<int> DEBUGProfilerReceived;


    public override void OnEnable()
    {
        if (instance == null) {
            instance = this;
        } else {
            Destroy(gameObject);
        }
        DEBUGProfilerSent = new List<int>();
        DEBUGProfilerReceived = new List<int>();
        base.OnEnable();
    }

    #region LobbyMenu
    void Start() {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();

        connectBtn = transform.Find("/UI/ConnectBtn").GetComponent<Button>();
        connectBtn.onClick.AddListener(Connect);
        roomNameInput = transform.Find("/UI/RoomNameInput").GetComponent<TMPro.TMP_InputField>();
        playerNameInput = transform.Find("/UI/PlayerNameInput").GetComponent<TMPro.TMP_InputField>();
    }

    void Connect() {
        if (roomNameInput.text.Length == 0 || playerNameInput.text.Length == 0) {
            AudioController.instance.PlaySound("UICancel");
            return;
        }
        AudioController.instance.PlaySound("UISelect");
        roomNameInput.interactable = false;
        playerNameInput.interactable = false;
        connectBtn.interactable = false;
        RoomOptions options = new RoomOptions();
        options.IsVisible = true;
        options.MaxPlayers = 8;
        PhotonNetwork.NickName = playerNameInput.text;
        PhotonNetwork.JoinOrCreateRoom(roomNameInput.text, options, TypedLobby.Default);
    }

    public override void OnJoinedRoom() {
        base.OnJoinedRoom();
        ChangeScene("SetupScene");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer) {
        if (PhotonNetwork.CurrentRoom.IsOpen == false) {
            nameToBlame = otherPlayer.NickName;
            Disconnect();
        }
        base.OnPlayerLeftRoom(otherPlayer);
    }

    #endregion

    public static void ChangeScene(string scene) {
        if (PhotonNetwork.IsMasterClient) {
            PhotonNetwork.LoadLevel(scene);
        }
    }

    public static void Disconnect() {
        PhotonNetwork.Disconnect();
    }

    public override void OnLeftRoom() {
        SceneManager.LoadScene("MainMenuScene");
        base.OnLeftRoom();
    }

    public override void OnDisconnected(DisconnectCause cause) {
        SceneManager.LoadScene("MainMenuScene");
        if (cause != DisconnectCause.DisconnectByClientLogic) {
            Debug.LogError(cause.ToString());
        }
        base.OnDisconnected(cause);
    }

    public override void OnConnectedToMaster()
    {
        connectBtn.GetComponent<Button>().interactable = true;
        base.OnConnectedToMaster();
    }

    public static GameObject SpawnNetworkedObject(string prefabName, Vector2 position, Transform parent = null) {
        GameObject go = PhotonNetwork.Instantiate($"NetworkPrefabs/{prefabName}", position, Quaternion.identity);
        if (parent != null) {
            go.GetPhotonView().RPC("RpcSetParent", RpcTarget.All, go.GetComponent<PhotonView>().ViewID, ConstantsAndHelpers.GetFullPathToTransform(parent));
        }
        return go;
    }

    public static void DestroyNetworkedObject(GameObject go) {
        PhotonNetwork.Destroy(go);
    }

    public static void LockRoom() {
        if (PhotonNetwork.IsMasterClient) {
            PhotonNetwork.CurrentRoom.IsOpen = false;
        }
    }

    public static void UnlockRoom() {
        if (PhotonNetwork.IsMasterClient) {
            PhotonNetwork.CurrentRoom.IsOpen = true;
        }
    }

    public void PrintDebugInfo() {
        if (DEBUGProfilerReceived.Count == 0 && DEBUGProfilerSent.Count == 0) {
            return;
        }
        int matchCount = 0;
        string filename = "";

        do {
            matchCount++;
            filename = $"{Application.persistentDataPath}/DEBUG_MATCH_{matchCount}.txt";
        }
        while(File.Exists(filename));

        string toWrite = "Sent\t-\tReceived";
        for (int i = 0; i < DEBUGProfilerReceived.Count; i++) {
            toWrite += $"\n{DEBUGProfilerSent[i]}\t-\t{DEBUGProfilerReceived[i]}";
        }
        byte[] info = new UTF8Encoding(true).GetBytes(toWrite);
        using (FileStream file = File.Create(filename)) {
            file.Write(info);
        }
        DEBUGProfilerReceived.Clear();
        DEBUGProfilerSent.Clear();
    }
}
