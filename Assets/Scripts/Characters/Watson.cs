using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon.Realtime;
using Photon.Pun;

public class Watson : Character
{
    protected override float JUMP_FORCE => 12f;
    protected override ConstantsAndHelpers.CharacterEnum character => ConstantsAndHelpers.CharacterEnum.WATSON;
    float shieldTimer = 0f;

    protected override void Start() {
        base.Start();
    }

    protected override void Update() {
        base.Update();
        if (!photonView.IsMine) {
            return;
        }
        if (GlobalUI.isMenuOpen) {
            return;
        }

        specialCooldown -= Time.deltaTime;
        shieldTimer -= Time.deltaTime;

        if (attackCooldown <= 0 && Input.GetMouseButtonUp(0)) {
            Vector3 mousePositionV3 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            GameObject projectileGO = NetworkController.SpawnNetworkedObject("projectile", new Vector2(transform.position.x, transform.position.y));
            photonView.RPC("RPCConfigureProjectile", RpcTarget.Others, projectileGO.GetPhotonView().ViewID);
            Firebolt firebolt = projectileGO.AddComponent<Firebolt>();
            firebolt.SetTarget(new Vector2(mousePositionV3.x, mousePositionV3.y));
            attackCooldown = ATTACK_COOLDOWN;
        }

        // Special ability: shield
        if (specialCooldown <= 0 && !isSpecialActive && Input.GetMouseButtonUp(1)) {
            photonView.RPC("RPCDisplayEffect", RpcTarget.All);
            shieldTimer = ConstantsAndHelpers.WATSON_SHIELD_DURATION;
            isSpecialActive = true;
        }
        if (shieldTimer <= 0 && isSpecialActive) {
            isSpecialActive = false;
            specialCooldown = ConstantsAndHelpers.WATSON_SHIELD_COOLDOWN;
        }
    }

    protected override void TakeDamage(short damage) {
        if (!isSpecialActive) {
            base.TakeDamage(damage);
        }
    }

    [PunRPC]
    void RPCConfigureProjectile(int projectileID) {
        GameObject projectileGO = PhotonNetwork.GetPhotonView(projectileID).gameObject;
        Firebolt firebolt = projectileGO.AddComponent<Firebolt>();
    }

    [PunRPC]
    void RPCDisplayEffect() {
        GameObject effectGO = Instantiate(PlayerController.instance.EffectPrefab, transform);
        Effect.AddEffectComponent(ConstantsAndHelpers.EffectType.SHIELD, effectGO);
    }
}