using System;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using Random = UnityEngine.Random;

public class DeliveryManager : MonoBehaviour
{
    public static DeliveryManager instance;
    
    [SerializeField] private GameObject player;
    
    [SerializeField] private GameObject npcPrefab;
    [SerializeField] private Vector3 bounds;

    [SerializeField] private float inRangeDistance;
    [SerializeField] private float deliveryTimer;
    [SerializeField] private float deliveryTime;
    [SerializeField] private SerializedFloat deliveryProgress;
    [SerializeField] public float deliveryCompleteTime;
    [SerializeField] private int deliveryAmount;
    private Transform npc;
    
    [SerializeField] public LayerMask npcLayer;

    public static Action<int> OnDeliveryCompleted;
    public static Action<Transform> OnDeliveryStarted;
    
    private void Awake()
    {
        instance = this;
        
        FrequencyManager.OnTransmissionCompleted += SpawnNPC;
        deliveryProgress.Value = 0;
    }

    private void SpawnNPC()
    {
        
        bool isValidSpawn = false;

        Vector3 spawnPosition = Vector3.zero;
        while (!isValidSpawn)
        {
            spawnPosition = new Vector3()
            {
                x = Random.Range(-bounds.x, bounds.x),
                y = 1.0f,
                z = Random.Range(-bounds.z, bounds.z)
            };
            
            Collider[] results = new Collider[5];
            int hitCount = Physics.OverlapSphereNonAlloc(spawnPosition, 2.5f, results, npcLayer);
            if (hitCount == 0)
            {
                isValidSpawn = true;
            }
        }

        
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
            
            int amount = deliveryAmount;
            CurrencyManager.Instance.Add(amount);
            OnDeliveryCompleted?.Invoke(amount);
        }
    }
}
