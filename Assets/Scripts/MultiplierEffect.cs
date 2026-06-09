using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// ============================================================
//  MultiplierEffect
// ============================================================
//  Setup di Unity:
//  1. Tambahkan script ini ke GameObject yang sama dengan
//     multiText (TMP_Text multiplier) di GameManager.
//  2. Drag field "multiplierText" ke TMP_Text multiplier.
//  3. Di GameManager Inspector, drag GameObject ini ke field
//     "multiplierEffect".
// ============================================================
 
public class MultiplierEffect : MonoBehaviour
{
    public static MultiplierEffect instance;
 
    [Header("References")]
    public TMP_Text multiplierText;
 
    [Header("Blink (multiplier naik)")]
    [Tooltip("Berapa kali teks berkedip (muncul-hilang).")]
    public int blinkCount = 3;
    [Tooltip("Durasi teks terlihat / tidak terlihat per kedip (detik).")]
    public float blinkInterval = 0.08f;
 
    [Header("Shake (multiplier turun)")]
    [Tooltip("Durasi total getaran (detik).")]
    public float shakeDuration = 0.35f;
    [Tooltip("Jarak maksimal geser kiri-kanan (unit).")]
    public float shakeMagnitude = 6f;
    [Tooltip("Seberapa cepat getaran berosilasi.")]
    public float shakeFrequency = 40f;
 
    // ── State internal ──────────────────────────────────────
    private Vector3        _originalLocalPos;
    private Coroutine      _activeRoutine;
 
    void Awake()
    {
        instance = this;
    }
 
    void Start()
    {
        if (multiplierText != null)
            _originalLocalPos = multiplierText.transform.localPosition;
    }
 
    // Dipanggil dari GameManager saat multiplier naik
    public void PlayBlink()
    {
        if (multiplierText == null) return;
        RestartRoutine(DoBlink());
    }
 
    // Dipanggil dari GameManager saat multiplier turun/reset
    public void PlayShake()
    {
        if (multiplierText == null) return;
        RestartRoutine(DoShake());
    }
 
    // ── Coroutines ──────────────────────────────────────────
 
    IEnumerator DoBlink()
    {
        for (int i = 0; i < blinkCount; i++)
        {
            multiplierText.enabled = false;
            yield return new WaitForSecondsRealtime(blinkInterval);
 
            multiplierText.enabled = true;
            yield return new WaitForSecondsRealtime(blinkInterval);
        }
 
        multiplierText.enabled = true;
    }
 
    IEnumerator DoShake()
    {
        float elapsed = 0f;
 
        while (elapsed < shakeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress  = elapsed / shakeDuration;
            float envelope  = 1f - progress;               // makin lama makin redam
            float offsetX   = Mathf.Sin(elapsed * shakeFrequency) * shakeMagnitude * envelope;
 
            multiplierText.transform.localPosition = _originalLocalPos + new Vector3(offsetX, 0f, 0f);
            yield return null;
        }
 
        multiplierText.transform.localPosition = _originalLocalPos;
    }
 
    // ── Helper ──────────────────────────────────────────────
 
    void RestartRoutine(IEnumerator routine)
    {
        if (_activeRoutine != null)
        {
            StopCoroutine(_activeRoutine);
            multiplierText.enabled = true;
            multiplierText.transform.localPosition = _originalLocalPos;
        }
        _activeRoutine = StartCoroutine(routine);
    }
}