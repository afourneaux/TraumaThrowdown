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
    
    public float respawnTimer = 1f;
    public bool needsRespawn = false;
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
        /*if (needsRespawn) {
            respawnTimer -= dt;
            if (respawnTimer < 0) {
                SpawnCharacter();
                needsRespawn = false;
            }
        }*/
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
        } else {
            isReady = (bool)stream.ReceiveNext();
            hasSelectedCharacter = (bool)stream.ReceiveNext();
            selectedCharacter = (ConstantsAndHelpers.CharacterEnum)stream.ReceiveNext();
        }
    }
}
