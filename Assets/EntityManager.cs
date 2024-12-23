using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityManager : MonoBehaviour
{
    public static EntityManager Instance;

    public List<GameObject> entities = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        
    }

    public bool canMoveToCell(GameObject obj, Vector3 newPosition)
    {
        Vector2 newPos =new Vector2(newPosition.x, newPosition.z);
        foreach(GameObject entity in entities)
        {
            //its all good if it's the same object
            if (entity == obj) continue;

            //if the new position is the same as the entity, that's no good - return false.
            Vector2 entityPosition = new Vector2(entity.transform.position.x, entity.transform.position.z);
            if (newPos == entityPosition) 
                return false;
        }

        return true;
    }
}
