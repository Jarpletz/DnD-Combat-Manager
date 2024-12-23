using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MovableObject : NetworkBehaviour
{
    Vector3 screenPoint;
    Vector3 offset;
    float footOffsetDistance;
    bool isNetworkInitialized = false;

    public LayerMask groundMask;
    public Transform footOffsetPoint;
    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();


    private void Start()
    {
        footOffsetDistance = transform.position.y - footOffsetPoint.position.y;
    }

    public override void OnNetworkSpawn()
    {
        isNetworkInitialized = true;
        Position.Value = transform.position;
    }

    public void Move()
    {
        var pos = getNewPosition();

        if (NetworkManager.Singleton.IsServer)
        {
            transform.position = pos;
            Position.Value = pos;
        }
        else
        {
            SubmitPositionRequestServerRpc(pos);
        }
    }

    [ServerRpc]
    void SubmitPositionRequestServerRpc(Vector3 newPosition)
    {
        if (!IsServer)
        {
            Debug.Log("Cannot move, sice you aren't the server!");
            return;
        }
            Position.Value = newPosition;
    }

    void OnMouseDown()
    {
        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);

        offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
    }
    void OnMouseDrag()
    {
        Move();
        transform.position = Position.Value;
        
    }

    private void Update()
    {
        if (!isNetworkInitialized) return; 

        transform.position = Position.Value;
    }

    Vector3 getNewPosition()
    {
        Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);

        //get the new position
        Vector3 newPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
        //for x and z, snap to grid
        newPosition.x = Mathf.Round(newPosition.x);
        newPosition.z = Mathf.Round(newPosition.z);
        //for y, snap to ground
        newPosition.y = getYPositionFromRaycast();

        return newPosition;
    }

    float getYPositionFromRaycast()
    {
        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        Vector3 rayStart = new Vector3(transform.position.x, 1000f, transform.position.z);

        if (Physics.Raycast(rayStart, transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity, groundMask))
        {
            return hit.point.y + footOffsetDistance;
        }
        else
        {
            return transform.position.y;
        }
    }


}
