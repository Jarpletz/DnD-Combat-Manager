using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;

public class EntityInfoBehavior : MonoBehaviour
{
    public Entity entity;

    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TMP_InputField initiativeInputField;

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
}
