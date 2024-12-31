using System;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class MovableObject : NetworkBehaviour
{
    Vector3 screenPoint;
    Vector3 offset;
    Vector3 previousPosition;
    float footOffsetDistance;
    bool isNetworkInitialized = false;
    bool isMouseDown = false;

    [Header ("Components")]
    public LayerMask groundMask;
    public Transform footOffsetPoint;

    [Header ("Network Variables")]
    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
    [SerializeField] private NetworkVariable<bool> IsFlying = new NetworkVariable<bool> ();


    private void Start()
    {
        //figure out how far from the ground to place the object
        if (footOffsetPoint == null) footOffsetPoint = transform;
        footOffsetDistance = transform.position.y - footOffsetPoint.position.y;
    }

    public override void OnNetworkSpawn()
    {
        isNetworkInitialized = true;
        if (Position.Value == Vector3.zero)
        {
            Position.Value = transform.position;
        }
        previousPosition = Position.Value;
        IsFlying.Value = false;
    }

    public bool HasUnconfirmedMovement()
    {
        return previousPosition != Position.Value;
    }
    public float GetUnconfirmmedDistance()
    {
        float rawDistance = Vector3.Distance(Position.Value, previousPosition);
        return rawDistance * GameSettings.Instance.distanceScaleMultipler;
    }

    public void ConfirmMovement()
    {
        Debug.Log($"ConfirmMovement - Setting previousPosition: {transform.position}");
        previousPosition = transform.position;
    }
    public void CancelMovement()
    {
        Debug.Log($"CancelMovement - Moving to previousPosition: {previousPosition}");
        Move(previousPosition);
    }

    void Move(Vector3 position)
    {
        if (IsServer)
        {
            if (EntityManager.Instance.canMoveToCell(gameObject,position))
            {
                transform.position = position;
                Position.Value = position;
            }
        }
        else
        {
            SubmitPositionRequestServerRpc(position);
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
        if (EntityManager.Instance.canMoveToCell(gameObject,newPosition))
        {
            Position.Value = newPosition;
            transform.position = newPosition;

        }
    }

    public void ToggleIsFlying(bool newValue)
    {
        if (IsServer)
        {
            IsFlying.Value = newValue;
            Vector3 groundPosition = Position.Value;
            try
            {
                groundPosition.y = getGroundPosition(groundPosition.x, groundPosition.z);
                Move(groundPosition);
            }
            catch(Exception e)
            {
                Debug.LogWarning(e.Message);
            }
        }
        else
        {
            ToggleIsFlyingServerRpc(newValue);
        }
    }
    [ServerRpc]
    private void ToggleIsFlyingServerRpc(bool newValue)
    {
        IsFlying.Value = newValue;
        Vector3 groundPosition = Position.Value;
        try
        {
            groundPosition.y = getGroundPosition(groundPosition.x, groundPosition.z);
            Move(groundPosition);
        }
        catch (Exception e)
        {
            Debug.LogWarning(e.Message);
        }
    }

    public bool GetIsFlying()
    {
        return IsFlying.Value;
    }
    public float GetDistanceFromGround()
    {
        float footPosition = transform.position.y + footOffsetDistance;
        return (footPosition - getGroundPosition(transform.position.x, transform.position.z)) * GameSettings.Instance.distanceScaleMultipler;
    }
    public void FlyUp()
    {
        if (!IsFlying.Value) return;
        Vector3 newPosition = Position.Value;
        newPosition.y = newPosition.y + 1;
        Move(newPosition);
    }
    public void FlyDown()
    {
        //must be flying for this to do anything.
        if (!IsFlying.Value) return;

        //calculate the new position, shifted down a block
        Vector3 newPosition = Position.Value;
        newPosition.y = newPosition.y - 1;

        //if this would put them underground, snap to right above ground.
        if(newPosition.y < getGroundPosition(newPosition.x, newPosition.z))
        {
            newPosition.y = getGroundPosition(newPosition.x,newPosition.z);
        }
        Move(newPosition);
    }

    void OnMouseDown()
    {
        if (!isNetworkInitialized) return;

        screenPoint = Camera.main.WorldToScreenPoint(Position.Value);

        offset = Position.Value - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
        Debug.Log($"OnMouseDown - ScreenPoint: {screenPoint}, Offset: {offset}");

        isMouseDown = true;
    }
    private void OnMouseUp()
    {
        isMouseDown = false;
    }
    void OnMouseDrag()
    {
        if (!isNetworkInitialized) return;
        if (IsOwner || IsServer)
        {
            Vector3 newPosition = getNewPosition();

            Debug.Log($"OnMouseDrag - Start: Transform: {transform.position}, Position: {Position.Value}, Previous: {previousPosition}");


            Move(newPosition);
        }
    }

    private void Update()
    {
        if (!isNetworkInitialized) return;

        if (transform.position != Position.Value)
        {
            transform.position = Position.Value;
        }
    }

    Vector3 getNewPosition()
    {
        if (!isMouseDown)
        {
            Debug.LogWarning("getNewPosition called before OnMouseDown initialized values!");
            return transform.position; // Fallback to current position
        }
        Debug.Log($"getNewPosition - ScreenPoint: {screenPoint}, Offset: {offset}, Flying: {IsFlying.Value}");

        Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);

        //get the new position
        Vector3 newPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
        //for x and z, snap to grid
        newPosition.x = Mathf.Round(newPosition.x);
        newPosition.z = Mathf.Round(newPosition.z);
        if (IsFlying.Value)
        {
            // if we are flying we dont need to snap to ground.
            // however, we do if flying would put us below ground.
            float groundLevel = getGroundPosition(newPosition.x, newPosition.z);
            if(groundLevel > Position.Value.y)
            {
                Debug.Log("Below ground, snapping to ground: "+groundLevel + " > "+ Position.Value.y);
                newPosition.y = groundLevel;
            }
            else
            {
                Debug.Log("Keeping at current height");
                newPosition.y = Position.Value.y;
            }

        }
        else
        {//if not y, snap to ground

            try
            {
                newPosition.y = getGroundPosition(newPosition.x, newPosition.z);
            }
            catch(Exception e)
            {
                Debug.LogWarning(e.Message);
                newPosition.y = Position.Value.y;
            }
        }

        return newPosition;
    }

    float getGroundPosition(float x, float z)
    {
        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        Vector3 rayStart = new Vector3(x, 1000f, z);

        if (Physics.Raycast(rayStart, transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity, groundMask))
        {
            return hit.point.y + footOffsetDistance;
        }
        else
        {
            throw new Exception("Ground not found!");
        }
    }

}
