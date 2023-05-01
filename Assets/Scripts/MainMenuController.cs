using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    void Start() {
        if (NetworkController.instance != null) {
            if (NetworkController.instance.nameToBlame != "") {
                TMPro.TMP_Text errorText = transform.Find("/UI/Message").GetComponent<TMPro.TMP_Text>();
                errorText.text = $"Disconnected by user quitting: {NetworkController.instance.nameToBlame}";
                errorText.gameObject.SetActive(true);
            }
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
        AudioController.instance.PlaySound("UISelect");
        SceneManager.LoadScene("LobbyScene");
    }
}
