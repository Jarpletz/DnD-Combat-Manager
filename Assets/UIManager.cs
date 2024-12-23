using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] EntityUI entityUI;

    public void ShowEntityDetails(Entity entity)
    {
        entityUI.gameObject.SetActive(true);
        entityUI.SetupEntityUI(entity);
    }
    public void CloseEntityDetails()
    {
        entityUI.CloseEntityUI();
        entityUI.gameObject.SetActive(false);
    }
}
