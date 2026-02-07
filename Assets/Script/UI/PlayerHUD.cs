using System;
using ArcadeVP;
using Azen.Logger;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] private ArcadeVehicleController player;
    [SerializeField] private GameObject playerObj;
    [SerializeField] private SerializedFloat deliveryProgress;
    [SerializeField] private RectTransform deliveryProgressTransform;
    [SerializeField] private Image deliveryProgressImage;

    [Header("Delivery Settings")] 
    [SerializeField] private RectTransform indicator;
    [SerializeField] private float indicatorRadius = 80f;  
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2f, 0f);
    [SerializeField] private Camera targetCamera;
    private Transform deliveryTarget;

    [Header("Indicator Behavior")]
    [SerializeField] private float hideIfCloserThan = 1.25f;

    [Header("Boost UI")] [SerializeField] 
    private SerializedFloat boostValue;
    [SerializeField] private Image boostUI;
    
    
    [Header("Health UI")]
    [SerializeField] private SerializedFloat healthValue;
    [SerializeField] private Image healthBar;

    
    private void Awake()
    {
        if (deliveryProgress == null)
        {
            CustomLogger.LogError("Delivery Progress Serialized Float not set", LogCategory.UI);
            return;
        }

        if (targetCamera == null)
            targetCamera = Camera.main;
        
        DeliveryManager.OnDeliveryStarted += UpdateIndicator;
        DeliveryManager.OnDeliveryCompleted += DisableIndicator;
        deliveryProgress.OnValueChanged += UpdateProgressBar;
        boostValue.OnValueChanged += UpdateBoostUI;
        healthValue.OnValueChanged += UpdateHealthUI;
        

        if (deliveryProgressTransform != null)
        {
            deliveryProgressTransform.gameObject.SetActive(false);
        }
        
        if(playerObj)
            player = playerObj.GetComponent<ArcadeVehicleController>();
    }

    private void UpdateHealthUI(float value)
    {
        Debug.Log("Called");
        healthBar.fillAmount = (value / player.maxHealth);
    }

    private void OnDestroy()
    {
        if (deliveryProgress != null)
            deliveryProgress.OnValueChanged -= UpdateProgressBar;
        
        DeliveryManager.OnDeliveryStarted -= UpdateIndicator;
        DeliveryManager.OnDeliveryCompleted -= DisableIndicator;
        deliveryProgress.OnValueChanged -= UpdateProgressBar;
        boostValue.OnValueChanged -= UpdateBoostUI;
        healthValue.OnValueChanged -= UpdateHealthUI;
    }

    private void Update()
    {
        if (playerObj == null || deliveryProgressTransform == null || targetCamera == null)
            return;

        // World position above player
        Vector3 worldPos = playerObj.transform.position + worldOffset;

        // Convert to screen position
        Vector3 screenPos = targetCamera.WorldToScreenPoint(worldPos);
        
        deliveryProgressTransform.position = screenPos;
        
        UpdateIndicatorArrow(screenPos);
    }
    
    private void UpdateIndicator(Transform target)
    {
        deliveryTarget = target;

        if (indicator == null) return;

        indicator.gameObject.SetActive(deliveryTarget != null);
    }

    private void DisableIndicator()
    {
        if (indicator == null) return;
        
        indicator.gameObject.SetActive(false);
    }

    private void UpdateProgressBar(float value)
    {
        if (deliveryProgressTransform == null || deliveryProgressImage == null)
        {
            CustomLogger.LogError("Delivery Progress UI not set", LogCategory.UI);
            return;
        }

        deliveryProgressImage.gameObject.SetActive(value > 0);
        
        deliveryProgressImage.fillAmount = Mathf.Clamp01(value / 5.0f);
    }

    private void UpdateBoostUI(float value)
    {
        if (boostUI == null)
            return;
        
        Debug.Log(value);
        boostUI.fillAmount = (value / 5.0f);
    }

    private void UpdateIndicatorArrow(Vector3 playerScreen)
    {
        if (indicator == null) return;

        if (deliveryTarget == null)
        {
            if (indicator.gameObject.activeSelf)
                indicator.gameObject.SetActive(false);
            return;
        }

        // If close enough in world space, hide arrow
        float worldDist = Vector3.Distance(playerObj.transform.position, deliveryTarget.position);
        if (worldDist <= hideIfCloserThan)
        {
            if (indicator.gameObject.activeSelf)
                indicator.gameObject.SetActive(false);
            return;
        }

        if (!indicator.gameObject.activeSelf)
            indicator.gameObject.SetActive(true);

        // Convert target to screen
        Vector3 targetScreen = targetCamera.WorldToScreenPoint(deliveryTarget.position);

        // If behind camera, flip direction so arrow still points "towards" it
        if (targetScreen.z < 0f)
        {
            targetScreen.x = Screen.width - targetScreen.x;
            targetScreen.y = Screen.height - targetScreen.y;
        }

        // Direction from player HUD anchor to target on screen
        Vector2 dir = ((Vector2)targetScreen - (Vector2)playerScreen).normalized;

        // Place arrow around the player at a fixed radius
        indicator.position = (Vector2)playerScreen + dir * indicatorRadius;

        // Rotate arrow to face direction (UI: right = 0 degrees)
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        indicator.rotation = Quaternion.Euler(0f, 0f, angle);

        // If your arrow sprite points "up" by default, use this instead:
        // indicator.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }
}
