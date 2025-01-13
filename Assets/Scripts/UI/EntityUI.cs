using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class EntityUI : MonoBehaviour
{
    public Entity entity;
    private MovableObject moveable;
    private NPCBehavior npcBehavior;

    [Header ("General Entity")]
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TMP_InputField initiativeInputField;
    [SerializeField] GameObject turnObject;
    [Header("Health")]
    [SerializeField] Slider healthSlider;
    [SerializeField] TMP_InputField healthInputField;
    [SerializeField] TMP_InputField maxHealthInputField;
    [Header("Movement")]
    [SerializeField] GameObject movementObject;
    [SerializeField] TextMeshProUGUI distanceMovedText;
    [Header("Flying")]
    [SerializeField] GameObject flyingControls;
    [SerializeField] TextMeshProUGUI flyingHeightDisplay;
    [SerializeField] Toggle flyingToggle;
    [Header("Prone")]
    [SerializeField] Toggle proneToggle;
    [Header("NPC")]
    [SerializeField] GameObject npcObject;
    [SerializeField] Toggle showNpcToggle;
    [Header("Condition")]
    [SerializeField] TMP_Dropdown conditionDropdown;

    private void Start()
    {
        conditionDropdown.ClearOptions();
        foreach(GameSettings.EntityCondition condition in GameSettings.Instance.conditions)
        {
            conditionDropdown.options.Add(new TMP_Dropdown.OptionData() { text = condition.name });
        }
        conditionDropdown.RefreshShownValue();

    }

    public void OnDestroy()
    {
        if (entity)
        {
            entity.OnEntityUpdatedCallback -= UpdateEntityDisplay;
        }
        if (moveable)
        {
            // Unsubscribe from the flying callback
            moveable.OnFlyingStateChangedCallback -= UpdateFlyingToggle;
            moveable.OnProneStateChangedCallback -= UpdateProneToggle;
        }
        if (npcBehavior)
        {
            npcBehavior.OnShowPlayersChanged -= UpdateShowPlayersToggle;
        }
    }

    public void SetupEntityUI(Entity entity)
    {
        //cancel any movement on a previously selected entity, if applicable.
        if(this.moveable != null && this.entity != entity)
        {
            CancelEntityMovement();
        }

        if(entity != null)
        {
            this.entity = entity;
            moveable = entity.gameObject.GetComponent<MovableObject>();

            entity.OnEntityUpdatedCallback += UpdateEntityDisplay;
            UpdateEntityDisplay();

            //subscribe to watch the flying stuff
            moveable.OnFlyingStateChangedCallback += UpdateFlyingToggle;
            UpdateFlyingToggle(moveable.GetIsFlying());

            //prone
            moveable.OnProneStateChangedCallback -= UpdateProneToggle;
            UpdateProneToggle(moveable.GetIsProne());

            //npc stuff
            npcBehavior = entity.gameObject.GetComponent<NPCBehavior>();
            npcObject.SetActive(npcBehavior);
            if(npcBehavior)
            {
                npcBehavior.OnShowPlayersChanged += UpdateShowPlayersToggle;
                UpdateShowPlayersToggle(npcBehavior.ShowPlayers.Value);
            }
        }
    }
    public void CloseEntityUI()
    {
        CancelEntityMovement();

        if (entity)
        {
            entity.OnEntityUpdatedCallback -= UpdateEntityDisplay;
            entity = null;
        }

        if (moveable)
        {
            moveable.OnFlyingStateChangedCallback -= UpdateFlyingToggle;
            moveable = null;
        }
        if (npcBehavior)
        {
            npcBehavior.OnShowPlayersChanged -= UpdateShowPlayersToggle;
            npcBehavior = null;
        }

    }
    
    private void Update()
    {
        UpdateTurnDisplay();
        UpdateMovementDisplay();
        UpdateFlyingDisplay();
    }

    #region General Entity
    private void UpdateEntityDisplay()
    {
        if (!entity) return;

        //name and initiative
        nameText.text = entity.GetEntityName();
        if(entity.GetEntityColor() != null) { 
            nameText.color = entity.GetEntityColor();
        }
        initiativeInputField.text = entity.Initiative.Value.ToString();

        //health
        healthSlider.minValue = 0;
        healthSlider.maxValue = entity.MaxHealth.Value;
        healthSlider.value = entity.Health.Value;
        healthInputField.text = entity.Health.Value.ToString();
        maxHealthInputField.text = entity.MaxHealth.Value.ToString();

        //condition
        conditionDropdown.value = entity.ConditionIndex.Value;
        conditionDropdown.RefreshShownValue();

    }
    public void UpdateInititative()
    {
        try
        {
            entity.updateIntitative(Int32.Parse(initiativeInputField.text));
        }
        catch (FormatException e)
        {
            Debug.LogWarning("Format Error Updating Initiative:" + e.Message);
        }
    }
    public void UpdateHealth()
    {
        if (!entity) return;

        try
        {
            int newHealth = Int32.Parse(healthInputField.text);
            entity.updateHealth(newHealth);
        }
        catch (FormatException e)
        {
            Debug.LogWarning(e.Message);
        }
    }
    public void UpdateMaxHealth()
    {
        if (!entity) return;

        try
        {
            int newMaxHealth = Int32.Parse(maxHealthInputField.text);
            entity.updateMaxHealth(newMaxHealth);
        }
        catch (FormatException e)
        {
            Debug.LogWarning(e.Message);
        }
    }

    public void UpdateConditionIndex(TMP_Dropdown dropdown)
    {
        if (entity)
        {
            entity.UpdateConditionIndex(dropdown.value);
        }
    }
    #endregion

    #region prone

    private void UpdateProneToggle(bool isOn)
    {
        proneToggle.isOn = isOn;
    }
    public void ToggleIsProne(Toggle toggle)
    {
        if (moveable)
        {
            moveable.ToggleIsProne(toggle.isOn);
        }
    }

    #endregion

    #region turns
    public void HandleEndTurn()
    {
        ConfirmEntityMovement();
        EntityManager.Instance.IncrementTurn();
    }
    private void UpdateTurnDisplay()
    {
        if (EntityManager.Instance && EntityManager.Instance.IsCurrentEntity(entity))
        {
            turnObject.SetActive(true);
        }
        else
        {
            turnObject.SetActive(false);
        }
    }
    #endregion

    #region movement

    public void CancelEntityMovement()
    {
        if (moveable == null) return;

        moveable.CancelMovement();
    }
    public void ConfirmEntityMovement()
    {
        if (moveable == null) return;

        moveable.ConfirmMovement();
    }
    private void UpdateMovementDisplay()
    {
        if(!moveable || !moveable.HasUnconfirmedMovement())
        {
            movementObject.SetActive(false);
        }
        else
        {
            movementObject.SetActive(true);
            distanceMovedText.text = "Distance Moved ( Raw) : " + moveable.GetUnconfirmmedDistance().ToString("0") + "ft.";
        }
    }
    #endregion

    #region flying
    public void MovableFlyUp()
    {
        if (moveable)
        {
            moveable.FlyUp();
        }
    }
    public void MovableFlyDown()
    {
        if (moveable)
        {
            moveable.FlyDown();
        }
    }
    public void ToggleFlying(Toggle change)
    {
        if (moveable)
        {
            moveable.ToggleIsFlying(change.isOn);
        }
    }
    private void UpdateFlyingDisplay()
    {
        bool isFlying = moveable && moveable.GetIsFlying();
        if (isFlying)
        {
            flyingHeightDisplay.text = moveable.GetDistanceFromGround().ToString("0") + " ft.";
        }
    }
    private void UpdateFlyingToggle(bool isFlying)
    {
        flyingControls.SetActive(isFlying);
        flyingToggle.isOn = isFlying;
    }
    #endregion

    #region npc

    private void UpdateShowPlayersToggle(bool showNpc)
    {
        showNpcToggle.isOn = showNpc;
    }
    public void ToggleShowPlayers(Toggle change)
    {
        if (npcBehavior)
        {
            npcBehavior.ToggleShowPlayers(change.isOn);
        }
    }


    #endregion
}
