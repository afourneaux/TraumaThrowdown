using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class SetupUIController : MonoBehaviour
{
    bool isCountdownActive = false;
    float countdown = 0f;
    TMPro.TMP_Text countdownText;

    void Start() {
        AudioController.instance.PlayMusic("menu");
        countdownText = transform.Find("/UI/Countdown").GetComponent<TMPro.TMP_Text>();
        if (PlayerController.instance == null) {
            NetworkController.SpawnNetworkedObject("Player", Vector2.zero);
        } else {
            PlayerController.instance.Reset();
        }
        NetworkController.UnlockRoom();
    }

    void Update()
    {
        float dt = Time.deltaTime;
        if (PlayerController.isDirty) {
            // Update the connection info
            int playerIndex;
            for (playerIndex = 0; playerIndex < PlayerController.AllPlayers.Count; playerIndex++) {
                PlayerController player = PlayerController.AllPlayers[playerIndex];
                GameObject playerBlock = transform.Find($"/UI/Connection/Users/User{playerIndex}").gameObject;
                playerBlock.SetActive(true);
                playerBlock.transform.Find("Ready").GetComponent<Image>().sprite = player.isReady ? ConstantsAndHelpers.GetSprite("art_green") : ConstantsAndHelpers.GetSprite("art_red");
                playerBlock.transform.Find("Name").GetComponent<TMPro.TMP_Text>().text = player.photonView.Controller.NickName;
                Image image = playerBlock.transform.Find("Character").GetComponent<Image>();
                if (player.selectedCharacter == ConstantsAndHelpers.CharacterEnum.NONE) {
                    image.enabled = false;
                } else {
                    image.enabled = true;
                    image.sprite = ConstantsAndHelpers.GetSprite(ConstantsAndHelpers.EnumToName[player.selectedCharacter]);
                }
            }
            isCountdownActive = PlayerController.AllPlayers.Count > 1 && PlayerController.AllPlayers.All(p => p.isReady);
            if (isCountdownActive) {
                countdown = ConstantsAndHelpers.START_GAMECOUNTDOWN_LENGTH;
                NetworkController.LockRoom();
            } else {
                NetworkController.UnlockRoom();
            }
            for (; playerIndex < 8; playerIndex++) {
                transform.Find($"/UI/Connection/Users/User{playerIndex}").gameObject.SetActive(false);
            }
            for (int characterIndex = 0; characterIndex < ConstantsAndHelpers.MAX_PLAYERS; characterIndex++) {
                if (PlayerController.instance.isReady) {
                    transform.Find($"/UI/CharacterSelect/CharacterSelect{characterIndex}").GetComponent<Button>().interactable = false;
                } else {
                    bool isInUse = PlayerController.AllPlayers.Any(p => p.selectedCharacter == (ConstantsAndHelpers.CharacterEnum)characterIndex);
                    transform.Find($"/UI/CharacterSelect/CharacterSelect{characterIndex}").GetComponent<Button>().interactable = !isInUse;
                }
            }
            transform.Find("/UI/Connection/ReadyBtn").GetComponent<Button>().interactable = PlayerController.instance.selectedCharacter != ConstantsAndHelpers.CharacterEnum.NONE;
            PlayerController.isDirty = false;
        } else {
            if (isCountdownActive) {
                countdown -= dt;
                countdownText.text = Mathf.CeilToInt(countdown).ToString();

                if (countdown <= 0) {
                    NetworkController.ChangeScene("GameScene");
                    isCountdownActive = false;
                }
            }
        }
        countdownText.enabled = isCountdownActive;
    }

    // TODO: Add character DEselect button
    public void CharacterSelectBtnOnClick(int selection) {
        if (PlayerController.AllPlayers.Any(p => p.selectedCharacter == (ConstantsAndHelpers.CharacterEnum)selection)) {
            // Two people selected a character at almost the same time. First one gets it
            AudioController.instance.PlaySound("UICancel");
            return;
        }
        AudioController.instance.PlaySound("UISelect");
        PlayerController.instance.selectedCharacter = (ConstantsAndHelpers.CharacterEnum)selection;
        AudioController.instance.PlayMusic($"theme_{ConstantsAndHelpers.EnumToName[(ConstantsAndHelpers.CharacterEnum)selection]}");
    }

    public void ReadyBtnOnClick() {
        PlayerController.instance.isReady = !PlayerController.instance.isReady;
        if (PlayerController.instance.isReady) {
            AudioController.instance.PlaySound("UISelect");
        } else {
            AudioController.instance.PlaySound("UICancel");
        }
    }

    public void DisconnectBtnOnClick() {
        AudioController.instance.PlaySound("UICancel");
        NetworkController.Disconnect();
    }
}
