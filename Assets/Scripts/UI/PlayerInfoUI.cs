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
                currentPlayerText.text = GetRandomString(currentEntity.GetEntityName().Length);
            }
            else
            {
                currentPlayerText.text = currentEntity.GetEntityName();
            }

        }
    }
    string GetRandomString(int length)
    {
        const string chars = "0123456789 !@#$%^&*()<>?,./;:'`~-=_+[]{}|";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s =>
            {
                return s[Random.Range(0,chars.Length)];
            }).ToArray());
    }
}
