using RuntimeHandle;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class MeasuringVolume : NetworkBehaviour
{

    [Serializable]
    public class NameObjectPair
    {
        public string name;
        public GameObject prefab; // Keep this if you need it.
    }


    [Header ("Children Objects and Config")]    
    [SerializeField] RuntimeTransformHandle rth;
    [SerializeField] NetworkObject currentVolume;
    [SerializeField] NameObjectPair currentObjectPair;
    [SerializeField] float rotationSpeed;

    [Header ("Possible Volumes")]
    [SerializeField] List<NameObjectPair> volumes = new List<NameObjectPair>();

    [Header("Network Variables")]
    private NetworkVariable<FixedString64Bytes> volumeName = new NetworkVariable<FixedString64Bytes> ();
    public NetworkVariable<float> volumeSizeFeet = new NetworkVariable<float>();
    public NetworkVariable<bool> showOthers = new NetworkVariable<bool> ();

    [Header("Local Settings")]
    public bool showTransformHandles;
    public bool isDisplayed;

    private void Start()
    {
        rth.handleCamera = Camera.main;
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            UpdateSize(10);
        }
        changeVolume("Cube");

        base.OnNetworkSpawn();
    }

    private void Update()
    {
        //show the transform handles if they have permission and they should be shown
        if((IsOwner || IsServer) && showTransformHandles)
        {
            rth.gameObject.SetActive(true);
        }
        else
        {
            rth.gameObject.SetActive(false);
        }
        //show the volume if the conditions allow
        if (currentVolume)
        {
            if (true/*(IsOwner && isDisplayed) || showOthers.Value*/)
            {
                currentVolume.gameObject.SetActive(true);
            }
            else
            {
                currentVolume.gameObject.SetActive(false);
            }

            //scale the volume properly
            if (IsServer)
            {
                float scale = volumeSizeFeet.Value / GameSettings.Instance.distanceScaleMultipler;
                currentVolume.transform.localScale = new Vector3(scale, scale, scale);

            }

            //rotate the volume if the correct key is pressed
            if (IsOwner && Input.GetKey(KeyCode.R))
            {
                if (IsServer)
                    currentVolume.transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
                else
                    updateRotationServerRpc(rotationSpeed * Time.deltaTime);
               
            }
        }
        //if the volumes name has changed, change the volume displayed
        if(getVolumeName() != currentObjectPair.name)
        {
            for(int i = 0; i < volumes.Count; i++)
            {
                if(getVolumeName() == volumes[i].name)
                {
                    changeVolume(volumes[i].name);
                    break;
                }
            }
        }
    }

    public string getVolumeName()
    {
        return volumeName.Value.Value;
    }
    public void setVolumeName(string newName)
    {
        if (IsServer)
        {
            volumeName.Value = newName;
        }
        else
        {
            updateVolumeNameServerRpc(newName);
        }
    }
    [ServerRpc]
    private void updateVolumeNameServerRpc(string newName)
    {
        volumeName.Value = newName;
    }

    public void UpdateSize(float newScale)
    {
        if (IsServer)
        {
            volumeSizeFeet.Value = newScale;
        }
        else
        {
            UpdateSizeServerRpc(newScale);
        }
    }
    [ServerRpc]
    private void UpdateSizeServerRpc(float newScale)
    {
        volumeSizeFeet.Value = newScale;
    }

    public void ToggleShowOthers(bool newValue)
    {
        if (IsServer)
        {
            showOthers.Value = newValue;
        }
        else
        {
            ToggleShowOthersServerRpc(newValue);
        }
    }
    [ServerRpc]
    private void ToggleShowOthersServerRpc(bool newValue)
    {
        showOthers.Value = newValue;
    }

    void changeVolume(string volumeName)
    {
        Debug.Log("Changing Volume:" + volumeName);

        NameObjectPair newVolumePair = volumes.Find(v => v.name == volumeName);
        if (newVolumePair == null)
        {
            Debug.LogWarning("No pair found!");
            return;
        }
        currentObjectPair = newVolumePair;

        spawnCurrentVolumeServerRpc();
    }

    [ServerRpc (RequireOwnership =false)]
    void spawnCurrentVolumeServerRpc()
    {
        Transform previousTransform = transform;

        if (currentVolume != null)
        {
            previousTransform = currentVolume.transform;
            currentVolume.GetComponent<NetworkObject>().Despawn();
        }

        GameObject newVolume = Instantiate(currentObjectPair.prefab, transform);
        NetworkObject netObj = newVolume.GetComponent<NetworkObject>();
        netObj.SpawnWithOwnership(OwnerClientId,true);

        netObj.TrySetParent(transform, false);
        netObj.transform.position = previousTransform.position;
        netObj.transform.rotation = previousTransform.rotation;
        netObj.transform.localScale = previousTransform.localScale;

        rth.SetTarget(netObj.gameObject);
        currentVolume = netObj;
        setCurrentVolumeClientRpc(netObj.NetworkObjectId);
    }
    
    [ClientRpc]
    void setCurrentVolumeClientRpc(ulong volumeId)
    {
        if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(volumeId, out var networkObject))
        {
            currentVolume = networkObject;
            rth.SetTarget(networkObject.gameObject);
            if (!IsServer)
            {
                rth.startedDraggingHandle.AddListener(OnStartDrag);
                rth.endedDraggingHandle.AddListener(OnEndDrag);
            }
        }
    }

    void OnStartDrag()
    {
        if (!currentVolume) return;

        currentVolume.GetComponent<NetworkTransform>().enabled = false;
    }
    void OnEndDrag()
    {
        if (!currentVolume) return;

        if (!IsServer)
        {
            updatePositionAfterDragServerRpc(currentVolume.transform.position);
        }

        currentVolume.GetComponent<NetworkTransform>().enabled = true;
    }

    [ServerRpc]
    void updatePositionAfterDragServerRpc(Vector3 newPosition)
    {
        currentVolume.transform.position = newPosition;
    }
    [ServerRpc]
    void updateRotationServerRpc(float change)
    {
        currentVolume.transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }
}
