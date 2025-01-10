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
    List<PlayerListItemUI> playerListUIs = new List<PlayerListItemUI>();    

    [Header("Player Settings")]
    [SerializeField] TMP_InputField characterNameField;
    [SerializeField] List<Color> possibleColors = new List<Color>();

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

        
        UpdatePlayerListInformation(lobby.Players);
;
    }

    void RegeneratePlayerList(List<Player> updatedPlayers)
    {
        Debug.Log("Regenerating Player List!");

        //whipe the old list
        playerListUIs.Clear();
        foreach (Transform child in scrollContentObject.transform)
        {
            Destroy(child.gameObject);
        }

        //generate the new one
        foreach(Player player in updatedPlayers)
        {
            GameObject newItem = Instantiate(playerItemPrefab, scrollContentObject.transform);
            PlayerListItemUI itemUI = newItem.GetComponent<PlayerListItemUI>();
            itemUI.SetUp(player);
            playerListUIs.Add(itemUI);
        }
    }

    void UpdatePlayerListInformation(List<Player> updatedPlayers)
    {
        if(updatedPlayers.Count != playerListUIs.Count)
        {
            RegeneratePlayerList(updatedPlayers);
        }
        else
        {
            for(int i=0; i<updatedPlayers.Count; i++)
            {
                playerListUIs[i].SetUp(updatedPlayers[i]);
            }
        }
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
