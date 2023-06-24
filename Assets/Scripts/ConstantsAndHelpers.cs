using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    TODO LIST OF THINGS TODO
    Implement more audio - countdown beeps, footsteps, Jump/Fall, etc
    Levels and level selection
    Unique character abilities
    Camera control
    Score on lobby page
    Art/Animation
    Menu design
*/


public class ConstantsAndHelpers
{
    public enum CharacterEnum {
        NONE = -1,
        HORALDIN = 0,
        ISIS = 1,
        LOUIS = 2,
        OSIRIS = 3,
        SCREAM = 4,
        VAKIR = 5,
        WATSON = 6,
        WILLA = 7
    }
    public enum RespawnState {
        NONE = -1,
        COUNTDOWN = 0,
        NOW = 1
    }
    public enum ProjectileType {
        NONE = -1,
        FIREBOLT = 0
    }
    public enum EffectType {
        NONE = -1,
        SHIELD = 0
    }

    // DEBUG VALUES
    public const bool DEBUG_ALLOW_SOLO_PLAY = true;
    public const bool DEBUG_BLOCK_GAME_OVER = true;
    
    // GLOBAL VALUES
    public const int MAX_PLAYERS = 8;
    public const float SPAWNER_COOLDOWN = 10f;
    public const float SPAWNER_SAFE_RADIUS = 2f;
    public const float RESPAWN_DELAY = 3f;
    public const int START_LIVES = 3;
    public const float START_GAMECOUNTDOWN_LENGTH = 5f;
    public const float IFRAME_DURATION = 0.2f;

    // CHARACTER SPECIFIC
    // HORALDIN
    // ISIS
    // LOUIS
    // OSIRIS
    // SCREAM
    public const float SCREAM_SELF_INVIS_FACTOR = 0.2f;
    public const float SCREAM_SPECIAL_DURATION = 3f;
    public const float SCREAM_SPECIAL_COOLDOWN = 10f;
    // VAKIR
    // WATSON
    public const float WATSON_SHIELD_VISIBILITY_FACTOR_START = 0.7f;
    public const float WATSON_SHIELD_VISIBILITY_FACTOR_END = 0.2f;
    public const float WATSON_SHIELD_DURATION = 1f;
    public const float WATSON_SHIELD_COOLDOWN = 4f;
    // WILLA



    public static Dictionary<ConstantsAndHelpers.CharacterEnum, string> EnumToName = new Dictionary<ConstantsAndHelpers.CharacterEnum, string>() {
        { CharacterEnum.HORALDIN, "horaldin" },
        { CharacterEnum.ISIS, "isis" },
        { CharacterEnum.LOUIS, "louis" },
        { CharacterEnum.OSIRIS, "osiris" },
        { CharacterEnum.SCREAM, "scream" },
        { CharacterEnum.VAKIR, "vakir" },
        { CharacterEnum.WATSON, "watson" },
        { CharacterEnum.WILLA, "willa" },
        { CharacterEnum.NONE, null }
    };

    public static string GetFullPathToTransform(Transform transform) {
        string path = "/" + transform.name;
        while (transform.parent != null) {
            transform = transform.parent;
            path = "/" + transform.name + path;
        }
        return path;
    }

    public static Sprite GetSprite(string key) {
        Sprite sprite = Resources.Load<Sprite>($"Sprites/{key}");
        if (sprite == null) {
            Debug.LogError($"Sprite not found: {key}");
        }
        return sprite;
    }

    public static Sprite GetSprite(CharacterEnum key, bool victory = false) {
        string spriteKey = EnumToName[key];
        if (victory) {
            spriteKey = $"victory_{spriteKey}";
        }
        return GetSprite(spriteKey);
    }
}