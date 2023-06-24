using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public abstract class Effect : MonoBehaviour
{
    protected Character character;
    protected SpriteRenderer sr;
    protected float timeAlive = 0f;

    protected virtual void Start() {
        character = GetComponentInParent<Character>();
        sr = GetComponent<SpriteRenderer>();
    }
    
    protected virtual void Update() {
        timeAlive += Time.deltaTime;
    }

    public static void AddEffectComponent(ConstantsAndHelpers.EffectType effectType, GameObject parent) {
        Effect newEffect;
        switch(effectType) {
            case ConstantsAndHelpers.EffectType.SHIELD:
                newEffect = parent.AddComponent<Shield>();
                break;
            case ConstantsAndHelpers.EffectType.TELEPORT:
                newEffect = parent.AddComponent<Teleport>();
                break;
            default:
                Debug.LogError($"Unrecognised effect ID: {effectType}");
                return;
        }
    }
}