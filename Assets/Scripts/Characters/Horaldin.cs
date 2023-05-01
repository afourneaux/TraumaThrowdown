using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon.Realtime;
using Photon.Pun;

public class Horaldin : Character
{
    protected override float JUMP_FORCE => 12f;
    protected override ConstantsAndHelpers.CharacterEnum character => ConstantsAndHelpers.CharacterEnum.HORALDIN;

    protected override void Start() {
        base.Start();
    }

    protected override void Update() {
        base.Update();
        if (!photonView.IsMine) {
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