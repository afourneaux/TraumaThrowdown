using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon.Realtime;
using Photon.Pun;

public class Willa : Character
{
    protected override float JUMP_FORCE => 12f;
    protected override string WALK_SFX => "StepsSlow";
    protected override ConstantsAndHelpers.CharacterEnum character => ConstantsAndHelpers.CharacterEnum.WILLA;

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

        if (attackCooldown <= 0 && Input.GetMouseButtonUp(0)) {
            GameObject projectileGO = NetworkController.SpawnNetworkedObject("projectile", new Vector2(transform.position.x, transform.position.y));
            photonView.RPC("RPCConfigureProjectile", RpcTarget.All, projectileGO.GetPhotonView().ViewID, (int)ConstantsAndHelpers.ProjectileType.FIREBOLT);
            Firebolt firebolt = projectileGO.AddComponent<Firebolt>();
            Vector3 mousePositionV3 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePosition = new Vector2(mousePositionV3.x, mousePositionV3.y);
            firebolt.SetTarget(mousePosition);
            attackCooldown = ATTACK_COOLDOWN;
        }

        // Special ability: Teleport
        if (specialCooldown <= 0 && Input.GetMouseButtonUp(1)) {
            Vector3 mousePositionV3 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePosition = new Vector2(mousePositionV3.x, mousePositionV3.y);
            Vector2 characterPosition = new Vector2(transform.position.x, transform.position.y);
            Vector2 teleportOffset = (mousePosition - characterPosition);
            if (teleportOffset.magnitude >= ConstantsAndHelpers.WILLA_TELEPORT_MIN_DISTANCE) {
                photonView.RPC("RPCDisplayEffect", RpcTarget.All, characterPosition.x, characterPosition.y);
                teleportOffset = Vector2.ClampMagnitude(teleportOffset, ConstantsAndHelpers.WILLA_TELEPORT_MAX_DISTANCE);
                transform.position += new Vector3(teleportOffset.x, teleportOffset.y);

                specialCooldown = ConstantsAndHelpers.WILLA_TELEPORT_COOLDOWN;
            }
        }
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

    [PunRPC]
    void RPCDisplayEffect(float x, float y) {
        GameObject effectGO = Instantiate(PlayerController.instance.EffectPrefab, new Vector3(x, y, 0), Quaternion.identity);
        Effect.AddEffectComponent(ConstantsAndHelpers.EffectType.TELEPORT, effectGO);
    }
}