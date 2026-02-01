using System;
using Azen.Logger;
using TMPro;
using UnityEngine;

public class FrequencyHUD : MonoBehaviour
{
    [SerializeField] private SerializedFloat frequencyDistance;
    [SerializeField] private SerializedFloat transmissionProgress;
    
    [SerializeField] private TMP_Text frequencyText;
    [SerializeField] private TMP_Text inRangeText;

    private void Start()
    {
        frequencyDistance.OnValueChanged += UpdateFrequencyUI;
        transmissionProgress.OnValueChanged += UpdateTransmission;
    }

    private void UpdateFrequencyUI(float value)
    {
        if (frequencyText == null)
        {
            CustomLogger.LogError("Frequency text Not set", LogCategory.UI);
            return;
        }
        
        frequencyText.text = value.ToString("F2");
    }

    private void UpdateTransmission(float value)
    {
        if (inRangeText == null)
        {
            CustomLogger.LogError("In Range Text not set", LogCategory.UI);
            return;
        }
        
        
        inRangeText.gameObject.SetActive(value > 0);
        if (inRangeText.gameObject.activeSelf)
        {
            float progress = (value / 5.0f) * 100;
            inRangeText.text = $"Receiving Transmission : {progress:F2}%";
        }
    }
}
