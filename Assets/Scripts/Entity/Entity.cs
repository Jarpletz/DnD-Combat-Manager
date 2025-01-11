using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.VisualScripting;
using UnityEngine;
using ColorUtility = UnityEngine.ColorUtility;

public class Entity : NetworkBehaviour
{
    [Header ("Network Variables")]
    public NetworkVariable<FixedString64Bytes> entityName = new NetworkVariable<FixedString64Bytes>("");
    public NetworkVariable<FixedString64Bytes> entityColor = new NetworkVariable<FixedString64Bytes>("#FFF");

    public NetworkVariable<int> health = new NetworkVariable<int>(0);
    public NetworkVariable<int> maxHealth = new NetworkVariable<int>(0);
    
    public NetworkVariable<int> initiative = new NetworkVariable<int>(0);
    public NetworkVariable<bool> isAlive = new NetworkVariable<bool>(true);

    [Header ("Settings")]
    [SerializeField] bool hasDeathSaves = true;
    [SerializeField] bool isPlayer = true;
    [SerializeField] string initialName;

    [Header("Child Components")]
    [SerializeField] TextMeshPro nameTag;
    [SerializeField] TextMeshPro statusText;
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            updateName(initialName);
            health.Value = 0;
            maxHealth.Value = 0;
            initiative.Value = 0;
            isAlive.Value = true;

            //if is a player, create a measuring volume for that player
            if (isPlayer)
            {
               FindObjectOfType<MeasuringVolumeManager>().SpawnMeasuringVolume(OwnerClientId);
            }
        }
        //add to the Entity Manager
        EntityManager.Instance.entities.Add(this);

        

        base.OnNetworkSpawn();
    }

    public override void OnDestroy()
    {
        EntityManager.Instance.entities.Remove(this);

        base.OnDestroy();
    }

    private void OnMouseDown()
    {
        if (IsOwner || IsServer)
        {
            UIManager uiManager = GameObject.FindWithTag("UI").GetComponent<UIManager>();
            uiManager.ShowEntityDetails(this);
        }
    }

    

    public string getEntityName()
    {
        return entityName.Value.Value;
    }
    public void updateName(string newName)
    {
        if (IsServer)
        {
            entityName.Value = newName;
        }
        else
        {
            updateNameServerRpc(newName);
        }
        nameTag.text = newName;
    }
    [ServerRpc]
    void updateNameServerRpc(string newName)
    {
        entityName.Value = newName;
    }

    public string getEntityColor()
    {
        return entityColor.Value.Value;
    }
    public void updateColor(Color color)
    {
        if (IsServer)
        {
            entityColor.Value = "#" +  color.ToHexString();
        }
        else
        {
            updateColorServerRpc("#" + color.ToHexString());
        }

        
    }
    [ServerRpc]
    void updateColorServerRpc(string newColor)
    {
        entityColor.Value = newColor;
    }

    public void updateIntitative(int newInitiative)
    {
        if (IsServer)
        {
            initiative.Value = newInitiative;
        }
        else
        {
            updateInitiativeServerRpc(newInitiative);
        }
    }
    [ServerRpc]
    void updateInitiativeServerRpc(int newInitiative)
    {
        initiative.Value = newInitiative;
    }

    public void updateMaxHealth(int newMaxHealth)
    {
        if (IsServer)
        {
            if(health.Value == maxHealth.Value)
            {
                health.Value = newMaxHealth;
            }
            maxHealth.Value = newMaxHealth;
        }
        else
        {
            updateMaxHealthServerRpc(newMaxHealth);
        }
    }

    [ServerRpc]
    void updateMaxHealthServerRpc(int newMaxHealth)
    {
        maxHealth.Value = newMaxHealth; 
        if (health.Value == maxHealth.Value)
        {
            health.Value = newMaxHealth;
        }
    }

    public void addHealth(int healthAdded)
    {
        addHealthServerRpc(healthAdded);
    }
    [ServerRpc]
    void addHealthServerRpc(int healthAdded)
    {
        health.Value += healthAdded;
        if (health.Value <= 0 && !hasDeathSaves)
        {
            isAlive.Value = false;
        }
        if (health.Value > maxHealth.Value)
        {
            health.Value = maxHealth.Value;
        }
    }
}
