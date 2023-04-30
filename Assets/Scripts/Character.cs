using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;

public class Character : MonoBehaviourPunCallbacks, IPunObservable
{
    const float MAX_SPEED = 4.0f;
    const float ACCELERATION = 15.0f;
    const float JUMP_MAX_RECOVERY_MOMENTUM = 2.0f;
    const float JUMP_MAX_UPWARD_MOMENTUM = 4.0f;
    const float JUMP_FORCE = 8f;
    const int MAX_HP = 100;

    int _hp;
    public int hp {
        get {
            return _hp;
        }
        set {
            _hp = value;
            OnHpChanged();
        }
    }
    bool isJumpReady = false;
    bool doJump = false;
    Rigidbody2D rb;
    RectTransform healthbarRT;
    PlayerController player;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Debug.Log("Character created here:");
        Debug.Log(ConstantsAndHelpers.GetFullPathToTransform(transform));
        player = transform.parent.GetComponent<PlayerController>();
        healthbarRT = transform.Find("CharacterUI/HealthBarValue").GetComponent<RectTransform>();
        hp = MAX_HP;
        transform.Find("CharacterUI/PlayerName").GetComponent<TMPro.TMP_Text>().text = photonView.name;
    }

    void Update()
    {
        if (!photonView.IsMine) {
            return;
        }
        
        float dt = Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.W)) {
            doJump = true;
        }
    }

    void FixedUpdate() 
    {
        if (!photonView.IsMine) {
            return;
        }

        if (Input.GetKey(KeyCode.A)) {
            Vector2 newSpeed = rb.velocity + Vector2.left * Time.fixedDeltaTime * ACCELERATION;
            newSpeed.x = Mathf.Max(newSpeed.x, -MAX_SPEED);
            rb.velocity = newSpeed;
        }
        if (Input.GetKey(KeyCode.D)) {
            Vector2 newSpeed = rb.velocity + Vector2.right * Time.fixedDeltaTime * ACCELERATION;
            newSpeed.x = Mathf.Min(newSpeed.x, MAX_SPEED);
            rb.velocity = newSpeed;
        }
        if (doJump && isJumpReady) {
            Vector2 newSpeed = rb.velocity;
            newSpeed.y = Mathf.Clamp(newSpeed.y, 0, JUMP_MAX_UPWARD_MOMENTUM);
            newSpeed = Vector2.ClampMagnitude(newSpeed, JUMP_MAX_RECOVERY_MOMENTUM);
            rb.velocity = newSpeed;
            rb.AddForce(Vector2.up * JUMP_FORCE, ForceMode2D.Impulse);
            isJumpReady = false;
        }
        doJump = false;
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.collider.gameObject.layer == 3) {
            Debug.Log("Groundbang");
            isJumpReady = true;
        }
        if (collision.collider.gameObject.layer == 6) {
            Debug.Log("Playerbang");
        }
    }

    void OnHpChanged() {
        float healthPercentage = (float)hp / (float)MAX_HP;
        healthbarRT.sizeDelta = new Vector2(healthPercentage, healthbarRT.sizeDelta.y);
    }

    public void OnHurtBox(Collider2D collider) {
        if (collider.gameObject.layer == 7) {
            Debug.Log("Hurtboxed");
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }

    [PunRPC]
    public void RpcSetParent(int viewID, string parent) {
        PhotonView child = PhotonNetwork.GetPhotonView(viewID);
        child.transform.SetParent(transform.Find(parent));
    }
}
