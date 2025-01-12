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

    [Header ("Child Components")]
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TMP_InputField initiativeInputField;
    [SerializeField] GameObject turnObject;
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

    public void OnDestroy()
    {
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

            //subscribe to watch the flying stuff
            moveable.OnFlyingStateChangedCallback += UpdateFlyingToggle;
            UpdateFlyingToggle(moveable.GetIsFlying());

            //prone
            moveable.OnProneStateChangedCallback -= UpdateProneToggle;
            UpdateProneToggle(moveable.GetIsProne());

            //name and initiative
            nameText.text = entity.getEntityName();
            initiativeInputField.text = entity.initiative.Value.ToString();

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
        entity = null;
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
    public void UpdateInititative()
    {
        try
        {
            entity.updateIntitative(Int32.Parse(initiativeInputField.text));
        }catch(FormatException e)
        {
            Debug.LogWarning("Format Error Updating Initiative:" + e.Message);
        }
    }
    private void Update()
    {
        UpdateTurnDisplay();
        UpdateMovementDisplay();
        UpdateFlyingDisplay();
    }

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
