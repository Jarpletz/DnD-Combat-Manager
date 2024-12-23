using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EntityUI : MonoBehaviour
{
    public Entity entity;

    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TMP_InputField initiativeInputField;
    [SerializeField] GameObject turnObject;

    public void SetupEntityUI(Entity entity)
    {
        if(entity != null)
        {
            this.entity = entity;
            nameText.text = entity.entityName.Value.ToString();
            initiativeInputField.text = entity.initiative.Value.ToString();
        }
    }

    public void CloseEntityUI()
    {
        entity = null;
    }
    public void UpdateInititative()
    {
        entity.updateIntitative( Int32.Parse(initiativeInputField.text) );
    }
}
