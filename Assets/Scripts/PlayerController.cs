using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    public static PlayerController instance;
    public static List<PlayerController> AllPlayers;
    public static bool isDirty = false;
    private ConstantsAndHelpers.CharacterEnum _selectedCharacter = ConstantsAndHelpers.CharacterEnum.NONE;
    public ConstantsAndHelpers.CharacterEnum selectedCharacter {
        get {
            return _selectedCharacter;
        }
        set {
            _selectedCharacter = value;
            isDirty = true;
        }
    }
    
    float respawnTimer;
    public ConstantsAndHelpers.RespawnState respawnState;
    short _lives = ConstantsAndHelpers.START_LIVES;
    public short lives {
        get {
            return _lives;
        }
        set {
            if (value != _lives) {
                isLivesDirty = true;
            }
            _lives = value;
        }
    }
    public bool isLivesDirty = true;
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
            stream.SendNext(selectedCharacter);
            stream.SendNext(lives);
            stream.SendNext(respawnState);
            bool hasCharacter = character != null;
            stream.SendNext(hasCharacter);
            if (hasCharacter) {
                stream.SendNext(character.hp);
                stream.SendNext(character.isInvincible);
                stream.SendNext(character.faceLeft);
            }
            
        } else {
            isReady = (bool)stream.ReceiveNext();
            selectedCharacter = (ConstantsAndHelpers.CharacterEnum)stream.ReceiveNext();
            lives = (short)stream.ReceiveNext();
            respawnState = (ConstantsAndHelpers.RespawnState)stream.ReceiveNext();
            bool hasCharacter = (bool)stream.ReceiveNext();
            if (hasCharacter) {
                character.hp = (short)stream.ReceiveNext();
                character.isInvincible = (bool)stream.ReceiveNext();
                character.faceLeft = (bool)stream.ReceiveNext();
            }
        }
    }

    public void Reset() {
        if (photonView.IsMine == false) {
            return;
        }
        isReady = false;
        selectedCharacter = ConstantsAndHelpers.CharacterEnum.NONE;
        lives = ConstantsAndHelpers.START_LIVES;
        respawnState = ConstantsAndHelpers.RespawnState.NONE;
        if (character != null) {
            NetworkController.DestroyNetworkedObject(character.gameObject);
        }
    }

    // TODO - Properly block other respawn requests and tell master client when it's done
    [PunRPC]
    public void RPCSpawnCharacter(float x, float y) {
        if (photonView.IsMine == false) {
            return;
        }
        respawnState =  ConstantsAndHelpers.RespawnState.NONE;
        if (character != null) {
            Debug.LogWarning($"Trying to spawn character for user {photonView.Owner.NickName} but one already exists");
            return;
        }
        GameObject go = NetworkController.SpawnNetworkedObject("Character", new Vector2(x, y));
        photonView.RPC("RPCConfigureCharacter", RpcTarget.All, go.GetPhotonView().ViewID, photonView.ViewID);
    }

    [PunRPC]
    void RPCConfigureCharacter(int characterID, int playerID) {
        GameObject characterGO = PhotonNetwork.GetPhotonView(characterID).gameObject;
        PlayerController player = PhotonNetwork.GetPhotonView(playerID).GetComponent<PlayerController>();
        if (player.character != null || selectedCharacter == ConstantsAndHelpers.CharacterEnum.NONE) {
            Debug.LogError("Trying to configure character when either none is selected or one is already assigned!");
        }
        Character characterComponent = null;
        switch (player.selectedCharacter) {
            case ConstantsAndHelpers.CharacterEnum.HORALDIN:
                characterComponent = characterGO.AddComponent<Horaldin>();
                break;
            case ConstantsAndHelpers.CharacterEnum.ISIS:
                characterComponent = characterGO.AddComponent<Isis>();
                break;
            case ConstantsAndHelpers.CharacterEnum.LOUIS:
                characterComponent = characterGO.AddComponent<Louis>();
                break;
            case ConstantsAndHelpers.CharacterEnum.OSIRIS:
                characterComponent = characterGO.AddComponent<Osiris>();
                break;
            case ConstantsAndHelpers.CharacterEnum.SCREAM:
                characterComponent = characterGO.AddComponent<Scream>();
                break;
            case ConstantsAndHelpers.CharacterEnum.VAKIR:
                characterComponent = characterGO.AddComponent<Vakir>();
                break;
            case ConstantsAndHelpers.CharacterEnum.WATSON:
                characterComponent = characterGO.AddComponent<Watson>();
                break;
            case ConstantsAndHelpers.CharacterEnum.WILLA:
                characterComponent = characterGO.AddComponent<Willa>();
                break;
            default:
                Debug.LogError($"Unrecognised character found when trying to configure {selectedCharacter} for user {photonView.Owner.NickName}");
                break;
        }
        player.character = characterComponent;
        characterComponent.player = player;
    }
}
