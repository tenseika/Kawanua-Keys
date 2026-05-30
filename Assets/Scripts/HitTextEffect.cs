using System.Collections;
using UnityEngine;

public class HitTextEffect : MonoBehaviour
{
    public SpriteRenderer hitSprite;

    public float duration = 0.6f;
    public float riseDistance = 1.2f;

    void Start()
    {
        StartCoroutine(PlayAnimation());
        Destroy(gameObject, duration + 0.05f);
    }

    IEnumerator PlayAnimation()
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + new Vector3(0f, riseDistance, 0f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Move upward
            transform.position = Vector3.Lerp(startPos, endPos, EaseOut(t));

            // Stay fully visible for first 30% then fade out
            float alpha = t < 0.3f ? 1f : Mathf.Lerp(1f, 0f, (t - 0.3f) / 0.7f);
            SetAlpha(alpha);

            yield return null;
        }
    }

    void SetAlpha(float a)
    {
        if (hitSprite == null) return;
        Color c = hitSprite.color;
        c.a = a;
        hitSprite.color = c;
    }

    float EaseOut(float t) => 1f - (1f - t) * (1f - t);
}