using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
public class Entity : NetworkBehaviour
{
    public NetworkVariable<FixedString64Bytes> entityName = new NetworkVariable<FixedString64Bytes>("");
    public NetworkVariable<int> health = new NetworkVariable<int>(0);
    public NetworkVariable<int> maxHealth = new NetworkVariable<int>(0);
    
    public NetworkVariable<int> initiative = new NetworkVariable<int>(0);
    public NetworkVariable<bool> isAlive = new NetworkVariable<bool>(true);

    [SerializeField] bool hasDeathSaves = true;

    private void OnMouseDown()
    {
        UIManager uiManager = GameObject.FindWithTag("UI").GetComponent<UIManager>();
        uiManager.ShowEntityDetails(this);
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
    }
    [ServerRpc]
    void updateNameServerRpc(string newName)
    {
        entityName.Value = newName;
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
