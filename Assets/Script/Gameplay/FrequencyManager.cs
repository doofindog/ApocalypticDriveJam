using System;
using Azen.Logger;
using UnityEngine;
using Random = UnityEngine.Random;

public class FrequencyManager : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float frequency;
    [SerializeField] private Vector3 bounds;
    [SerializeField] private Vector3 frequencyLocation;
    [SerializeField] private float frequencyAcceptableDistance;
    [SerializeField] private float transmissionCompleteTime;
    
    [SerializeField] private SerializedFloat frequencyDistance;
    [SerializeField] private SerializedFloat transmissionProgress;
    
    public static Action<float, float> OnFrequencyChange; //Frequency Range, Transmission Load
    public static Action<float> OnReceivingTransmission;
    public static Action OnTransmissionCompleted;

    private void Awake()
    {
        DeliveryManager.OnDeliveryCompleted += SetNewFrequencyLocation;
    }

    private void SetNewFrequencyLocation()
    {
        frequencyLocation = transform.position;
        frequencyLocation.x = Random.Range(-bounds.x, bounds.x);
        frequencyLocation.y = Random.Range(-bounds.y, bounds.y);
        
        GameManager.Instance.SetState(GameplayState.Tracking);
    }

    private void Update()
    {
        if(GameManager.Instance.gameplayState != GameplayState.Tracking)
            return;
        
        
        if (player == null)
        {
            CustomLogger.LogError("Player Not found", LogCategory.FrequencyManager);
            return;
        }
        
        frequencyDistance.Value = Vector3.Distance(frequencyLocation, player.position);
        
        HandleInTransmissionRange(frequencyDistance.Value);
    }


    private void HandleInTransmissionRange(float distance)
    {
        
        transmissionProgress.Value = distance < frequencyAcceptableDistance
            ? transmissionProgress.Value + Time.deltaTime
            : transmissionProgress.Value - Time.deltaTime;
        
        transmissionProgress.Value = Mathf.Clamp(transmissionProgress.Value, 0, transmissionCompleteTime);
        
        if (transmissionProgress.Value >= transmissionCompleteTime)
        {
            transmissionProgress.Value = 0;
            OnTransmissionCompleted?.Invoke();
            
            GameManager.Instance.SetState(GameplayState.Delivering);
        }
    }
}
