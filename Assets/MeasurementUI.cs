using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class MeasurementUI : MonoBehaviour
{
    [Header("Child Components")]
    [SerializeField] Toggle showToolToggle;
    [SerializeField] Toggle showOthersToggle;
    [SerializeField] TMP_Dropdown shapeDropdown;
    [SerializeField] TMP_InputField sizeField;

    public MeasuringVolume measuringVolume;



    private void Update()
    {
        if(measuringVolume == null)
        {
            findMeasuringVolume();
            return;
        }
    }


    void findMeasuringVolume()
    {
        GameObject[] volumes = GameObject.FindGameObjectsWithTag("MeasuringVolume");
        foreach(GameObject volume in volumes)
        {
            NetworkObject networkObject = volume.GetComponent<NetworkObject>();
            if (networkObject != null && networkObject.IsOwner)
            {
                measuringVolume = networkObject.GetComponent<MeasuringVolume>();
                return;
            }
        }
    }

    void refreshValues()
    {
        showToolToggle.isOn = measuringVolume && measuringVolume.isDisplayed;

        if (measuringVolume == null) return;

        shapeDropdown.ClearOptions();
        foreach(MeasuringVolume.NameObjectPair pair in measuringVolume.volumes)
        {
            shapeDropdown.options.Add(new TMP_Dropdown.OptionData() { text = pair.name });
        }
        shapeDropdown.value = shapeDropdown.options.FindIndex(option => option.text == measuringVolume.getVolumeName());

        sizeField.text = measuringVolume.volumeSizeFeet.Value.ToString();
        showOthersToggle.isOn = measuringVolume.showOthers.Value;

    }

}
