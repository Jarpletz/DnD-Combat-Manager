using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbySceneTransitioner : MonoBehaviour
{
    [SerializeField] string gameplaySceneName;

    private int requiredPlayers = 2;
    private int connectedPlayers = 0;

    public float lobbyTimeout = 30f; // Time in seconds before starting the game
    private float timer;

    private void Start()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            timer = lobbyTimeout;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    private void Update()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                StartGame();
            }
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        List<Player> players = LobbyManager.Instance.GetPlayersInLobby();
        if (players == null) return;
        requiredPlayers = players.Count;

        connectedPlayers++;
        Debug.Log($"Client connected: {clientId}. Total connected: {connectedPlayers}");

        if (connectedPlayers >= requiredPlayers)
        {
            StartGame();
        }
    }

    private void StartGame()
    {
        Debug.Log("All players connected. Starting game...");
        NetworkManager.Singleton.SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Single);
    }
}

