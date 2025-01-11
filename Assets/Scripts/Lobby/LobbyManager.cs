using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;
using QueryFilter = Unity.Services.Lobbies.Models.QueryFilter;

public class LobbyManager : MonoBehaviour
{
    public string playerName;
    public string encounterName;
    [SerializeField] Color defaultColor;
    public Lobby joinedLobby;

    private Lobby hostLobby;
    private float heartbeatTimer;
    private float pollTimeer;
    private bool connectedToRelay = false;

    //Events
    public delegate void OnJoinedLobbyUpdated(Lobby joinedLobby);
    public static event OnJoinedLobbyUpdated JoinedLobbyUpdatedEvent;

    public delegate void JoinSuccessful(bool isJoinSuccessful, string message="");
    public static event JoinSuccessful JoinAttemtedEvent;

    public delegate void UpdatedLobbyInfo(Lobby joinedLobby);
    public static event UpdatedLobbyInfo UpdatedLobbyInfoEvent;

    public static LobbyManager Instance;

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

    void Start()
    {
        String userName = "Player"+UnityEngine.Random.Range(0, 9000).ToString();
        Authenticate(userName.ToString());

        joinedLobby = null;
    }

    /*void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        if (GUILayout.Button("Create Lobby")) CreateLobby();
        if (GUILayout.Button("List Lobbies")) ListLobbies();

        GUILayout.EndArea();
    }*/

    private void Update()
    {
        HandleLobbyHeartbeat();
        HandleLobbyPollForUpdates();
    }

    public async void Authenticate(string playerName)
    {
        this.playerName = playerName;
        InitializationOptions initializationOptions = new InitializationOptions();
        initializationOptions.SetProfile(playerName);

        await UnityServices.InitializeAsync(initializationOptions);
        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    #region Lobby Hosting
    public async void CreateLobby()
    {
        try
        {
            JoinAttemtedEvent?.Invoke(false, "Creating Lobby...");

            playerName = "Game Master";
            string lobbyName = "MyLobby";
            int maxPlayers = 10;
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = true,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                        { "EncounterName", new DataObject( DataObject.VisibilityOptions.Member, encounterName) },
                        { "RelayCode", new DataObject( DataObject.VisibilityOptions.Member,"0")}
                },
            };
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);

            hostLobby = lobby;
            joinedLobby = lobby;

            //notify new lobby info 
            JoinedLobbyUpdatedEvent?.Invoke(joinedLobby);
            //send info about lobby joined 
            UpdatedLobbyInfoEvent?.Invoke(joinedLobby);
            //notify join success
            JoinAttemtedEvent?.Invoke(true, "");

        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e.Message);
            JoinAttemtedEvent?.Invoke(false, "Error creating lobby:" + e.Message);
        }

    }

    async void HandleLobbyHeartbeat()
    {
        if (hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer <= 0f)
            {
                heartbeatTimer = 15;
                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    public async void UpdateEncounterName(string newName)
    {
        if (hostLobby == null) return;

        try
        {
            encounterName = newName;
            hostLobby = await LobbyService.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                    {
                        { "EncounterName", new DataObject( DataObject.VisibilityOptions.Member, encounterName) }
                    },
            });

            joinedLobby = hostLobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e.Message);
        }
    }
    public async void KickPlayer(string playerID) 
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerID);

            if(playerID == AuthenticationService.Instance.PlayerId)
            {
                joinedLobby = null;
                hostLobby = null;
                JoinedLobbyUpdatedEvent?.Invoke(joinedLobby);
            }

        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e.Message);
        }
    }
    
    public async void StartGame()
    {
        if (IsPlayerHost())
        {
            try
            {
                Debug.Log("Starting Game!");

                string relayCode = await RelayManager.Instance.CreateRelay();

                joinedLobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        { "RelayCode", new DataObject(DataObject.VisibilityOptions.Member,relayCode) }
                    }
                });

                connectedToRelay = true;
                //set the connection data to contain the lobby player id
                string playerId = AuthenticationService.Instance.PlayerId;
            }
            catch(LobbyServiceException e)
            {
                Debug.LogWarning(e);
            }
        }
    }
    
    #endregion

    #region Lobby Joining
    async void HandleLobbyPollForUpdates()
    {
        if (joinedLobby != null)
        {
            pollTimeer -= Time.deltaTime;
            if (pollTimeer <= 0f)
            {
                pollTimeer = 1.1f;
                try
                {
                    joinedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                    UpdatedLobbyInfoEvent?.Invoke(joinedLobby);

                    if (!connectedToRelay && joinedLobby.Data["RelayCode"].Value != "0")
                    {// if there is a relay code, start the game by connecting to relay!
                        if (!IsPlayerHost())
                        {
                            //join the relay 
                            await RelayManager.Instance.JoinRelay(joinedLobby.Data["RelayCode"].Value);
                            connectedToRelay = true;
                        }
                    }
                }
                catch(LobbyServiceException e)
                {//if ping fails, disconnect
                    joinedLobby = null;
                    hostLobby = null;
                    JoinAttemtedEvent?.Invoke(false, "Lost connection to server: "+e.Message);
                    JoinedLobbyUpdatedEvent?.Invoke(joinedLobby);
                }
            }
        }
    }

    private async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false,QueryOrder.FieldOptions.Created)
                },
            };

            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();
            Debug.Log("Lobbies found:" + queryResponse.Results.Count);
            foreach (Lobby lobby in queryResponse.Results)
            {
                Debug.Log(lobby.Name + " " + lobby.MaxPlayers);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e.Message);
        }
    }

    public async void JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            JoinAttemtedEvent?.Invoke(false, "Joining Lobby...");

            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer(),
            };
            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);

            joinedLobby = lobby;

            //send updated lobby join info
            JoinedLobbyUpdatedEvent?.Invoke(joinedLobby);
            //send info about lobby joined 
            UpdatedLobbyInfoEvent?.Invoke(joinedLobby);
            //notify join was success
            JoinAttemtedEvent?.Invoke(true, "");


            PrintPlayers(joinedLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e.Message);
            JoinAttemtedEvent?.Invoke(false, "Error joining lobby: "+ e.Message);
        }
    }

    public async void LeaveLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
            joinedLobby = null;
            hostLobby = null;
            JoinedLobbyUpdatedEvent?.Invoke(joinedLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e.Message);
        }
    }

    #endregion

    #region Player Info 
    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
                    {
                        { "PlayerName", new PlayerDataObject( PlayerDataObject.VisibilityOptions.Public, playerName) },
                        { "Color", new PlayerDataObject ( PlayerDataObject.VisibilityOptions.Member, "#"+defaultColor.ToHexString())},
                    },
        };
    }
    public List<Player> GetPlayersInLobby()
    {
        if (joinedLobby == null) return null;
        return joinedLobby.Players;
    }
    public bool IsPlayerHost()
    {
        if (joinedLobby == null) return false;

        return joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }
    public bool IsPlayerHost(string playerId)
    {
        if (joinedLobby == null) return false;

        return joinedLobby.HostId == playerId;
    }

    public async void UpdatePlayerName(string newPlayerName)
    {
        try
        {
            playerName = newPlayerName;
            joinedLobby = await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
                    {
                        { "PlayerName", new PlayerDataObject( PlayerDataObject.VisibilityOptions.Public, playerName) }
                    },
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e.Message);
        }
    }
    public async void UpdatePlayerColor(string newColorHexCode)
    {
        try
        {
            joinedLobby = await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
                    {
                        { "Color", new PlayerDataObject( PlayerDataObject.VisibilityOptions.Member, newColorHexCode) }
                    },
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e.Message);
        }
    }
    public Player GetClientPlayerInLobby()
    {
        if (joinedLobby == null) return null;

        return joinedLobby.Players.Find(p => p.Id == AuthenticationService.Instance.PlayerId);
    }

    private void PrintPlayers(Lobby lobby)
    {
        Debug.Log("Players in Lobby " + lobby.Name);
        foreach (Player player in lobby.Players)
        {
            Debug.Log(player.Id + " " + player.Data["PlayerName"].Value);
        }

    }


    #endregion
}