using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstantsAndHelpers
{
    
    public enum CharacterEnum {
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

    public const int MAX_PLAYERS = 8;
    public const float SPAWNER_COOLDOWN = 10f;
    public const float RESPAWN_DELAY = 5f;
    public const int START_LIVES = 3;
    public const float COUNTDOWN_LENGTH = 5f;

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
}