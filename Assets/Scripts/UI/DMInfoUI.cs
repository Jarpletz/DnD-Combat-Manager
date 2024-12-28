using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class DMInfoUI : UIInfo
{
    [SerializeField] GameObject entityOrderInfoPrefab;
    [SerializeField] GameObject scrollContent;

    public List<GameObject> entityOrderInfos= new List<GameObject>();

   
    private void Update()
    {
        if (EntitiesChanged() || !ListIsSorted())
        {
            UpdateEntityList();
        }
    }

    private bool EntitiesChanged()
    {
        if (EntityManager.Instance.entities == null) return false;
        if (entityOrderInfos.Count != EntityManager.Instance.entities.Count)
        {
            return true;
        }
        return false;
    }
    
    //if the list is not sorted properly by initiative, return false 
    private bool ListIsSorted()
    {
        if (entityOrderInfos.Count < 1) return true;

        int previousInitiative = entityOrderInfos[0].GetComponent<EntityInfoBehavior>().entity.initiative.Value;
        foreach(GameObject entityInfo in entityOrderInfos)
        {
            int initiative = entityInfo.GetComponent<EntityInfoBehavior>().entity.initiative.Value;
            if ( initiative > previousInitiative)
            {
                return false;
            }
            previousInitiative = initiative;
        }


        return true;
    }
    private void UpdateEntityList()
    {
        Debug.Log("Updating Entities");
        // Clear existing UI elements
        foreach (var entityInfo in entityOrderInfos)
        {
            Destroy(entityInfo);
        }
        entityOrderInfos.Clear();

        // Create UI elements for each entity
        foreach (var entity in EntityManager.Instance.entities)
        {
            var entityInfo = Instantiate(entityOrderInfoPrefab, scrollContent.transform);
            entityOrderInfos.Add(entityInfo);

            // Set up the entity info with the info it has
            entityInfo.GetComponent<EntityInfoBehavior>().Initialize(entity);
        }
    }

}
