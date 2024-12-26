using System;
using TMPro;
using UnityEngine;

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
            nameText.text = entity.getEntityName();
            initiativeInputField.text = entity.initiative.Value.ToString();
        }
    }

    public void CloseEntityUI()
    {
        entity = null;
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
}
