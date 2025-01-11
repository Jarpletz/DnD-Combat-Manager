using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerSpawner : NetworkBehaviour
{
    [Serializable]
    public class PlayerInformation
    {
        public ulong clientId;
        public string characterName;
        public string colorString;
    }

    [SerializeField] private GameObject playerPrefab; // Assign your Player Prefab in the Inspector
    [SerializeField] private Transform spawnPosition;

    private void Start()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            SpawnPlayersForConnectedClients();
        }
    }

    public override void OnDestroy()
    {
        if (NetworkManager.Singleton && NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
        base.OnDestroy();
    }

    private void OnClientConnected(ulong clientId)
    {
        SpawnPlayerForClient(clientId);
    }

    private void SpawnPlayersForConnectedClients()
    {
        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            SpawnPlayerForClient(clientId);
        }
    }

    private void SpawnPlayerForClient(ulong clientId)
    {
        if (!IsServer) return;

        var spawnPosition = GetSpawnPosition(clientId); // Implement logic to determine spawn position
        var playerInstance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        var networkObject = playerInstance.GetComponent<NetworkObject>();
        networkObject.SpawnAsPlayerObject(clientId, true);

        if (networkObject.IsSpawned)
        {
           /* var entity = playerInstance.GetComponent<Entity>();

            Debug.Log(clientId + " " + playerInfo);
           
            Color c;
            string lobbyId = System.Text.Encoding.UTF8.GetString(NetworkManager.Singleton.NetworkConfig.ConnectionData);

            Player lobbyPlayer = LobbyManager.Instance.GetPlayersInLobby().Find(p=>p.Id = )

            if (ColorUtility.TryParseHtmlString(playerInfo.colorString, out c))
            {
                entity.updateColor(c);
            }*/
        }
    }

    private Vector3 GetSpawnPosition(ulong clientId)
    {
        // Example logic: Use clientId to generate a unique spawn position
        return spawnPosition.position + new Vector3(Random.value*clientId,0, Random.value * clientId);
    }
}
