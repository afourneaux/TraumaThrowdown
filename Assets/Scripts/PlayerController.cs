using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    public static PlayerController instance;
    public static List<PlayerController> AllPlayers;
    public static bool isDirty = false;
    public bool hasSelectedCharacter = false;
    private ConstantsAndHelpers.CharacterEnum _selectedCharacter;
    public ConstantsAndHelpers.CharacterEnum selectedCharacter {
        get {
            return _selectedCharacter;
        }
        set {
            _selectedCharacter = value;
            isDirty = true;
        }
    }
    
    float respawnTimer = 1f;
    public ConstantsAndHelpers.RespawnState respawnState;
    public int lives = ConstantsAndHelpers.START_LIVES;
    public Character character;

    private bool _isReady;
    public bool isReady {
        get {
            return _isReady;
        }
        set {
            _isReady = value;
            isDirty = true;
        }
    }

    public override void OnEnable() {
        if (photonView.IsMine) {
            if (instance == null) {
                instance = this;
            } else {
                Destroy(gameObject);
                return;
            }
        }
        transform.name = $"PlayerController-{photonView.Owner.NickName}";
        if (AllPlayers == null) {
            AllPlayers = new List<PlayerController>();
        }
        isDirty = true;
        AllPlayers.Add(this);
        base.OnEnable();
    }

    void Start()
    {
    }

    void Update()
    {
        if (photonView.IsMine == false) {
            return;
        }
        if (character?.isInitialised == true && character?.hp <= 0) {
            NetworkController.DestroyNetworkedObject(character.gameObject);
            character = null;
            lives--;
            if (lives > 0) {
                respawnState = ConstantsAndHelpers.RespawnState.COUNTDOWN;
                respawnTimer = ConstantsAndHelpers.RESPAWN_DELAY;
            }
        }
        if (respawnState == ConstantsAndHelpers.RespawnState.COUNTDOWN) {
            respawnTimer -= Time.deltaTime;
            if (respawnTimer < 0) {
                respawnState =  ConstantsAndHelpers.RespawnState.NOW;
            }
        }
    }

    public override void OnDisable() {
        isDirty = true;
        AllPlayers.Remove(this);
        base.OnDisable();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) {
            stream.SendNext(isReady);
            stream.SendNext(hasSelectedCharacter);
            stream.SendNext(selectedCharacter);
            stream.SendNext(lives);
            stream.SendNext(respawnState);
        } else {
            isReady = (bool)stream.ReceiveNext();
            hasSelectedCharacter = (bool)stream.ReceiveNext();
            selectedCharacter = (ConstantsAndHelpers.CharacterEnum)stream.ReceiveNext();
            lives = (int)stream.ReceiveNext();
            respawnState = (ConstantsAndHelpers.RespawnState)stream.ReceiveNext();
        }
    }

    // TODO - Properly block other respawn requests and tell master client when it's done
    [PunRPC]
    public void RPCSpawnCharacter(float x, float y) {
        respawnState =  ConstantsAndHelpers.RespawnState.NONE;
        if (photonView.IsMine == false) {
            return;
        }
        if (character != null) {
            Debug.LogWarning($"Trying to spawn character for user {photonView.Owner.NickName} but one already exists");
            return;
        }
        GameObject go = NetworkController.SpawnNetworkedObject("Character", new Vector2(x, y), transform);
        character = go.GetComponent<Character>();
    }
}
