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

        foreach (PlayerController player in PlayerController.AllPlayers) {
            SpawnCharacter(player);
        }
    }

    void Update() {
        if (!PhotonNetwork.IsMasterClient) {
            return;
        }
        float dt = Time.deltaTime;
        for (int i = 0; i < ConstantsAndHelpers.MAX_PLAYERS; i++) {
            SpawnerCooldowns[i] -= dt;
        }
        foreach (PlayerController player in PlayerController.AllPlayers.Where(p => p.character == null && p.respawnState == ConstantsAndHelpers.RespawnState.NOW)) {
            SpawnCharacter(player);
        }
    }

    public void EndMatch() {
        NetworkController.ChangeScene("SetupScene");
        instance.isGameOn = false;
    }

    // TODO - make sure we don't spawn where someone is already standing
    public void SpawnCharacter(PlayerController player) {
        int locationIndex;
        IEnumerable<int> validSpawners = SpawnerCooldowns.Where(s => s <= 0).Select(s => SpawnerCooldowns.IndexOf(s));
        if (validSpawners.Any()) {
            locationIndex = Random.Range(0, validSpawners.Count() - 1);
        } else {
            locationIndex = SpawnerCooldowns.IndexOf(SpawnerCooldowns.Min());
        }
        SpawnerCooldowns[locationIndex] = ConstantsAndHelpers.SPAWNER_COOLDOWN;
        player.respawnState = ConstantsAndHelpers.RespawnState.NONE;
        player.photonView.RPC("RPCSpawnCharacter", RpcTarget.All, Spawners[locationIndex].x, Spawners[locationIndex].y);
    }
}
