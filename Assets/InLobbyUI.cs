using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class InLobbyUI : MonoBehaviour
{
    [Header ("Title")]
    [SerializeField] TextMeshProUGUI encounterNameTitle;
    [SerializeField] TMP_InputField gameCodeText;

    [Header("Settings Objects")]
    [SerializeField] GameObject playerSettingsObject;
    [SerializeField] GameObject gmSettingsObject;

    [Header("Player List")]
    [SerializeField] GameObject playerItemPrefab;
    [SerializeField] GameObject scrollContentObject;
    List<Player> playerList = new List<Player>();
    

    [Header("Player Settings")]
    [SerializeField] TMP_InputField characterNameField;

    [Header("GM Settings")]
    [SerializeField] TMP_InputField encounterNameField;

    bool isGM = false;

    private void Start()
    {
        LobbyManager.UpdatedLobbyInfoEvent += UpdateInfo;
    }

    private void OnDestroy()
    {
        LobbyManager.UpdatedLobbyInfoEvent -= UpdateInfo;

    }

    void UpdateInfo(Lobby lobby)
    {
        if (lobby == null) return;

        //update title
        encounterNameTitle.text = lobby.Data["EncounterName"].Value;
        gameCodeText.text = "Game Code: " + lobby.LobbyCode;

        //figure out if is GM and show things accordingly
        isGM = LobbyManager.Instance.IsPlayerHost();
        gmSettingsObject.SetActive(isGM);
        playerSettingsObject.SetActive(!isGM);

        //player settings
        Player instancePlayer  = LobbyManager.Instance.GetInstancePlayerInLobby();
        if (instancePlayer!= null)
        {
            if (!characterNameField.isFocused)
            {
                characterNameField.text = instancePlayer.Data["PlayerName"].Value;
            }
        }

        //GM Settings
        if (!encounterNameField.isFocused)
        {
            encounterNameField.text = lobby.Data["EncounterName"].Value;
        }

        if(playerList != lobby.Players)
        {
            UpdatePlayerList(lobby.Players);
        }

        playerList = lobby.Players;
    }

    void UpdatePlayerList(List<Player> updatedPlayers)
    {

    }

    public void QuitLobby()
    {
        LobbyManager.Instance.LeaveLobby();
    }

    public void UpdatePlayerName()
    {
        string newName = characterNameField.text;
        if(newName != "")
        {
            LobbyManager.Instance.UpdatePlayerName(newName);
        }
    }
    public void UpdateEncounterName()
    {
        string newName = encounterNameField.text;
        if (newName != "")
        {
            LobbyManager.Instance.UpdateEncounterName(newName);
        }
    }
}
