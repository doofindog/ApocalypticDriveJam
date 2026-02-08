using System;
using Azen.Logger;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using Random = UnityEngine.Random;

public class FrequencyHUD : MonoBehaviour
{
    [SerializeField] private SerializedFloat frequencyDistance;
    [SerializeField] private SerializedFloat transmissionProgress;
    
    [SerializeField] private TMP_Text frequencyText;
    [SerializeField] private TMP_Text inRangeText;

    [Header("Frequency HUD")] 
    [SerializeField] private UIWaveGraphic targetWave;
    [SerializeField] private UIWaveGraphic playerWave;
    
    [Header("Wave Visual")]
    [SerializeField] private float amplitude = 30f;
    [SerializeField] private float phase = 0f;     // keep phase constant for both
    
    [Header("Frequency Range")]
    [SerializeField] private float minFreq = 2.0f;
    [SerializeField] private float maxFreq = 10.0f;

    [Header("Distance Tuning")]
    [Tooltip("At/inside this distance, match succeeds.")]
    [SerializeField] private float matchRadius = 12f;

    [Tooltip("Beyond this distance, player frequency stays 'wrong'.")]
    [SerializeField] private float farRadius = 60f;
    
    [Header("Alignment")]
    [Tooltip("How fast the player frequency moves toward target when close.")]
    [SerializeField] private float alignSpeed = 2.5f; // units: freq-per-second (scaled by closeness)
    
    [SerializeField] private float farFrequencyOffset = 1.5f;
    
    
    private float targetFreq;
    private float playerFreq;
    private bool matched;

    private void Start()
    {
        frequencyDistance.OnValueChanged += UpdateFrequencyUI;
        transmissionProgress.OnValueChanged += UpdateTransmission;
        DeliveryManager.OnDeliveryCompleted += SetTargetPoint;
        
        
        RollNewTargetFrequency();
        playerFreq = RandomWrongFrequencyFarFromTarget();
    }

    private void OnDestroy()
    {
        frequencyDistance.OnValueChanged -= UpdateFrequencyUI;
        transmissionProgress.OnValueChanged -= UpdateTransmission;
        DeliveryManager.OnDeliveryCompleted -= SetTargetPoint;
    }

    private void UpdateFrequencyUI(float value)
    {
        if (frequencyText == null)
        {
            CustomLogger.LogError("Frequency text Not set", LogCategory.UI);
            return;
        }

     
        
        float dist = value;

        // 0 = far, 1 = close
        float closeness01 = Mathf.InverseLerp(farRadius, matchRadius, dist);
        closeness01 = Mathf.Clamp01(closeness01);

        playerFreq = Mathf.Lerp(0, targetFreq, closeness01);

        // Draw
        frequencyText.text = $"Frequency : {closeness01:F2} | {dist:F2} | {playerFreq:F2} Hz";
        playerWave.frequency = playerFreq;
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
    
    public void SetTargetPoint()
    {
        RollNewTargetFrequency();
        playerFreq = RandomWrongFrequencyFarFromTarget();
    }

    private void RollNewTargetFrequency()
    {
        matched = false;
        targetFreq = Random.Range(minFreq, maxFreq);
    }

    private float RandomWrongFrequencyFarFromTarget()
    {
        // Ensure we don't accidentally start already matched
        float f;
        do
        {
            f = Random.Range(minFreq, maxFreq);
        } while (Mathf.Abs(f - targetFreq) < 0.25f);

        return f;
    }

    private void Update()
    {
        targetWave.frequency = targetFreq;
        
        phase += Time.deltaTime * 2;
        playerWave.phase = phase;
        targetWave.phase = phase;
    }

    private void OnMatched()
    {
        //Debug.Log($"Frequency matched! targetFreq={targetFreq:F2} at point {targetPoint}");
        // Trigger download / objective complete here
    }
}
