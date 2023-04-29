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

    int hp;
    bool isJumpReady = false;
    bool doJump = false;
    Rigidbody2D rb;
    RectTransform healthbarRT;

    float _debugHP = (float)MAX_HP;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        healthbarRT = transform.Find("CharacterUI/HealthBarValue").GetComponent<RectTransform>();
        hp = MAX_HP;
    }

    void Update()
    {
        if (hp <= 0) {
            transform.parent.GetComponent<PlayerController>().KillCharacter();
            return;
        }
        float healthPercentage = (float)hp / (float)MAX_HP;
        healthbarRT.sizeDelta = new Vector2(healthPercentage, healthbarRT.sizeDelta.y);
        float dt = Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.W)) {
            doJump = true;
        }
        _debugHP = _debugHP - (dt * 20);
        hp = Mathf.FloorToInt(_debugHP);
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

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
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

    public void OnHurtBox(Collider2D collider) {
        if (collider.gameObject.layer == 7) {
            Debug.Log("Hurtboxed");
        }
    }
}
