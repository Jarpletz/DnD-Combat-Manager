using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EntityInfoBehavior : MonoBehaviour
{
    public Entity entity;

    [Header ("Components")]
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TMP_InputField initiativeInputField;
    [SerializeField] Image backgroundImage;

    [Header("Colors")]
    [SerializeField] Color32 currentTurnColor;
    [SerializeField] Color32 notCurrentTurnColor;

    private string initiativeValue;

    private void Update()
    {
        if (!entity) return;

        //update the initiative text displayed if its valid and not currently selected
        if(!initiativeInputField.isFocused)
        {
            try
            {
                int currentInitiative = Int32.Parse(initiativeInputField.text);

                if (entity.initiative.Value != currentInitiative)
                {
                    initiativeValue = entity.initiative.Value.ToString();
                    initiativeInputField.text = initiativeValue;
                }
            }
            catch(FormatException e) { 
                Debug.LogWarning(e.Message);
            }
        }

        //handle how this panel should be displayed based on if it is its current turn.
        HandleTurnChanged();
    }

    public void Initialize(Entity entity)
    {
        this.entity = entity;
        this.initiativeValue = entity.initiative.Value.ToString();
        this.initiativeInputField.text = initiativeValue;
        this.nameText.text = entity.getEntityName();

    }

    public void UpdateInitiative()
    {
        if (initiativeInputField.text == "") return;
        try
        {
            int newInitiative = Int32.Parse(initiativeInputField.text);

            entity.updateIntitative(newInitiative);
            initiativeValue = initiativeInputField.text;
        }
        catch(FormatException e)
        {
            Debug.LogWarning("Initiative Input format error:"+e.Message);
        }
    }

    void HandleTurnChanged()
    {
        if (EntityManager.Instance.IsCurrentEntity(entity))
        {
            DisplayAsCurrent();
        }
        else
        {
            DisplayAsNotCurrent();
        }
    }
    void DisplayAsCurrent()
    {
        nameText.fontStyle = FontStyles.Bold;
        backgroundImage.color = currentTurnColor;
    }
    void DisplayAsNotCurrent()
    {
        nameText.fontStyle = FontStyles.Normal;
        backgroundImage.color = notCurrentTurnColor;
    }
}
