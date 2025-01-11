using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PersistentNetworkManager : MonoBehaviour
{
    private void Awake()
    {
        if (FindObjectsOfType<NetworkManager>().Length > 1)
        {
            Destroy(gameObject); // Destroy duplicate NetworkManager
            return;
        }

        DontDestroyOnLoad(gameObject); // Persist this instance across scenes
    }
}

