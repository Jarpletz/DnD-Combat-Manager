using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject dmInfoUIPrefab;
    [SerializeField] GameObject playerInfoPrefab;

    [SerializeField] EntityUI entityUI;
    UIInfo uiInfo;

    void Start()
    {
        //If already connected, set up UI according to roles.
        if (NetworkManager.Singleton.IsListening)
        {
            UpdateUIFromRoles();
        }
        else
        {
            // Wait for the network to start, then set up UI according to roles
            NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton)
        {
            NetworkManager.Singleton.OnServerStarted -= HandleServerStarted;
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        }
    }

    public void ShowEntityDetails(Entity entity)
    {
        entityUI.gameObject.SetActive(true);
        entityUI.SetupEntityUI(entity);

        if (uiInfo)
        {
            uiInfo.gameObject.SetActive(false);
        }
    }
    public void CloseEntityDetails()
    {
        entityUI.CloseEntityUI();
        entityUI.gameObject.SetActive(false);

        if (uiInfo)
        {
            uiInfo.gameObject.SetActive(true);
        }

    }

    private void UpdateUIFromRoles()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            HandleServerStarted();
        }
        else
        {
            HandleClientConnected(NetworkManager.Singleton.LocalClientId);
        }
    }

    private void HandleServerStarted()
    {
        //if a uiInfo instance exists, whipe it before creating the server UI
        if (uiInfo)
        {
            Destroy(uiInfo.gameObject);
            uiInfo = null;
        }

        if (!uiInfo)
        {
            GameObject dmInfoObject = Instantiate(dmInfoUIPrefab, transform);
            dmInfoObject.transform.SetAsFirstSibling();
            uiInfo = dmInfoObject.GetComponent<DMInfoUI>();
        }
        
    }
    private void HandleClientConnected(ulong clientId)
    {
        // if the client connected is not this client, or this is also the server, return.
        if (NetworkManager.Singleton.LocalClientId != clientId || NetworkManager.Singleton.IsServer)
        {
            return;
        }

        //if a ui info already exists, destroy it before creating the player ui info
        if (uiInfo)
        {
            Destroy(uiInfo.gameObject);
            uiInfo = null;
        }

        //if the player ui doesn't exist, create it
        if (!uiInfo)
        {
            GameObject playerInfoObject = Instantiate(playerInfoPrefab, transform);
            playerInfoObject.transform.SetAsFirstSibling();
            uiInfo = playerInfoObject.GetComponent<PlayerInfoUI>();
        }

        
    }
}
