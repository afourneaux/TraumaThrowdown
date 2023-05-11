using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Photon.Pun;

public class GameController : MonoBehaviour
{
    private const bool DEBUG_ALLOW_GAME_OVER = false;
    public static GameController instance;
    public GameObject ScoreDisplayPrefab;
    public GameObject LifeIconPrefab;
    public GameObject LifeNumberPrefab;
    public GameObject VictoryScreen;
    bool isGameOver = false;
    List<Vector2> Spawners;
    List<float> SpawnerCooldowns;
    Dictionary<PlayerController, GameObject> PlayerLives;

    void OnEnable() {
        if (instance == null) {
            instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    void Start() {
        //AudioController.instance.StopMusic();
        AudioController.instance.PlayMusic("battle_theme");
        Transform PlayerLivesUI = transform.Find("/UI/PlayerLives");
        PlayerLives = new Dictionary<PlayerController, GameObject>();
        foreach (PlayerController player in PlayerController.AllPlayers) {
            GameObject go = Instantiate(ScoreDisplayPrefab, PlayerLivesUI);
            go.transform.Find("PlayerName").GetComponent<TMPro.TMP_Text>().text = player.photonView.Owner.NickName;
            PlayerLives.Add(player, go);
        }

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
        if (isGameOver && DEBUG_ALLOW_GAME_OVER) {
            return;
        }
        int playersAlive = 0;
        foreach (PlayerController player in PlayerController.AllPlayers) {
            if (player.isLivesDirty) {
                player.isLivesDirty = false;
                GameObject container = PlayerLives[player];
                Transform iconContainer = container.transform.Find("Icons");
                foreach (Transform child in iconContainer) {
                    Destroy(child.gameObject);
                }
                if (player.lives > 3) {
                    GameObject go = Instantiate(LifeIconPrefab, iconContainer);
                    go.transform.GetComponent<Image>().sprite = ConstantsAndHelpers.GetSprite(player.selectedCharacter);
                    GameObject textGO = Instantiate(LifeNumberPrefab, iconContainer);
                    textGO.GetComponent<TMPro.TMP_Text>().text = player.lives.ToString();
                } else if (player.lives > 0) {
                    Sprite sprite = ConstantsAndHelpers.GetSprite(player.selectedCharacter);
                    for (int i = 0; i < player.lives; i++) {
                        GameObject go = Instantiate(LifeIconPrefab, iconContainer);
                        go.transform.GetComponent<Image>().sprite = ConstantsAndHelpers.GetSprite(player.selectedCharacter);
                    }
                } else {
                    container.transform.Find("isDead").gameObject.SetActive(true);
                }
            }
            if (player.lives > 0) {
                playersAlive++;
            }
        }

        if (playersAlive <= 1 && DEBUG_ALLOW_GAME_OVER) {
            isGameOver = true;
            PlayerController winner = PlayerController.AllPlayers.First(p => p.lives > 0);
            if (PlayerController.instance.character != null) {
                NetworkController.DestroyNetworkedObject(PlayerController.instance.character.gameObject);
            }
            VictoryScreen.SetActive(true);
            VictoryScreen.transform.Find("Panel/Image").GetComponent<Image>().sprite = ConstantsAndHelpers.GetSprite(winner.selectedCharacter, true);
            VictoryScreen.transform.Find("Panel/Text").GetComponent<TMPro.TMP_Text>().text = $"Congratulations {winner.photonView.Owner.NickName}!";
            if (PhotonNetwork.LocalPlayer.IsMasterClient == false) {
                VictoryScreen.transform.Find("Panel/Button").GetComponent<Button>().interactable = false;
                VictoryScreen.transform.Find("Panel/Button/Text").GetComponent<TMPro.TMP_Text>().text = $"Waiting for host {PhotonNetwork.MasterClient.NickName}";
            }
            AudioController.instance.PlayMusic("VictoryFanfare");
            return;
        }

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
        AudioController.instance.PlaySound("UISelect");
        NetworkController.ChangeScene("SetupScene");
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
