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
    public class MeasurementTool
    {
        public string name;
        public GameObject prefab;
        public float scaleMultiplier;
    }


    [Header ("Children Objects")]    
    [SerializeField] RuntimeTransformHandle rth;
    [SerializeField] NetworkObject currentVolume;
    [SerializeField] Transform positionTargetVolume;
    [SerializeField] MeasurementTool currrentMeasurementTool;

    [Header ("Config")]
    [SerializeField] float rotationSpeed;
    [SerializeField] float movementToTargetSpeed;
    [SerializeField] Color GMColor;

    [Header ("Possible Volumes")]
    public List<MeasurementTool> measurementTools = new List<MeasurementTool>();

    [Header("Network Variables")]
    private NetworkVariable<FixedString64Bytes> volumeName = new NetworkVariable<FixedString64Bytes> ();
    public NetworkVariable<float> volumeSizeFeet = new NetworkVariable<float>();
    public NetworkVariable<bool> showOthers = new NetworkVariable<bool> ();
    public delegate void StateChanged();
    public event StateChanged OnStateChanged;

    [Header("Local Settings")]
    public bool showTransformHandles;
    public bool isDisplayed;
    [SerializeField] string initialShapeName;
    [SerializeField] float initialSizeFeet;
    public bool isInitialized = false;

    private void Awake()
    {
        //subscribe network variables to StateChanged event, so UI knows when to update
        volumeName.OnValueChanged += (oldValue, newValue) =>
        {
            OnStateChanged?.Invoke();
        };
        volumeSizeFeet.OnValueChanged += (oldValue, newValue) =>
        {
            OnStateChanged?.Invoke();
        };
        showOthers.OnValueChanged += (oldValue, newValue) =>
        {
            OnStateChanged?.Invoke();
        };

        //set some initial values
        isDisplayed = false;
        if (IsServer)
        {
            showOthers.Value = false;
            volumeName.Value = initialShapeName;
            volumeSizeFeet.Value = initialSizeFeet;
        }
    }

    private void Start()
    {
        rth.handleCamera = Camera.main;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner)
        {
            UpdateSize(10);
        }
        changeVolume(initialShapeName);
        isInitialized = true;
    }

    private void Update()
    {
        //show the transform handles if they have permission and they should be shown
        if(IsOwner && isDisplayed && showTransformHandles)
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
            if ((IsOwner && isDisplayed) || showOthers.Value)
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
                float scale = volumeSizeFeet.Value / GameSettings.Instance.distanceScaleMultipler * currrentMeasurementTool.scaleMultiplier;
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

            //update the position to match the target modified by the rth
            if (IsOwner)
            {
                updatePosition(positionTargetVolume.position);
            }

        }
        //if the volumes name has changed, change the volume displayed
        if(GetVolumeName() != currrentMeasurementTool.name)
        {
            for(int i = 0; i < measurementTools.Count; i++)
            {
                if(GetVolumeName() == measurementTools[i].name)
                {
                    changeVolume(measurementTools[i].name);
                    break;
                }
            }
        }
    }

    public string GetVolumeName()
    {
        return volumeName.Value.Value;
    }
    public void UpdateVolumeName(string newName)
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
        MeasurementTool newVolumePair = measurementTools.Find(v => v.name == volumeName);
        if (newVolumePair == null)
        {
            Debug.LogWarning("No pair found!");
            return;
        }
        currrentMeasurementTool = newVolumePair;

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
            currentVolume = null;
        }

        GameObject newVolume = Instantiate(currrentMeasurementTool.prefab, transform);
        NetworkObject netObj = newVolume.GetComponent<NetworkObject>();
        netObj.SpawnWithOwnership(OwnerClientId,true);

        netObj.TrySetParent(transform, true);
        netObj.transform.position = previousTransform.transform.position;
        netObj.transform.rotation = previousTransform.rotation;
        netObj.transform.localScale = previousTransform.localScale;

        SetVolumeColor(netObj);


        currentVolume = netObj;
        if (!IsOwner) 
            currentVolume.gameObject.SetActive(showOthers.Value);
        if (netObj.IsSpawned)
        {
            setCurrentVolumeClientRpc(netObj.NetworkObjectId);
        }
    }
    
    [ClientRpc]
    void setCurrentVolumeClientRpc(ulong volumeId)
    {
        if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(volumeId, out var networkObject))
        {
            currentVolume = networkObject;
            
            if (!IsOwner)
            {
                currentVolume.gameObject.SetActive(showOthers.Value);
            }

            SetVolumeColor(networkObject);
        }
    }

    [ServerRpc]
    void updateRotationServerRpc(float change)
    {   
        currentVolume.transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }

    void updatePosition(Vector3 targetPosition)
    {
        if (IsServer)
        {
            if (currentVolume != null)
            {
                currentVolume.transform.position = targetPosition;
            }
        }
        else updatePositionServerRpc(targetPosition);
    }

    [ServerRpc]
    void updatePositionServerRpc(Vector3 targetPosition)
    {
        if (currentVolume != null)
        {
            currentVolume.transform.position = targetPosition;
        }
    }
   
    void SetVolumeColor(NetworkObject volume)
    {
        Entity entity = EntityManager.Instance.entities.Find(e => e.OwnerClientId == volume.OwnerClientId);

        Color c;
        if (entity && !entity.IsOwnedByServer)
        {
            c = entity.GetEntityColor();
        }
        else
        {
            c = GMColor;
        }
        Renderer renderer = volume.GetComponentInChildren<Renderer>();
        renderer.material.color = c;
    }
}
