using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using System.Linq;

public abstract class Projectile : MonoBehaviourPunCallbacks, IPunObservable
{
    protected abstract string SPRITE {
        get;
    }
    protected abstract float START_SPEED {
        get;
    }
    protected abstract float ACCELERATION {
        get;
    }
    protected abstract float MAX_SPEED {
        get;
    }
    protected abstract short DAMAGE {
        get;
    }
    protected abstract float IMPACT_FORCE {
        get;
    }
    protected abstract float MAX_LIFE {
        get;
    }
    protected abstract bool DESTROY_ON_GROUND {
        get;
    }
    protected abstract bool DESTROY_ON_PLAYER {
        get;
    }
    protected abstract bool DESTROY_ON_PROJECTILE {
        get;
    }

    Rigidbody2D rb;

    float speed;
    float life;
    Vector2 target;
    bool setTarget = false;

    public override void OnEnable() {
        base.OnEnable();
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    protected virtual void Start() {
        transform.Find("Sprite").GetComponent<SpriteRenderer>().sprite = ConstantsAndHelpers.GetSprite(SPRITE);
        speed = START_SPEED;
        life = MAX_LIFE;
    }

    void Update() {
        if (!photonView.IsMine) {
            return;
        }
        float dt = Time.deltaTime;
        if (setTarget) {
            float rotation = Mathf.Atan2(target.y - transform.position.y, target.x - transform.position.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, rotation));
            setTarget = false;
        }
        speed += ACCELERATION * dt;
        rb.velocity = Vector2.ClampMagnitude((new Vector2(transform.right.x, transform.right.y) * speed), MAX_SPEED);
        life -= dt;
        if (life <= 0) {
            NetworkController.DestroyNetworkedObject(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D collider) {
        if (!photonView.IsMine) {
            return;
        }
        // Ground
        if (collider.gameObject.layer == 3) {
            if (DESTROY_ON_GROUND) {
                NetworkController.DestroyNetworkedObject(gameObject);
            }
        }
        // Player
        if (collider.gameObject.layer == 6) {
            if (collider.gameObject.GetPhotonView().IsMine) {
                return;
            }
            if (collider.gameObject.GetComponent<Character>().isInvincible == false) {
                photonView.RPC("RPCDoHit", RpcTarget.All, collider.gameObject.GetPhotonView().ViewID);
                if (DESTROY_ON_PROJECTILE) {
                    NetworkController.DestroyNetworkedObject(gameObject);
                }
            }
        }
        // Projectile
        if (collider.gameObject.layer == 7) {
            if (DESTROY_ON_PROJECTILE) {
                NetworkController.DestroyNetworkedObject(gameObject);
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) {
        } else {
        }
    }

    public virtual void SetTarget(Vector2 newTarget) {
        target = newTarget;
        setTarget = true;
    }
    
    [PunRPC]
    protected abstract void RPCDoHit(int hitChar);
}