using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] GameObject colorButtonPrefab;
    [SerializeField] Transform colorButtonsParent;
    [SerializeField] List<Color> possibleColors = new List<Color>();
    [SerializeField] List<GameObject> colorButtons = new List<GameObject>();

    [Header("GM Settings")]
    [SerializeField] TMP_InputField encounterNameField;

    bool isGM = false;

    private void Start()
    {
        LobbyManager.UpdatedLobbyInfoEvent += UpdateInfo;
        GenerateColorButtons();
    }

    private void OnDestroy()
    {
        LobbyManager.UpdatedLobbyInfoEvent -= UpdateInfo;

    }

    void GenerateColorButtons()
    {
        foreach(Color c in possibleColors)
        {
            GameObject colorButton = Instantiate(colorButtonPrefab, colorButtonsParent);
            colorButton.GetComponent<Image>().color = c;
            colorButton.GetComponent<Button>().onClick.AddListener(delegate{ UpdatePlayerColor(c); });
            colorButtons.Add(colorButton);
        }
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

        //Client player settings
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

        //Color settings
        foreach(GameObject colorButton in colorButtons)
        {
           
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
    public void UpdatePlayerColor(Color color)
    {
        LobbyManager.Instance.UpdatePlayerColor("#" + color.ToHexString());
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
