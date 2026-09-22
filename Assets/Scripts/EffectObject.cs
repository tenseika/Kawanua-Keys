using System.Collections;
using UnityEngine;


public class EffectObject : MonoBehaviour
{
    public enum EffectType { Hit, Good, Perfect }

    public EffectType effectType;

    public SpriteRenderer[] rings;
    public SpriteRenderer[] sparks;
    public SpriteRenderer[] dots;

    [Header("Timing")]
    public float duration = 0.45f;

    void Start()
    {
        switch (effectType)
        {
            case EffectType.Perfect: StartCoroutine(PlayPerfect()); break;
            case EffectType.Good:    StartCoroutine(PlayGood());    break;
            case EffectType.Hit:     StartCoroutine(PlayHit());     break;
        }

        Destroy(gameObject, duration + 0.05f);
    }

    IEnumerator PlayPerfect()
    {
        float elapsed = 0f;

        float[] ringDelays    = { 0f,    0.06f, 0.12f };
        float[] ringMaxScales = { 2.2f,  1.8f,  1.4f  };

        Vector3[] sparkStartPositions = new Vector3[sparks.Length];
        for (int i = 0; i < sparks.Length; i++)
            sparkStartPositions[i] = sparks[i].transform.localPosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Rings: scale up + fade out
            for (int i = 0; i < rings.Length && i < ringDelays.Length; i++)
            {
                float ringT = Mathf.Clamp01((elapsed - ringDelays[i]) / (duration - ringDelays[i]));
                float scale = Mathf.Lerp(0.1f, ringMaxScales[i], EaseOut(ringT));
                float alpha = Mathf.Lerp(1f, 0f, EaseIn(ringT));

                rings[i].transform.localScale = Vector3.one * scale;
                SetAlpha(rings[i], alpha);
            }

            // Sparks: fly outward + shrink
            for (int i = 0; i < sparks.Length; i++)
            {
                float sparkT = Mathf.Clamp01(elapsed / (duration * 0.75f));
                float dist   = Mathf.Lerp(0f, 1.1f, EaseOut(sparkT));
                float alpha  = Mathf.Lerp(1f, 0f, EaseIn(sparkT));
                float scaleY = Mathf.Lerp(1f, 0.3f, sparkT);

                sparks[i].transform.localPosition = sparkStartPositions[i] * (1f + dist * 2.5f);
                sparks[i].transform.localScale = new Vector3(1f, scaleY, 1f);
                SetAlpha(sparks[i], alpha);
            }

            yield return null;
        }
    }

    IEnumerator PlayGood()
    {
        float elapsed = 0f;
        float[] ringDelays    = { 0f, 0.08f };
        float[] ringMaxScales = { 2.0f, 1.6f };

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            for (int i = 0; i < rings.Length && i < ringDelays.Length; i++)
            {
                float ringT = Mathf.Clamp01((elapsed - ringDelays[i]) / (duration - ringDelays[i]));
                float scale = Mathf.Lerp(0.1f, ringMaxScales[i], EaseOut(ringT));
                float alpha = Mathf.Lerp(1f, 0f, EaseIn(ringT));

                rings[i].transform.localScale = Vector3.one * scale;
                SetAlpha(rings[i], alpha);
            }

            yield return null;
        }
    }

    IEnumerator PlayHit()
    {
        float elapsed = 0f;

        // random outward dot direction
        Vector2[] dotDirections = new Vector2[dots.Length];
        for (int i = 0; i < dots.Length; i++)
            dotDirections[i] = Random.insideUnitCircle.normalized;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Single ring: fast pop
            if (rings.Length > 0)
            {
                float scale = Mathf.Lerp(0.1f, 1.8f, EaseOut(t));
                float alpha = Mathf.Lerp(1f,   0f,   EaseIn(t));
                rings[0].transform.localScale = Vector3.one * scale;
                SetAlpha(rings[0], alpha);
            }

            // Dots: outward
            for (int i = 0; i < dots.Length; i++)
            {
                float dist  = Mathf.Lerp(0f, 0.8f, EaseOut(t));
                float alpha = Mathf.Lerp(1f, 0f,   EaseIn(Mathf.Clamp01(t / 0.8f)));

                dots[i].transform.localPosition = (Vector3)(dotDirections[i] * dist);
                SetAlpha(dots[i], alpha);
            }

            yield return null;
        }
    }

    void SetAlpha(SpriteRenderer sr, float a)
    {
        if (sr == null) return;
        Color c = sr.color;
        c.a = a;
        sr.color = c;
    }

    float EaseOut(float t) => 1f - (1f - t) * (1f - t);
    float EaseIn(float t)  => t * t;
}