using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon.Realtime;
using Photon.Pun;

public class Scream : Character
{
    protected override float JUMP_FORCE => 12f;
    protected override string WALK_SFX => "StepsVeryFast";
    protected override ConstantsAndHelpers.CharacterEnum character => ConstantsAndHelpers.CharacterEnum.SCREAM;
    float invisibilityTimer;

    protected override void Start() {
        specialCooldown = ConstantsAndHelpers.SCREAM_SPECIAL_COOLDOWN;
        base.Start();
    }

    protected override void Update() {
        base.Update();
        if (isSpecialActive) {
            if (photonView.IsMine) {
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, sr.color.a * ConstantsAndHelpers.SCREAM_SELF_INVIS_FACTOR);
            } else {
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0);
                isUiVisible = false;
            }
        }
        if (!photonView.IsMine) {
            return;
        }
        if (GlobalUI.isMenuOpen) {
            return;
        }

        specialCooldown -= Time.deltaTime;
        invisibilityTimer -= Time.deltaTime;

        if (attackCooldown <= 0 && Input.GetMouseButtonUp(0)) {
            GameObject projectileGO = NetworkController.SpawnNetworkedObject("projectile", new Vector2(transform.position.x, transform.position.y));
            photonView.RPC("RPCConfigureProjectile", RpcTarget.All, projectileGO.GetPhotonView().ViewID, (int)ConstantsAndHelpers.ProjectileType.FIREBOLT);
            Firebolt firebolt = projectileGO.AddComponent<Firebolt>();
            Vector3 mousePositionV3 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePosition = new Vector2(mousePositionV3.x, mousePositionV3.y);
            firebolt.SetTarget(mousePosition);
            attackCooldown = ATTACK_COOLDOWN;
            EndInvisibility();
        }

        // Special ability: invisibility
        if (specialCooldown <= 0 && !isSpecialActive && Input.GetMouseButtonUp(1)) {
            invisibilityTimer = ConstantsAndHelpers.SCREAM_SPECIAL_DURATION;
            isSpecialActive = true;
        }
        if (invisibilityTimer <= 0) {
            EndInvisibility();
        }
    }

    void EndInvisibility() {
        if (isSpecialActive) {
            isSpecialActive = false;
            specialCooldown = ConstantsAndHelpers.SCREAM_SPECIAL_COOLDOWN;
        }
    }

    protected override void TakeDamage(short damage) {
        base.TakeDamage(damage);
        EndInvisibility();
    }

    [PunRPC]
    void RPCConfigureProjectile(int projectileID, int projectileType) {
        if (photonView.IsMine) {
            return;
        }
        GameObject projectileGO = PhotonNetwork.GetPhotonView(projectileID).gameObject;
        switch((ConstantsAndHelpers.ProjectileType)projectileType) {
            case ConstantsAndHelpers.ProjectileType.FIREBOLT:
                projectileGO.AddComponent<Firebolt>();
                break;
            default:
                Debug.LogError($"Unrecognised projectile type - {projectileType}");
                break;
        }
    }
}