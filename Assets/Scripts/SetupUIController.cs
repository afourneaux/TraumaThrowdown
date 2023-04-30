using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class SetupUIController : MonoBehaviour
{
    Dictionary<ConstantsAndHelpers.CharacterEnum, string> SpriteNames;
    bool isCountdownActive = false;
    float countdown = 0f;
    TMPro.TMP_Text countdownText;

    void OnEnable() {
        SpriteNames = new Dictionary<ConstantsAndHelpers.CharacterEnum, string>();
        SpriteNames.Add(ConstantsAndHelpers.CharacterEnum.HORALDIN, "horaldin");
        SpriteNames.Add(ConstantsAndHelpers.CharacterEnum.ISIS, "isis");
        SpriteNames.Add(ConstantsAndHelpers.CharacterEnum.LOUIS, "louis");
        SpriteNames.Add(ConstantsAndHelpers.CharacterEnum.OSIRIS, "osiris");
        SpriteNames.Add(ConstantsAndHelpers.CharacterEnum.SCREAM, "scream");
        SpriteNames.Add(ConstantsAndHelpers.CharacterEnum.VAKIR, "vakir");
        SpriteNames.Add(ConstantsAndHelpers.CharacterEnum.WATSON, "watson");
        SpriteNames.Add(ConstantsAndHelpers.CharacterEnum.WILLA, "willa");
    }

    void Start() {
        AudioController.instance.PlayMusic("menu_a");
        countdownText = transform.Find("/UI/Countdown").GetComponent<TMPro.TMP_Text>();
        NetworkController.SpawnNetworkedObject("Player", Vector2.zero);
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
                playerBlock.transform.Find("Character").GetComponent<Image>().sprite = player.hasSelectedCharacter ? ConstantsAndHelpers.GetSprite(SpriteNames[player.selectedCharacter]) : null;
            }
            isCountdownActive = PlayerController.AllPlayers.All(p => p.isReady);
            if (isCountdownActive) {
                countdown = ConstantsAndHelpers.COUNTDOWN_LENGTH;
            }
            for (; playerIndex < 8; playerIndex++) {
                transform.Find($"/UI/Connection/Users/User{playerIndex}").gameObject.SetActive(false);
            }
            for (int characterIndex = 0; characterIndex < ConstantsAndHelpers.MAX_PLAYERS; characterIndex++) {
                if (PlayerController.instance.isReady) {
                    transform.Find($"/UI/CharacterSelect/CharacterSelect{characterIndex}").GetComponent<Button>().interactable = false;
                } else {
                    bool isInUse = PlayerController.AllPlayers.Any(p => p.hasSelectedCharacter && p.selectedCharacter == (ConstantsAndHelpers.CharacterEnum)characterIndex);
                    transform.Find($"/UI/CharacterSelect/CharacterSelect{characterIndex}").GetComponent<Button>().interactable = !isInUse;
                }
            }
            transform.Find("/UI/Connection/ReadyBtn").GetComponent<Button>().interactable = PlayerController.instance.hasSelectedCharacter;
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

    public void CharacterSelectBtnOnClick(int selection) {
        if (PlayerController.AllPlayers.Any(p => p.hasSelectedCharacter && p.selectedCharacter == (ConstantsAndHelpers.CharacterEnum)selection)) {
            // Two people selected a character at almost the same time. First one gets it
            return;
        }
        PlayerController.instance.hasSelectedCharacter = true;
        PlayerController.instance.selectedCharacter = (ConstantsAndHelpers.CharacterEnum)selection;
    }

    public void ReadyBtnOnClick() {
        PlayerController.instance.isReady = !PlayerController.instance.isReady;
    }

    public void DisconnectBtnOnClick() {
        NetworkController.Disconnect();
    }
}
