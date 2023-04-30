using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Photon.Pun;

public class GameController : MonoBehaviour
{
    public static GameController instance;
    Transform PlayerHolderTransform;
    bool isGameOn = false;
    List<Vector2> Spawners;
    List<float> SpawnerCooldowns;

    void OnEnable() {
        if (instance == null) {
            instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    void Start() {
        if (!PhotonNetwork.IsMasterClient) {
            return;
        }
        Spawners = new List<Vector2>();
        SpawnerCooldowns = new List<float>();
        for (int i = 0; i < ConstantsAndHelpers.MAX_PLAYERS; i++) {
            Vector3 position = transform.Find($"/Level/SpawnPoints/Spawner{i}").transform.position;
            Spawners.Add(new Vector2(position.x, position.y));
            SpawnerCooldowns.Add(0f);
        }
        List<Vector2> mixedSpawners = new List<Vector2>(Spawners).OrderBy(p => Random.Range(0f, 1f)).ToList();

        for (int i = 0; i < PlayerController.AllPlayers.Count; i++) {
            SpawnCharacter(PlayerController.AllPlayers[i], mixedSpawners[i]);
        }
    }

    void Update() {
        float dt = Time.deltaTime;
        for (int i = 0; i < ConstantsAndHelpers.MAX_PLAYERS; i++) {
            SpawnerCooldowns[i] -= dt;
        }
        if (PlayerController.instance.character != null) {
            if (PlayerController.instance.character.hp <= 0) {
                KillCharacter(PlayerController.instance);
            }
        }
    }

    public void EndMatch() {
        NetworkController.ChangeScene("SetupScene");
        instance.isGameOn = false;
    }

    public static void SpawnCharacter(PlayerController owner, Vector2 location) {
        NetworkController.SpawnNetworkedObject("Character", location, owner.transform);

    }

    public void KillCharacter(PlayerController owner) {
        if (owner.photonView.IsMine == true) {
            NetworkController.DestroyNetworkedObject(owner.character.gameObject);
        }
        owner.character = null;
        owner.lives--;
        if (owner.lives > 0) {
            owner.needsRespawn = true;
            owner.respawnTimer = ConstantsAndHelpers.RESPAWN_DELAY;
        }
    }
}
