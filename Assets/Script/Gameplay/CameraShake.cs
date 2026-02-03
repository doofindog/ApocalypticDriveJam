using System;
using System.Collections;
using Azen.Logger;
using Unity.Cinemachine;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCameraBase virtualCamera;
    [SerializeField] private float shakeDuration;

    private CinemachineBasicMultiChannelPerlin multiChannelPerlin;

    private void Awake()
    {
        multiChannelPerlin ??= virtualCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
        multiChannelPerlin.AmplitudeGain = 0;
    }

    public void Shake(float intensity, float time)
    {
        if (!multiChannelPerlin)
            CustomLogger.LogError("Multi Channel Perlin not found on Camera Shake", LogCategory.Gameplay);
        
        multiChannelPerlin.AmplitudeGain = intensity;
        shakeDuration = time;

        StartCoroutine(PerformShake());
    }

    private IEnumerator PerformShake()
    {
        if (!multiChannelPerlin)
            yield break;

        float startAmplitude = multiChannelPerlin.AmplitudeGain;
        float timeLeft = shakeDuration;

        while (timeLeft > 0f)
        {
            timeLeft -= Time.deltaTime;

            float t = timeLeft / shakeDuration; // 1 → 0
            multiChannelPerlin.AmplitudeGain = startAmplitude * t;

            yield return null;
        }

        multiChannelPerlin.AmplitudeGain = 0f;
    }
}
