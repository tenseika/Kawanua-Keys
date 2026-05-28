using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HeartIndicator : MonoBehaviour
{
    [Header("References")]
    public Image heartImage;         // drag heart Image UI here

    [Header("Warna Threshold")]
    public Color colorNormal  = new Color(0.27f, 0.85f, 0.40f); // hijau  (HP > 60%)
    public Color colorWarning = new Color(1.00f, 0.80f, 0.10f); // kuning (HP 30-60%)
    public Color colorDanger  = new Color(0.90f, 0.25f, 0.25f); // merah  (HP < 30%)

    [Header("Pulse Settings")]
    public float gainPulseScale  = 1.3f;  // scale saat HP naik
    public float lossPulseScale  = 0.85f; // scale saat HP turun
    public float pulseDuration   = 0.2f;

    [Header("Shake Settings")]
    public float shakeStrength   = 8f;    // seberapa jauh bergeser (pixel)
    public float shakeDuration   = 0.3f;

    [Header("Heartbeat Settings")]
    public float heartbeatInterval = 0.6f; // jeda antar denyut saat danger

    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Coroutine pulseCoroutine;
    private Coroutine shakeCoroutine;
    private Coroutine heartbeatCoroutine;
    private bool isInDanger = false;

    void Start()
    {
        if (heartImage == null) return;
        originalScale    = heartImage.transform.localScale;
        originalPosition = heartImage.transform.localPosition;
    }

    // Dipanggil dari GameManager.UpdateHPDisplay() setiap kali HP berubah
    public void OnHPChanged(float currentHP, float maxHP, bool isGain)
    {
        if (heartImage == null) return;

        float ratio = currentHP / maxHP;

        // Update warna
        UpdateColor(ratio);

        // Animasi berdasarkan naik/turun
        if (isGain)
            TriggerPulse(gainPulseScale);
        else
        {
            TriggerPulse(lossPulseScale);
            TriggerShake();
        }

        // Heartbeat idle saat masuk/keluar danger
        bool enteringDanger = ratio <= 0.3f;
        if (enteringDanger && !isInDanger)
        {
            isInDanger = true;
            heartbeatCoroutine = StartCoroutine(HeartbeatIdle());
        }
        else if (!enteringDanger && isInDanger)
        {
            isInDanger = false;
            if (heartbeatCoroutine != null)
                StopCoroutine(heartbeatCoroutine);

            // Reset scale dan posisi
            heartImage.transform.localScale    = originalScale;
            heartImage.transform.localPosition = originalPosition;
        }
    }

    // -------------------------------------------------------------------------
    // Warna
    // -------------------------------------------------------------------------
    void UpdateColor(float ratio)
    {
        if (ratio > 0.6f)
            heartImage.color = colorNormal;
        else if (ratio > 0.3f)
            heartImage.color = colorWarning;
        else
            heartImage.color = colorDanger;
    }

    // -------------------------------------------------------------------------
    // Pulse (scale up/down lalu kembali normal)
    // -------------------------------------------------------------------------
    void TriggerPulse(float targetScale)
    {
        if (pulseCoroutine != null)
            StopCoroutine(pulseCoroutine);
        pulseCoroutine = StartCoroutine(PulseRoutine(targetScale));
    }

    IEnumerator PulseRoutine(float targetScale)
    {
        float half    = pulseDuration / 2f;
        float elapsed = 0f;
        Vector3 target = originalScale * targetScale;

        // Scale ke target
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            heartImage.transform.localScale = Vector3.Lerp(originalScale, target, elapsed / half);
            yield return null;
        }

        elapsed = 0f;

        // Kembali ke normal
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            heartImage.transform.localScale = Vector3.Lerp(target, originalScale, elapsed / half);
            yield return null;
        }

        heartImage.transform.localScale = originalScale;
    }

    // -------------------------------------------------------------------------
    // Shake (getar kiri-kanan saat HP turun)
    // -------------------------------------------------------------------------
    void TriggerShake()
    {
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);
        shakeCoroutine = StartCoroutine(ShakeRoutine());
    }

    IEnumerator ShakeRoutine()
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / shakeDuration;

            // Kekuatan shake melemah seiring waktu
            float currentStrength = Mathf.Lerp(shakeStrength, 0f, progress);
            float offsetX = Mathf.Sin(elapsed * 60f) * currentStrength;

            heartImage.transform.localPosition = originalPosition + new Vector3(offsetX, 0f, 0f);
            yield return null;
        }

        heartImage.transform.localPosition = originalPosition;
    }

    // -------------------------------------------------------------------------
    // Heartbeat Idle (denyut terus-menerus saat danger)
    // -------------------------------------------------------------------------
    IEnumerator HeartbeatIdle()
    {
        while (isInDanger)
        {
            // Denyut pertama (lebih besar)
            yield return PulseRoutine(1.25f);
            yield return new WaitForSeconds(0.1f);

            // Denyut kedua (lebih kecil, seperti "lub-dub")
            yield return PulseRoutine(1.12f);
            yield return new WaitForSeconds(heartbeatInterval);
            
        }
    }
}
