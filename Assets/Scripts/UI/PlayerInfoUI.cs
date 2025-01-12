using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            NPCBehavior npcBehavior = currentEntity.GetComponent<NPCBehavior>();
            if(npcBehavior && !npcBehavior.ShowPlayers.Value)
            {
                currentPlayerText.text = GetRandomString(currentEntity.getEntityName().Length);
            }
            else
            {
                currentPlayerText.text = currentEntity.getEntityName();
            }

        }
    }
    string GetRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 !@#$%^&*()<>?,./;:'`~-=_+[]{}|??±º?¶‰???????…???§†‡*¬??";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s =>
            {
                return s[Random.Range(0,chars.Length)];
            }).ToArray());
    }
}
