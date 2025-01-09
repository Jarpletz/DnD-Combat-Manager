using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class MeasurementUI : MonoBehaviour
{
    public MeasuringVolume measuringVolume;

    [Header("Child Components")]
    [SerializeField] Toggle showToolToggle;
    [SerializeField] GameObject controlsObject;
    [SerializeField] Toggle showOthersToggle;
    [SerializeField] TMP_Dropdown shapeDropdown;
    [SerializeField] TMP_InputField sizeField;
    [Header("Config")]
    [SerializeField] Vector2 sizeRange;


    public void OnDestroy()
    {
        if (measuringVolume)
        {
            // Unsubscribe from the flying callback
            measuringVolume.OnStateChanged -= RefreshValues;
        }

    }

    private void Update()
    {
        if(measuringVolume == null)
        {
            FindMeasuringVolume();
            return;
        }

    }


    void FindMeasuringVolume()
    {
        GameObject[] volumes = GameObject.FindGameObjectsWithTag("MeasuringVolume");
        foreach(GameObject volume in volumes)
        {
            NetworkObject networkObject = volume.GetComponent<NetworkObject>();
            if (networkObject != null && networkObject.IsOwner)
            {
                //set up the values if the volume is initialized, otherwise, drop it for now
                measuringVolume = networkObject.GetComponent<MeasuringVolume>();
                if (measuringVolume.isInitialized)
                {
                    RefreshValues();
                    measuringVolume.OnStateChanged += RefreshValues;
                }
                else
                {
                    measuringVolume = null;
                }
                
                return;
            }
        }
    }

    void RefreshValues()
    {
        bool useMeasuringTools = measuringVolume && measuringVolume.isDisplayed;

        showToolToggle.isOn = useMeasuringTools;
        controlsObject.SetActive(useMeasuringTools);

        if (measuringVolume == null) return;

        shapeDropdown.ClearOptions();
        foreach(MeasuringVolume.NameObjectPair pair in measuringVolume.volumes)
        {
            shapeDropdown.options.Add(new TMP_Dropdown.OptionData() { text = pair.name });
        }
        shapeDropdown.value = shapeDropdown.options.FindIndex(option => option.text == measuringVolume.GetVolumeName());
        shapeDropdown.RefreshShownValue();
        sizeField.text = measuringVolume.volumeSizeFeet.Value.ToString();
        showOthersToggle.isOn = measuringVolume.showOthers.Value;
    }
    
    public void ToggleIsDisplayed(Toggle change)
    {
        if(!measuringVolume) return;
        measuringVolume.isDisplayed = change.isOn;
        showToolToggle.isOn = change.isOn;
        controlsObject.SetActive(change.isOn);
        //if its being turned off, don't show others
        if (!measuringVolume.isDisplayed)
        {
            measuringVolume.ToggleShowOthers(false);
        }
    }
    public void UpdateSize()
    {
        if (!measuringVolume) return;

        try
        {
            int newSize = Int32.Parse(sizeField.text);

            if(newSize < sizeRange.x)
            {
                newSize = (int)sizeRange.x;
                sizeField.text = sizeRange.x.ToString();
            }
            if(newSize > sizeRange.y)
            {
                newSize = (int)sizeRange.y;
                sizeField.text = sizeRange.y.ToString();
            }

            measuringVolume.UpdateSize(newSize);

        }
        catch (FormatException e)
        {
            Debug.LogWarning(e.Message);
            return;
        }
    }
    public void UpdateShape()
    {
        if (!measuringVolume) return;
        measuringVolume.UpdateVolumeName(shapeDropdown.options[shapeDropdown.value].text);
    }
    public void ToggleShowOthers(Toggle change)
    {
        if (!measuringVolume) return;
        measuringVolume.ToggleShowOthers(change.isOn);
    }

}
