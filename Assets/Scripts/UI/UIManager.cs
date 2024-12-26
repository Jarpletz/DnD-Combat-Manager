using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] EntityUI entityUI;
    [SerializeField] DMInfoUI dmInfoUI;

    public void ShowEntityDetails(Entity entity)
    {
        entityUI.gameObject.SetActive(true);
        entityUI.SetupEntityUI(entity);

        dmInfoUI.gameObject.SetActive(false);
    }
    public void CloseEntityDetails()
    {
        entityUI.CloseEntityUI();
        entityUI.gameObject.SetActive(false);
        dmInfoUI.gameObject.SetActive(true);

    }
}
