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
        NONE = 0,
        COUNTDOWN = 1,
        NOW = 2
    }
    public enum ProjectileType {
        NONE = 0,
        FIREBOLT = 1
    }

    public const int MAX_PLAYERS = 8;
    public const float SPAWNER_COOLDOWN = 10f;
    public const float SPAWNER_SAFE_RADIUS = 2f;
    public const float RESPAWN_DELAY = 3f;
    public const int START_LIVES = 3;
    public const float START_GAMECOUNTDOWN_LENGTH = 5f;
    public const float IFRAME_DURATION = 0.2f;
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