using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using ColorUtility = UnityEngine.ColorUtility;

public class Entity : NetworkBehaviour
{
    [Header ("Network Variables")]
    public NetworkVariable<FixedString64Bytes> EntityName = new NetworkVariable<FixedString64Bytes>("");
    public NetworkVariable<FixedString64Bytes> EntityColor = new NetworkVariable<FixedString64Bytes>("#FFF");

    public NetworkVariable<int> Health = new NetworkVariable<int>(0);
    public NetworkVariable<int> MaxHealth = new NetworkVariable<int>(0);
    
    public NetworkVariable<int> Initiative = new NetworkVariable<int>(0);
    public NetworkVariable<int> ConditionIndex = new NetworkVariable<int>(0);


    public delegate void EntityUpdated();
    public EntityUpdated OnEntityUpdatedCallback;

    [Header ("Settings / Initial Values")]
    public bool isPlayer = true;
    [SerializeField] string initialName;
    [SerializeField] Color initialColor;
    [SerializeField] int inititalHealth;
    [SerializeField] int inititalMaxHealth;

    [Header("Child Components")]
    [SerializeField] TextMeshPro nameTag;
    [SerializeField] SpriteRenderer statusBackground;
    [SerializeField] TextMeshPro statusText;

    private void Awake()
    {
        EntityName.OnValueChanged += (oldValue, newValue) =>
        {
            OnEntityUpdatedCallback?.Invoke();
        };
        EntityColor.OnValueChanged += (oldValue, newValue) =>
        {
            OnEntityUpdatedCallback?.Invoke();
        };
        Health.OnValueChanged += (oldValue, newValue) =>
        {
            OnEntityUpdatedCallback?.Invoke();
        };
        MaxHealth.OnValueChanged += (oldValue, newValue) =>
        {
            OnEntityUpdatedCallback?.Invoke();
        };
        Initiative.OnValueChanged += (oldValue, newValue) =>
        {
            OnEntityUpdatedCallback?.Invoke();
        };
        ConditionIndex.OnValueChanged += (oldValue, newValue) =>
        {
            OnEntityUpdatedCallback?.Invoke();
        };    
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            updateName(initialName);
            updateColor(initialColor);
            Health.Value = inititalHealth;
            MaxHealth.Value = inititalMaxHealth;
            Initiative.Value = 0;
            ConditionIndex.Value = 0;

            //if is a player, create a measuring volume for that player
            if (isPlayer)
            {
               FindObjectOfType<MeasuringVolumeManager>().SpawnMeasuringVolume(OwnerClientId);
            }
        }
        //add to the Entity Manager
        EntityManager.Instance.entities.Add(this);

        nameTag.text = GetEntityName();
        nameTag.color = GetEntityColor();
    }
    private void Update()
    {
        UpdateConditionStatusRing();
    }
    public override void OnDestroy()
    {
        if (EntityManager.Instance)
        {
            EntityManager.Instance.entities.Remove(this);
        }

        base.OnDestroy();
    }

    private void OnMouseDown()
    {
        //Cancel if mouse is over a UI element
        if (HelperFunctions.IsPointerOverUIElement()) return;

        if (IsOwner || IsServer)
        {
            UIManager uiManager = GameObject.FindWithTag("UI").GetComponent<UIManager>();
            uiManager.ShowEntityDetails(this);
        }
    }

    #region Name
    public string GetEntityName()
    {
        return EntityName.Value.Value;
    }
    public void updateName(string newName)
    {
        if (IsServer)
        {
            EntityName.Value = newName;
            updateNametagClientRpc(newName);
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
        EntityName.Value = newName;
        nameTag.text = newName;
        updateNametagClientRpc(newName);

    }
    [ClientRpc]
    void updateNametagClientRpc(string newName)
    {
        nameTag.text = newName;
    }
    #endregion

    #region Color
    public Color GetEntityColor()
    {
        Color c;

        if (ColorUtility.TryParseHtmlString(EntityColor.Value.Value, out c))
        {
            return c;
        }

        return Color.white;
    }
    public void updateColor(Color color)
    {
        string newColor = "#" + color.ToHexString();
        if (IsServer)
        {
            EntityColor.Value = newColor;
            nameTag.color = color;
            updateNametagColorClientRpc(newColor);
        }
        else
        {
            updateColorServerRpc(newColor);
        }

        
    }
    [ServerRpc]
    void updateColorServerRpc(string newColor)
    {
        EntityColor.Value = newColor;
        updateNametagColorClientRpc(newColor);

        Color c;
        if (ColorUtility.TryParseHtmlString(newColor, out c))
        {
            nameTag.color = c;
        }
    }
    [ClientRpc]
    void updateNametagColorClientRpc(string newColor)
    {
        Color c;
        if (ColorUtility.TryParseHtmlString(newColor, out c))
        {
            nameTag.color = c;
        }
    }
    #endregion

    #region Initiative
    public void updateIntitative(int newInitiative)
    {
        if (IsServer)
        {
            Initiative.Value = newInitiative;
        }
        else
        {
            updateInitiativeServerRpc(newInitiative);
        }
    }
    [ServerRpc]
    void updateInitiativeServerRpc(int newInitiative)
    {
        Initiative.Value = newInitiative;
    }
    #endregion

    #region Health
    public void updateMaxHealth(int newMaxHealth)
    {
        if (IsServer)
        {
            if(Health.Value == MaxHealth.Value)
            {
                Health.Value = newMaxHealth;
            }
            else if(Health.Value > MaxHealth.Value)
            {//if health is greater(ex. temp hp, add the overage to the new max health
                Health.Value = newMaxHealth + (Health.Value - MaxHealth.Value);
            }
            else if (Health.Value > newMaxHealth)
            {
                Health.Value = newMaxHealth;

            }
            MaxHealth.Value = newMaxHealth;
        }
        else
        {
            updateMaxHealthServerRpc(newMaxHealth);
        }
    }

    [ServerRpc]
    void updateMaxHealthServerRpc(int newMaxHealth)
    {
        if (Health.Value == MaxHealth.Value)
        {
            Health.Value = newMaxHealth;
        }
        else if (Health.Value > MaxHealth.Value)
        {//if health is greater(ex. temp hp, add the overage to the new max health
            Health.Value = newMaxHealth + (Health.Value - MaxHealth.Value);
        }
        else if (Health.Value > newMaxHealth)
        {
            Health.Value = newMaxHealth;

        }
        MaxHealth.Value = newMaxHealth;
    }

    public void updateHealth(int newHealth)
    {
        if (IsServer)
        {
            Health.Value = newHealth;
        }
        else
        {
            updateHealthServerRpc(newHealth);
        }
    }
    [ServerRpc]
    void updateHealthServerRpc(int newHealth)
    {
        Health.Value = newHealth;
    }

    #endregion

    #region Condition
    public string GetConditionName()
    {
        return GameSettings.Instance.conditions[ConditionIndex.Value].name;
    }
    public Color GetConditionColor()
    {
        return GameSettings.Instance.conditions[ConditionIndex.Value].color;
    }
    public void UpdateConditionIndex(int newIndex)
    {
        if (IsServer)
        {
            ConditionIndex.Value = newIndex;
        }
        else
        {
            UpdateConditionIndexServerRpc(newIndex);
        }
    }
    [ServerRpc]
    void UpdateConditionIndexServerRpc(int newIndex)
    {
        ConditionIndex.Value = newIndex;
    }

    void UpdateConditionStatusRing()
    {
        int newIndex = ConditionIndex.Value;

        bool showStatusRing = newIndex != 0;

        NPCBehavior npcBehavior = GetComponent<NPCBehavior>();
        //if it should be hidden by npc behavior, don't show it
        if(npcBehavior && !npcBehavior.ShowPlayers.Value && IsClient && !IsServer) {
            showStatusRing = false;
        }
        //dont show if status is 0 (None)
        statusBackground.gameObject.SetActive(showStatusRing);

        string conditionName = GameSettings.Instance.conditions[newIndex].name;
        statusText.text = conditionName + " " + conditionName + " " + conditionName;

        Color statusColor = GameSettings.Instance.conditions[newIndex].color;
        statusColor.a = statusBackground.color.a;
        statusBackground.color = statusColor;
    }
    #endregion
}
