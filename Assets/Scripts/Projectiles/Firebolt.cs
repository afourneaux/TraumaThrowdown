using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon.Realtime;
using Photon.Pun;

public class Firebolt : Projectile
{
    protected override string SPRITE => "firebolt";
    protected override float START_SPEED => 6f;
    protected override float ACCELERATION => 3f;
    protected override float MAX_SPEED => 10f;
    protected override short DAMAGE => 20;
    protected override float IMPACT_FORCE => 1f;
    protected override float MAX_LIFE => 1f;
    protected override bool DESTROY_ON_GROUND => true;
    protected override bool DESTROY_ON_PLAYER => true;
    protected override bool DESTROY_ON_PROJECTILE => true;

    protected override void Start() {
        AudioController.instance.PlaySound("Fireball", false, photonView.IsMine ? 1f : 0.5f);
        base.Start();
    }

    [PunRPC]
    protected override void RPCDoHit(int hitChar) {
        Character character = PhotonNetwork.GetPhotonView(hitChar).gameObject.GetComponent<Character>();
        bool isInvolved = photonView.IsMine || character.photonView.IsMine;
        int hurtSoundIndex = Random.Range(1, 4);
        AudioController.instance.PlaySound($"hit{hurtSoundIndex}", false, isInvolved ? 1f : 0.5f);
        character.OnHit(DAMAGE, (character.transform.position - transform.position) * IMPACT_FORCE);
    }
}