using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using System.Linq;

public abstract class Character : MonoBehaviourPunCallbacks
{
    protected virtual float MAX_SPEED => 4.0f;
    protected virtual float ACCELERATION => 15.0f;
    protected virtual float JUMP_MAX_RECOVERY_MOMENTUM => 2.0f;
    protected virtual float JUMP_MAX_UPWARD_MOMENTUM => 4.0f;
    protected virtual float JUMP_FORCE => 10f;
    protected virtual short MAX_HP => 100;
    protected virtual float ATTACK_COOLDOWN => 0.7f;
    protected virtual string WALK_SFX => "StepsMid";

    protected virtual ConstantsAndHelpers.CharacterEnum character {
        get { return ConstantsAndHelpers.CharacterEnum.NONE; }
    }

    public virtual bool IsSpecialActive() {
        return false;
    }

    short _hp;
    public short hp {
        get {
            return _hp;
        }
        set {
            if (_hp != value) {
                OnHpChanged(_hp, value);
            }
            _hp = value;
        }
    }
    public bool isInitialised = false;
    bool isJumpReady = false;
    bool doJump = false;
    public Rigidbody2D rb;
    RectTransform healthbarRT;
    public PlayerController player;
    protected SpriteRenderer sr;
    protected float attackCooldown = 0;
    protected float iframeTimer = 0f;
    public bool isInvincible = false;
    public bool faceLeft = false;
    private bool isAirborne = true;
    private string walkSfxId = null;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        healthbarRT = transform.Find("CharacterUI/HealthBarValue").GetComponent<RectTransform>();
        transform.Find("CharacterUI/PlayerName").GetComponent<TMPro.TMP_Text>().text = photonView.Owner.NickName;
        if (photonView.IsMine) {
            hp = MAX_HP;
        } else {
            rb.bodyType = RigidbodyType2D.Static;
        }
        sr = transform.Find("Sprite").GetComponent<SpriteRenderer>();
        sr.sprite = ConstantsAndHelpers.GetSprite(character);
        isInitialised = true;
    }

    protected virtual void Update()
    {
        if (isInvincible) {
            sr.color = new Color(1f, .8f, .8f, .6f);
        } else {
            sr.color = Color.white;
        }
        sr.flipX = faceLeft;
        if (!photonView.IsMine) {
            return;
        }
        
        float dt = Time.deltaTime;
        attackCooldown -= dt;
        iframeTimer -= dt;
        isInvincible = iframeTimer > 0;
        
        if (GlobalUI.isMenuOpen) {
            return;
        }

        if (Input.GetKeyDown(KeyCode.W)) {
            doJump = true;
        }
    }

    void FixedUpdate() 
    {
        if (!photonView.IsMine) {
            return;
        }
        
        if (GlobalUI.isMenuOpen) {
            return;
        }
        
        bool isMoving = false;

        if (Input.GetKey(KeyCode.A)) {
            faceLeft = true;
            Vector2 newSpeed = rb.velocity + Vector2.left * Time.fixedDeltaTime * ACCELERATION;
            newSpeed.x = Mathf.Max(newSpeed.x, -MAX_SPEED);
            rb.velocity = newSpeed;
            isMoving = true;
        }
        if (Input.GetKey(KeyCode.D)) {
            faceLeft = false;
            Vector2 newSpeed = rb.velocity + Vector2.right * Time.fixedDeltaTime * ACCELERATION;
            newSpeed.x = Mathf.Min(newSpeed.x, MAX_SPEED);
            rb.velocity = newSpeed;
            isMoving = true;
        }
        if (isMoving && !isAirborne) {
            if (string.IsNullOrEmpty(walkSfxId)) {
                walkSfxId = AudioController.instance.PlaySound(WALK_SFX, true);
            }
        } else {
            if (!string.IsNullOrEmpty(walkSfxId)) {
                AudioController.instance.StopByID(walkSfxId);
                walkSfxId = null;
            }
        }
        if (doJump && isJumpReady) {
            Vector2 newSpeed = rb.velocity;
            newSpeed.y = Mathf.Clamp(newSpeed.y, 0, JUMP_MAX_UPWARD_MOMENTUM);
            newSpeed = Vector2.ClampMagnitude(newSpeed, JUMP_MAX_RECOVERY_MOMENTUM);
            rb.velocity = newSpeed;
            rb.AddForce(Vector2.up * JUMP_FORCE, ForceMode2D.Impulse);
            isJumpReady = false;
            AudioController.instance.PlaySound("JumpRise");
        }
        doJump = false;
    }

    void OnCollisionEnter2D(Collision2D collision) {
        // Ground
        if (collision.collider.gameObject.layer == 3) {
            Vector3 normal = collision.collider.bounds.ClosestPoint(transform.position) - transform.position;
            if (normal.y <= 0) {
                AudioController.instance.PlaySound("JumpLand", false, photonView.IsMine ? 1f : 0.5f);
                isJumpReady = true;
                isAirborne = false;
            }
        }
        // Player
        if (collision.collider.gameObject.layer == 6) {
            Debug.Log("Playerbang");
        }
        // Projectile
        if (collision.collider.gameObject.layer == 7) {
            if (collision.collider.gameObject.GetPhotonView().IsMine) {
                return;
            }
            Debug.Log("Hurtboxed");
        }
        // Killbox
        if (collision.collider.gameObject.layer == 9) {
            Debug.Log("Killzoned");
            hp = -999;
        }
    }

    void OnCollisionExit2D() {
        isAirborne = true;
    }

    protected virtual void OnHpChanged(short oldHP, short newHP) {
        if (isInitialised) {
            float healthPercentage = (float)newHP / (float)MAX_HP;
            healthbarRT.sizeDelta = new Vector2(healthPercentage, healthbarRT.sizeDelta.y);
        }
    }

    public void StartIFrame() {
        iframeTimer = ConstantsAndHelpers.IFRAME_DURATION;
    }

    void OnDestroy() {
        if (!string.IsNullOrEmpty(walkSfxId)) {
            AudioController.instance.StopByID(walkSfxId);
        }
    }
}
