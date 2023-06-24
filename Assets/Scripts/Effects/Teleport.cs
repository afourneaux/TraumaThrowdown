using UnityEngine;

public class Teleport : Effect {

    protected override void Start() {
        base.Start();
        sr.sprite = ConstantsAndHelpers.GetSprite("art_green");
    }

    protected override void Update() {
        base.Update();
        if (timeAlive >= ConstantsAndHelpers.WILLA_TELEPORT_EFFECT_DURATION) {
            Destroy(gameObject);
            return;
        }
        float lifetime = timeAlive / ConstantsAndHelpers.WILLA_TELEPORT_EFFECT_DURATION;
        float factor;
        if (lifetime < 0.5) {
            factor = lifetime * 2;
        } else {
            factor = 1 - ((lifetime - 0.5f) * 2);
        }
        sr.color = new Color(1, 1, 1, factor);
        transform.localScale = new Vector3(factor, factor, 1);
    }
}