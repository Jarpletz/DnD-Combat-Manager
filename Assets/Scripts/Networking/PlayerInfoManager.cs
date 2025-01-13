using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PlayerInfoManager : NetworkBehaviour
{

    [Serializable]
    public class PlayerInfo : INetworkSerializable
    {
        public string name;
        public string color;
        public ulong clientId;
        // Default constructor is required for deserialization
        public PlayerInfo() { }

        public PlayerInfo(string name, string color, ulong clientId)
        {
            this.name = name;
            this.color = color;
            this.clientId = clientId;
        }

        public Color GetColor()
        {
            Color c;

            if (ColorUtility.TryParseHtmlString(this.color, out c))
            {
                return c;
            }
            return Color.white;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref name);
            serializer.SerializeValue(ref color);
            serializer.SerializeValue(ref clientId);
        }
    }

    public static PlayerInfoManager Instance;

    List<PlayerInfo> localPlayers = new List<PlayerInfo>();
    public List<PlayerInfo> m_players = new List<PlayerInfo>();

    public delegate void OnPlayerAdded(PlayerInfo playerInfo);
    public static OnPlayerAdded onPlayerAddedCallback;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(transform.gameObject);
        }
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += RemovePlayerInfo;
    }
    public override void OnDestroy()
    {
        if (NetworkManager.Singleton)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= RemovePlayerInfo;
        }
        base.OnDestroy();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        foreach (PlayerInfo player in localPlayers)
        {
            AddPlayerInfoToServer(player);
        }
    }


    public void AddPlayerInfo(ulong clientId, Player lobbyPlayer)
    {
        //dont add a player for the server
        if (NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsHost) return;

        PlayerInfo playerInfo = new PlayerInfo();
        playerInfo.clientId = clientId;
        playerInfo.color = lobbyPlayer.Data["Color"].Value;
        playerInfo.name = lobbyPlayer.Data["PlayerName"].Value;

        Debug.Log("Adding Player Info to Info Manager: Client ID " + clientId);

        localPlayers.Add(playerInfo);

        if (this.IsSpawned)
        {
            AddPlayerInfoToServer(playerInfo);
        }
    }

    void AddPlayerInfoToServer(PlayerInfo playerInfo)
    {
        if (IsServer)
        {
            m_players.Add(playerInfo);
            onPlayerAddedCallback?.Invoke(playerInfo);
        }
        else
        {
            AddPlayerInfoServerRpc(playerInfo);
        }
    }

    [ServerRpc (RequireOwnership =false)]
    private void AddPlayerInfoServerRpc(PlayerInfo playerInfo)
    {

        m_players.Add(playerInfo);
        onPlayerAddedCallback?.Invoke(playerInfo);
    }

    private void RemovePlayerInfo(ulong clientId) {
        localPlayers.Remove(m_players.Find(p => p.clientId == clientId));
        m_players.Remove(m_players.Find(p => p.clientId == clientId));
    }
}
