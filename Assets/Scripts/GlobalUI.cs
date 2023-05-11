using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalUI : MonoBehaviour
{
    GameObject PauseScreen;
    string currentScene;
    Transform quitBtnTransform;
    public static bool isMenuOpen = false;

    void Start()
    {
        PauseScreen = transform.Find("PauseMenu").gameObject;
        SceneManager.activeSceneChanged += OnSceneChanged;
        quitBtnTransform = transform.Find("PauseMenu/QuitBtn");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            SetPauseActive(!isMenuOpen);
        }
        
    }

    public void ClosePauseMenu() {
        SetPauseActive(false);
    }

    public void OpenPauseMenu() {
        SetPauseActive(true);
    }

    public void ChangeMasterVolume(float newValue) {
        AudioController.instance.ChangeMasterVolume(newValue);
    }

    public void ChangeMusicVolume(float newValue) {
        AudioController.instance.ChangeMusicVolume(newValue);
    }

    public void ChangeSoundVolume(float newValue) {
        AudioController.instance.ChangeSoundVolume(newValue);
    }

    public void QuitGame() {
        AudioController.instance.PlaySound("UICancel");
        isMenuOpen = false;
        SceneManager.activeSceneChanged -= OnSceneChanged;
        NetworkController.Disconnect();
        Destroy(gameObject);
    }

    void SetPauseActive(bool state) {
        isMenuOpen = state;
        PauseScreen.SetActive(state);
        if (state) {
            AudioController.instance.PlaySound("UISelect");
        } else {
            AudioController.instance.PlaySound("UICancel");
        }
    }

    void OnSceneChanged(Scene scene1, Scene scene2) {
        Canvas canvas = transform.GetComponent<Canvas>();
        canvas.worldCamera = Camera.main;
        quitBtnTransform.gameObject.SetActive(scene2.name != "MainMenuScene");
    }
}
