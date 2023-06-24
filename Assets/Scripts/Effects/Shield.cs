using UnityEngine;

public class Shield : Effect {

    protected override void Start() {
        base.Start();
        sr.sprite = ConstantsAndHelpers.GetSprite("art_green");
    }

    protected override void Update() {
        base.Update();
        if (timeAlive >= ConstantsAndHelpers.WATSON_SHIELD_DURATION) {
            Destroy(gameObject);
            return;
        }
        float opacity = Mathf.Lerp(ConstantsAndHelpers.WATSON_SHIELD_VISIBILITY_FACTOR_START,
                                    ConstantsAndHelpers.WATSON_SHIELD_VISIBILITY_FACTOR_END,
                                    timeAlive / ConstantsAndHelpers.WATSON_SHIELD_DURATION);
        sr.color = new Color(1, 1, 1, opacity);
    }
}