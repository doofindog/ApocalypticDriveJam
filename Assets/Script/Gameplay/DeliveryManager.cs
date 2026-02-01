using System;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using Random = UnityEngine.Random;

public class DeliveryManager : MonoBehaviour
{
    [SerializeField] private GameObject player;
    
    [SerializeField] private GameObject npcPrefab;
    [SerializeField] private Vector3 bounds;

    [SerializeField] private float inRangeDistance;
    [SerializeField] private float deliveryTimer;
    [SerializeField] private float deliveryTime;
    [SerializeField] private SerializedFloat deliveryProgress;
    [SerializeField] private float deliveryCompleteTime;
    private Transform npc;

    public static Action OnDeliveryCompleted;
    public static Action<Transform> OnDeliveryStarted;
    
    private void Awake()
    {
        FrequencyManager.OnTransmissionCompleted += SpawnNPC;
        deliveryProgress.Value = 0;
    }

    private void SpawnNPC()
    {
        Vector3 spawnPosition = new Vector3()
        {
            x = Random.Range(-bounds.x, bounds.x),
            y = 1.0f,
            z = Random.Range(-bounds.z, bounds.z)
        };
        
        var npcObj = Instantiate(npcPrefab, spawnPosition, Quaternion.identity);
        npc = npcObj.transform;
        
        OnDeliveryStarted?.Invoke(npc);
    }

    private void Update()
    {
        if(player == null || npc == null) return;
        
        float distance = Vector3.Distance(player.transform.position, npc.position);
        
        deliveryProgress.Value = distance < inRangeDistance 
            ? deliveryProgress.Value + Time.deltaTime 
            : deliveryProgress.Value - Time.deltaTime;
        
        deliveryProgress.Value = Mathf.Clamp(deliveryProgress.Value, 0, deliveryCompleteTime);

        if (deliveryProgress.Value >= deliveryCompleteTime)
        {
            deliveryProgress.Value = 0;
            Destroy(npc.gameObject);
            OnDeliveryCompleted?.Invoke();
        }
    }
}
