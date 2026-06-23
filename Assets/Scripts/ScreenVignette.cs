using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

// ============================================================
//  ScreenVignette — Post Processing Edition
// ============================================================
//  Setup di Unity:
//  1. Install Post Processing via Package Manager.
//  2. Main Camera: tambahkan Post Process Layer, atur Layer
//     ke layer khusus (misal "PostProcessing").
//  3. Buat GameObject "PP Volume", set Layer yang sama,
//     centang Is Global, tambahkan Post Process Volume.
//  4. Buat Profile baru di Volume, tambahkan effect Vignette.
//  5. Di Vignette, centang Intensity dan Color agar scriptable.
//  6. Tambahkan script ini ke GameObject mana saja (misal
//     GameManager atau PP Volume itu sendiri).
//  7. Drag Post Process Volume ke field "volume" di Inspector.
// ============================================================

public class ScreenVignette : MonoBehaviour
{
    public static ScreenVignette instance;

    [Header("References")]
    [Tooltip("Post Process Volume yang memiliki effect Vignette.")]
    public PostProcessVolume volume;

    [Header("Intensity per kondisi HP")]
    [Range(0f, 1f)] public float intensityHigh   = 0.15f; // HP > 60%
    [Range(0f, 1f)] public float intensityMedium = 0.38f; // HP 30–60%
    [Range(0f, 1f)] public float intensityLow    = 0.62f; // HP < 30%

    [Header("Warna per kondisi HP")]
    public Color colorHigh   = new Color(0f,    0f,   0f); // hitam netral
    public Color colorMedium = new Color(0.4f,  0.15f, 0f); // oranye gelap
    public Color colorLow    = new Color(0.65f, 0f,   0f); // merah

    [Header("Transisi")]
    public float smoothSpeed = 3.5f;

    // ── State internal ──────────────────────────────────────
    private Vignette _vignette;
    private float    _currentIntensity;
    private Color    _currentColor;

    void Awake()
    {
        instance = this;

        if (volume != null && volume.profile.TryGetSettings(out _vignette))
        {
            _currentIntensity = _vignette.intensity.value;
            _currentColor     = _vignette.color.value;
        }
        else
        {
            Debug.LogWarning("[ScreenVignette] Vignette effect tidak ditemukan di Post Process Volume. " +
                             "Pastikan effect Vignette sudah ditambahkan ke profile dan semua " +
                             "parameter yang ingin dikontrol sudah dicentang (override).");
        }
    }

    void Update()
    {
        if (_vignette == null) return;

        float ratio           = GetHPRatio();
        float targetIntensity = GetTargetIntensity(ratio);
        Color targetColor     = GetTargetColor(ratio);

        /*_currentIntensity = Mathf.Lerp(_currentIntensity, targetIntensity, Time.deltaTime * smoothSpeed);
        _currentColor     = Color.Lerp(_currentColor,     targetColor,     Time.deltaTime * smoothSpeed);*/

        _currentIntensity = Mathf.Lerp(_currentIntensity, 
                                        targetIntensity, 
                                        Time.deltaTime * smoothSpeed);
        _currentColor     = Color.Lerp(_currentColor,
                                        targetColor,
                                        Time.deltaTime * smoothSpeed);

        _vignette.intensity.value = Mathf.Clamp01(_currentIntensity);
        _vignette.color.value     = _currentColor;
    }

    // ── Helpers ─────────────────────────────────────────────

    float GetHPRatio()
    {
        if (GameManager.instance == null) return 1f;
        return GameManager.instance.currentHP / GameManager.instance.maxHP;
    }

    float GetTargetIntensity(float ratio)
    {
        if (ratio > 0.6f) return intensityHigh;
        if (ratio > 0.3f) return intensityMedium;
        return intensityLow;
    }

    Color GetTargetColor(float ratio)
    {
        if (ratio > 0.6f) return colorHigh;
        if (ratio > 0.3f) return colorMedium;
        return colorLow;
    }
}
