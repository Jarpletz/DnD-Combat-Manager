using System;
using TMPro;
using UnityEngine;

public class EntityUI : MonoBehaviour
{
    public Entity entity;
    private MovableObject moveable;

    [Header ("Child Components")]
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TMP_InputField initiativeInputField;
    [SerializeField] GameObject turnObject;
    [SerializeField] GameObject movementObject;
    [SerializeField] TextMeshProUGUI distanceMovedText;

    public void SetupEntityUI(Entity entity)
    {
        //cancel any movement on a previously selected entity, if applicable.
        if(this.moveable != null)
        {
            CancelEntityMovement();
        }

        if(entity != null)
        {
            this.entity = entity;
            moveable = entity.gameObject.GetComponent<MovableObject>();
            nameText.text = entity.getEntityName();
            initiativeInputField.text = entity.initiative.Value.ToString();
        }
    }
    public void CloseEntityUI()
    {
        CancelEntityMovement();
        entity = null;
        moveable = null;
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
    public void HandleEndTurn()
    {
        ConfirmEntityMovement();
        EntityManager.Instance.IncrementTurn();
    }

    private void Update()
    {
        UpdateTurnDisplay();
        UpdateMovementDisplay();
    }

    private void UpdateTurnDisplay()
    {
        if (EntityManager.Instance.IsCurrentEntity(entity))
        {
            turnObject.SetActive(true);
        }
        else
        {
            turnObject.SetActive(false);
        }
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

}
