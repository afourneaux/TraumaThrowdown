using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using System.Linq;

public class Character : MonoBehaviourPunCallbacks, IPunObservable
{
    const float MAX_SPEED = 4.0f;
    const float ACCELERATION = 15.0f;
    const float JUMP_MAX_RECOVERY_MOMENTUM = 2.0f;
    const float JUMP_MAX_UPWARD_MOMENTUM = 4.0f;
    const float JUMP_FORCE = 10f;
    const int MAX_HP = 100;

    int _hp;
    public int hp {
        get {
            return _hp;
        }
        set {
            bool isChange = _hp != value;
            _hp = value;
            if (isChange) {
                OnHpChanged();
            }
        }
    }
    public bool isInitialised = false;
    bool isJumpReady = false;
    bool doJump = false;
    Rigidbody2D rb;
    RectTransform healthbarRT;
    PlayerController player;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = transform.parent.GetComponent<PlayerController>();
        healthbarRT = transform.Find("CharacterUI/HealthBarValue").GetComponent<RectTransform>();
        transform.Find("CharacterUI/PlayerName").GetComponent<TMPro.TMP_Text>().text = photonView.Owner.NickName;
        if (photonView.IsMine) {
            hp = MAX_HP;
        }
        isInitialised = true;
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
        if (Input.GetKey(KeyCode.Space)) {
            hp -= 10;
        }
        doJump = false;
    }

    void OnCollisionEnter2D(Collision2D collision) {
        // Ground
        if (collision.collider.gameObject.layer == 3) {
            Vector3 normal = collision.collider.bounds.ClosestPoint(transform.position) - transform.position;
            Debug.Log($"Groundbang - normal Y = {normal.y}");
            if (normal.y < 0) {
                isJumpReady = true;
            }
        }
        // Player
        if (collision.collider.gameObject.layer == 6) {
            Debug.Log("Playerbang");
        }
        // Projectile
        if (collision.collider.gameObject.layer == 7) {
            Debug.Log("Hurtboxed");
        }
        // Killbox
        if (collision.collider.gameObject.layer == 9) {
            hp = -999;
        }
    }

    void OnHpChanged() {
        if (isInitialised) {
            float healthPercentage = (float)hp / (float)MAX_HP;
            healthbarRT.sizeDelta = new Vector2(healthPercentage, healthbarRT.sizeDelta.y);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) {
            stream.SendNext(hp);
        } else {
            hp = (int)stream.ReceiveNext();
        }
    }

    [PunRPC]
    public void RpcSetParent(int viewID, string parent) {
        PhotonView child = PhotonNetwork.GetPhotonView(viewID);
        child.transform.SetParent(transform.Find(parent));
    }
}
