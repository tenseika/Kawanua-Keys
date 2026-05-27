using System.Collections;
using UnityEngine;

// Attach this to each effect prefab root.
// Each prefab should have child GameObjects for its visual parts (rings, lines, X, etc.)
// All visuals need a SpriteRenderer component.

public class EffectObject : MonoBehaviour
{
    public enum EffectType { Hit, Good, Perfect, Miss }

    public EffectType effectType;

    // Assign child SpriteRenderers in the Inspector.
    // Perfect : ring1, ring2, ring3, and 8 spark lines
    // Good    : ring1, ring2
    // Hit     : ring1, and 3-4 small dot/square sprites
    // Miss    : xSprite

    public SpriteRenderer[] rings;     // circle outline sprites, scaled outward
    public SpriteRenderer[] sparks;    // line sprites for Perfect burst arms
    public SpriteRenderer[] dots;      // small squares/dots for Hit scatter
    public SpriteRenderer xSprite;     // X sprite for Miss

    // Optional: assign a fullscreen UI Image for the screen flash (Miss / Perfect)
    public UnityEngine.UI.Image screenFlash;

    [Header("Timing")]
    public float duration = 0.45f;

    void Start()
    {
        switch (effectType)
        {
            case EffectType.Perfect: StartCoroutine(PlayPerfect()); break;
            case EffectType.Good:    StartCoroutine(PlayGood());    break;
            case EffectType.Hit:     StartCoroutine(PlayHit());     break;
            case EffectType.Miss:    StartCoroutine(PlayMiss());    break;
        }

        Destroy(gameObject, duration + 0.05f);
    }

    // -------------------------------------------------------------------------
    // PERFECT  — 3 rings expand + 8 sparks shoot outward, gold color
    // -------------------------------------------------------------------------
    IEnumerator PlayPerfect()
    {
        float elapsed = 0f;

        // Stagger the three rings
        float[] ringDelays    = { 0f,    0.06f, 0.12f };
        float[] ringMaxScales = { 2.2f,  1.8f,  1.4f  };

        // Pre-cache initial states
        Vector3[] sparkStartPositions = new Vector3[sparks.Length];
        for (int i = 0; i < sparks.Length; i++)
            sparkStartPositions[i] = sparks[i].transform.localPosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Rings: scale up and fade out
            for (int i = 0; i < rings.Length && i < ringDelays.Length; i++)
            {
                float ringT = Mathf.Clamp01((elapsed - ringDelays[i]) / (duration - ringDelays[i]));
                float scale = Mathf.Lerp(0.1f, ringMaxScales[i], EaseOut(ringT));
                float alpha = Mathf.Lerp(1f, 0f, EaseIn(ringT));

                rings[i].transform.localScale = Vector3.one * scale;
                SetAlpha(rings[i], alpha);
            }

            // Sparks: fly outward and shrink
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

    // -------------------------------------------------------------------------
    // GOOD  — 2 rings expand with a slight scale bounce, teal color
    // -------------------------------------------------------------------------
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

    // -------------------------------------------------------------------------
    // HIT  — 1 ring pops out fast, dots scatter and fade, blue color
    // -------------------------------------------------------------------------
    IEnumerator PlayHit()
    {
        float elapsed = 0f;

        // Give each dot a random outward direction
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

            // Dots: scatter outward
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

    // -------------------------------------------------------------------------
    // MISS  — X sprite scales up with a punch then fades, optional red flash
    // -------------------------------------------------------------------------
    IEnumerator PlayMiss()
    {
        float elapsed = 0f;
        float punchDuration = duration * 0.25f;   // quick scale-up punch
        float fadeDuration  = duration * 0.75f;   // then fade out

        // Screen flash
        if (screenFlash != null)
            StartCoroutine(ScreenFlash(new Color(0.88f, 0.29f, 0.29f, 0.35f), 0.12f));

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            if (xSprite != null)
            {
                float scale, alpha;

                if (elapsed < punchDuration)
                {
                    // Punch in: overshoot slightly
                    float t = elapsed / punchDuration;
                    scale = Mathf.Lerp(0f, 1.15f, EaseOut(t));
                    alpha = 1f;
                }
                else
                {
                    // Settle and fade
                    float t = (elapsed - punchDuration) / fadeDuration;
                    scale = Mathf.Lerp(1.15f, 1.0f, t * 3f);   // quick settle
                    scale = Mathf.Clamp(scale, 1.0f, 1.15f);
                    alpha = Mathf.Lerp(1f, 0f, EaseIn(t));
                }

                xSprite.transform.localScale = Vector3.one * scale;
                SetAlpha(xSprite, alpha);
            }

            yield return null;
        }
    }

    // -------------------------------------------------------------------------
    // Shared screen flash (used by Miss, can also be called for Perfect)
    // -------------------------------------------------------------------------
    IEnumerator ScreenFlash(Color flashColor, float flashDuration)
    {
        if (screenFlash == null) yield break;

        screenFlash.color = flashColor;
        float elapsed = 0f;

        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float a = Mathf.Lerp(flashColor.a, 0f, elapsed / flashDuration);
            screenFlash.color = new Color(flashColor.r, flashColor.g, flashColor.b, a);
            yield return null;
        }

        screenFlash.color = Color.clear;
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------
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