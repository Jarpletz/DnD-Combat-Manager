using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuitGameUI : MonoBehaviour
{
    [SerializeField] GameObject quitGameUi;
    bool quitGameUiOpen = false;

    private void Start()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += handleHostLeaveGame;
        quitGameUi.SetActive(false);
        quitGameUiOpen = false;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= handleHostLeaveGame;
        }
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            quitGameUiOpen = !quitGameUiOpen;

            quitGameUi.SetActive(quitGameUiOpen);
        }
    }

    public void QuitGameBack()
    {
        quitGameUi.SetActive(false);
        quitGameUiOpen=false;
    }


    private void handleHostLeaveGame(ulong clientId)
    {
        if(clientId == NetworkManager.ServerClientId)
        {
            QuitGame();
        }
    }
    public void QuitGame()
    {
        if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsConnectedClient)
        {
            NetworkManager.Singleton.Shutdown();
        }
        if (LobbyManager.Instance && LobbyManager.Instance.joinedLobby != null)
        {
            LobbyManager.Instance.LeaveLobby();
        }
        SceneManager.LoadScene("Lobby");
    }
}
