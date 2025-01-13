using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class EntityManager : NetworkBehaviour
{
    public static EntityManager Instance;

    public NetworkVariable<int> currentTurnIndex = new NetworkVariable<int>();

    public List<Entity> entities = new List<Entity>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        currentTurnIndex.Value = 0;
        base.OnNetworkSpawn();
    }

    public bool canMoveToCell(GameObject obj, Vector3 newPosition)
    {
        Vector2 newPos =new Vector2(newPosition.x, newPosition.z);
        foreach(Entity entity in entities)
        {
            //its all good if it's the same object
            if (entity.gameObject == obj) continue;

            //if the new position is the same as the entity, that's no good - return false.
            Vector2 entityPosition = new Vector2(entity.gameObject.transform.position.x, entity.gameObject.transform.position.z);
            if (newPos == entityPosition) 
                return false;
        }

        return true;
    }


    public void Update()
    {
        if (!EntitiesAreInOrder())
        {
            entities = entities.OrderByDescending(e => e.Initiative.Value).ToList();
        }
    }

    private bool EntitiesAreInOrder()
    {
        if (entities.Count < 2) return true;

        int previousInitiative = entities[0].Initiative.Value;
        for(int i = 1; i < entities.Count; i++)
        {
            if(entities[i].Initiative.Value > previousInitiative)
            {
                return false;
            }
            previousInitiative = entities[i].Initiative.Value;
        }

        return true;
    }

    public Entity GetCurrentEntity()
    {
        //return null if the current turn index is out of range (or entities list is empty)
        if(entities.Count < currentTurnIndex.Value +1)
        {
            return null;
        }

        return entities[currentTurnIndex.Value];
    }

    public bool IsCurrentEntity(Entity entity)
    {
        if(entity == null) return false;

        return entity == GetCurrentEntity();
    }

    public void IncrementTurn()
    {
        if (IsServer)
        {
            currentTurnIndex.Value = (currentTurnIndex.Value + 1) % entities.Count;
        }
        else
        {
            UpdateTurnServerRPC(1);
        }
    }
    public void DecrementTurn()
    {
        if (IsServer)
        {
            currentTurnIndex.Value = (currentTurnIndex.Value - 1) % entities.Count;
            if (currentTurnIndex.Value < 0)
            {
                currentTurnIndex.Value = entities.Count - 1;
            }
        }
        else
        {
            UpdateTurnServerRPC(-1);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateTurnServerRPC(int distanceChanged, ServerRpcParams rpcParams = default)
    {
        //prevent players from ending eachothers turns
        ulong senderClientId = rpcParams.Receive.SenderClientId;
        if (entities[currentTurnIndex.Value].OwnerClientId != senderClientId) return;

        currentTurnIndex.Value = (currentTurnIndex.Value + distanceChanged) % entities.Count;
        if (currentTurnIndex.Value < 0)
        {
            currentTurnIndex.Value = entities.Count -  1;
        }
    }
}
