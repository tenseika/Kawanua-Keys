using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI fpsText;
    
    [Header("Settings")]
    [Range(0.01f, 0.5f)] public float smoothingFactor = 0.1f; 
    public float updateInterval = 0.2f; 

    private float smoothedFps;
    private float nextUpdateTime = 0.0f;

    void Awake()
    {
        // Unlocks performance on mobile targets past the 30fps default fallback
        Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;
    }

    void Update()
    {
        // 1. Capture and smooth performance EVERY frame so no lag spikes are missed
        float currentFps = 1f / Time.unscaledDeltaTime;
        smoothedFps = (smoothingFactor * currentFps) + (1f - smoothingFactor) * smoothedFps;

        // 2. Pace the UI string generation to eliminate Garbage Collection memory stutters
        if (Time.unscaledTime >= nextUpdateTime)
        {
            fpsText.text = string.Format("FPS: {0:0}", smoothedFps);
            nextUpdateTime = Time.unscaledTime + updateInterval;
        }
    }
}
