using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Character character;

    void Start()
    {
        SpawnCharacter();
    }

    void Update()
    {
        
    }

    void SpawnCharacter() {
        if (character != null) {
            Debug.LogError("Player is trying to spawn a character but already has one!");
            return;
        }
        GameObject go = NetworkController.instance.SpawnNetworkedObject("Character", new Vector2(Random.Range(-5f, 5f), 0.5f), transform);
        character = go.GetComponent<Character>();
    }

    public void KillCharacter() {
        Destroy(character.gameObject);
        character = null;
    }
}
