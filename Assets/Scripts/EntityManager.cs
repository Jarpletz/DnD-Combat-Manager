using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class EntityManager : NetworkBehaviour
{
    public static EntityManager Instance;

    public List<Entity> entities = new List<Entity>();

    public Entity currentEntity;
    public int currentEntityIndex;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
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
        entities = entities.OrderByDescending(e => e.initiative.Value).ToList();
    }
}
