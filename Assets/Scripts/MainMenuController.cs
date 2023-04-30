using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    void Start() {
        if (NetworkController.instance != null) {
            Destroy(NetworkController.instance.gameObject);
            NetworkController.instance = null;
        }
        if (GameController.instance != null) {
            Destroy(GameController.instance.gameObject);
            GameController.instance = null;
        }
        if (PlayerController.instance != null) {
            Destroy(PlayerController.instance.gameObject);
            PlayerController.instance = null;
        }
        AudioController.instance.PlayMusic("main_theme");
    }

    public void OnGoBtnClicked() {
        SceneManager.LoadScene("LobbyScene");
    }
}
