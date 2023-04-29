using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class NetworkController : MonoBehaviourPunCallbacks
{
    public static NetworkController instance;
    Transform connectBtn;
    bool isConnected = false;

    public override void OnEnable()
    {
        DontDestroyOnLoad(gameObject);
        instance = this;
        base.OnEnable();
    }

    void Start() {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();

        connectBtn = transform.Find("/UI/ConnectBtn");
        connectBtn.GetComponent<Button>().onClick.AddListener(ChangeConnectionState);
        UpdateConnectionBtnText();
    }

    void ChangeConnectionState() {
        if (isConnected) {
            Disconnect();
        } else {
            Connect();
        }
    }

    void Connect() {
        connectBtn.GetComponent<Button>().interactable = false;
        RoomOptions options = new RoomOptions();
        options.IsVisible = true;
        options.MaxPlayers = 8;
        PhotonNetwork.JoinOrCreateRoom("testroom", options, TypedLobby.Default);
    }

    void Disconnect() {
        connectBtn.GetComponent<Button>().interactable = false;
        PhotonNetwork.LeaveRoom();
    }

    void UpdateConnectionBtnText() {
        connectBtn.GetComponentInChildren<TMPro.TMP_Text>().text = isConnected ? "Disconnect" : "Connect";
    }

    public override void OnJoinedRoom() {
        base.OnJoinedRoom();
        connectBtn.GetComponent<Button>().interactable = true;
        isConnected = true;
        UpdateConnectionBtnText();

        SpawnNetworkedObject("Player", Vector2.zero, GameController.instance.PlayerHolderTransform);
    }

    public override void OnLeftRoom() {
        base.OnLeftRoom();
        connectBtn.GetComponent<Button>().interactable = false;
        isConnected = false;
        UpdateConnectionBtnText();
    }

    public override void OnConnectedToMaster()
    {
        connectBtn.GetComponent<Button>().interactable = true;
        base.OnConnectedToMaster();
    }

    public GameObject SpawnNetworkedObject(string prefabName, Vector2 position, Transform parent = null) {
        GameObject go = PhotonNetwork.Instantiate(prefabName, position, Quaternion.identity);
        if (parent != null) {
            RpcSetParent(go.GetComponent<PhotonView>().ViewID, parent);
        }
        return go;
    }

    [PunRPC]
    public void RpcSetParent(int viewID, Transform parent) {
        PhotonView child = PhotonNetwork.GetPhotonView(viewID);
        child.transform.SetParent(parent);
    }
}
