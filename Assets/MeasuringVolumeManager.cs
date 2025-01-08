using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MeasuringVolumeManager : NetworkBehaviour
{
    [SerializeField] GameObject measuringVolumePrefab;
    public void SpawnMeasuringVolume(ulong clientId)
    {

        // Instantiate the MeasuringVolume object on the server
        GameObject measuringVolume = Instantiate(measuringVolumePrefab, transform);

        // Set its parent to the MeasuringVolumeManager
        measuringVolume.transform.parent = this.transform;

        // Spawn it across the network
        NetworkObject networkObject = measuringVolume.GetComponent<NetworkObject>();
        networkObject.SpawnWithOwnership(clientId);
    }
}
