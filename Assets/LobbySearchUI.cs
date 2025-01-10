using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbySearchUi : MonoBehaviour
{
    [SerializeField] TMP_InputField characterNameInputField;
    [SerializeField] TMP_InputField gameCodeInputField;
    [SerializeField] Button joinButton;
    [SerializeField] TextMeshProUGUI errorText;

    private void Start()
    {
        LobbyManager.JoinAttemtedEvent += UpdateErrorText;
        UpdateInputFields();
    }

    private void OnDestroy()
    {
        LobbyManager.JoinAttemtedEvent -= UpdateErrorText;
    }
    private void OnEnable()
    {
        UpdateInputFields ();
    }

    private void Update()
    {
        joinButton.interactable = characterNameInputField.text != "" && gameCodeInputField.text != "";
    }

    void UpdateInputFields()
    {
        gameCodeInputField.text = "";
        if (LobbyManager.Instance)
        {
            characterNameInputField.text = LobbyManager.Instance.playerName;
        }
    }

    void UpdateErrorText(bool isSuccess, string message)
    {
            errorText.text = message;
    }

    public void UpdatePlayerName()
    {
        LobbyManager.Instance.playerName = characterNameInputField.text;
    }
    public void AttemptJoinLobby()
    {
        string code = gameCodeInputField.text;
        if (code!="")
        {
            LobbyManager.Instance.JoinLobbyByCode(code);
        }
    }

    public void AttemptHostLobby()
    {
        LobbyManager.Instance.CreateLobby();
    }

}
