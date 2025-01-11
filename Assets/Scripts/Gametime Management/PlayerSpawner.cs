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
            PlayerInfoManager.onPlayerAddedCallback += SpawnPlayerForClient;
            SpawnPlayersForConnectedClients();
        }
    }

    public override void OnDestroy()
    {
        if (NetworkManager.Singleton && NetworkManager.Singleton.IsServer)
        {
            PlayerInfoManager.onPlayerAddedCallback -= SpawnPlayerForClient;

        }
        base.OnDestroy();
    }

    private void SpawnPlayersForConnectedClients()
    {
        foreach (var playerInfo in PlayerInfoManager.Instance.m_players)
        {
            SpawnPlayerForClient(playerInfo);
        }
    }

    private void SpawnPlayerForClient(PlayerInfoManager.PlayerInfo playerInfo)
    {
        if (!IsServer) return;

        Debug.Log("Spawning Player for " + playerInfo.clientId);

        var spawnPosition = GetSpawnPosition(playerInfo.clientId); // Implement logic to determine spawn position
        var playerInstance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        var networkObject = playerInstance.GetComponent<NetworkObject>();
        networkObject.SpawnAsPlayerObject(playerInfo.clientId, true);

        if (networkObject.IsSpawned)
        {
           var entity = playerInstance.GetComponent<Entity>();
            entity.updateColor(playerInfo.GetColor());
            entity.updateName(playerInfo.name);
        }
    }

    private Vector3 GetSpawnPosition(ulong clientId)
    {
        // Example logic: Use clientId to generate a unique spawn position
        return spawnPosition.position + new Vector3(Random.value*clientId,0, Random.value * clientId);
    }
}
