using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyUIManager : MonoBehaviour
{
    [SerializeField] GameObject lobbySearchObj;
    [SerializeField] GameObject inLobbyObj;

    private void Start()
    {
        LobbyManager.JoinedLobbyUpdatedEvent += UpdateScreenDisplayed;
        UpdateScreenDisplayed(null);
    }

    private void OnDestroy()
    {
        LobbyManager.JoinedLobbyUpdatedEvent -= UpdateScreenDisplayed;

    }

    void UpdateScreenDisplayed(Lobby lobby)
    {
        if(lobby == null)
        {
            lobbySearchObj.SetActive(true);
            inLobbyObj.SetActive(false);
        }
        else
        {
            lobbySearchObj.SetActive(false);
            inLobbyObj.SetActive(true);
        }
    }
}
