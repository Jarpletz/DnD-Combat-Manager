using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerInfoUI : UIInfo
{
    [SerializeField] TextMeshProUGUI currentPlayerText;
    
    void Update()
    {
        currentPlayerText.text = EntityManager.Instance.GetCurrentEntity().getEntityName();
    }
}
