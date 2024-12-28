using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerInfoUI : UIInfo
{
    [SerializeField] TextMeshProUGUI currentPlayerText;
    
    void Update()
    {
        Entity currentEntity = EntityManager.Instance.GetCurrentEntity();
        if (currentEntity)
        {
            currentPlayerText.text = currentEntity.getEntityName();

        }
    }
}
