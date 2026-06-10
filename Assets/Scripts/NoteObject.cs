using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteObject : MonoBehaviour
{
    public bool canBePressed;

    public KeyCode keyToPress;

    public GameObject hitEffect, goodEffect, perfectEffect;
    public GameObject hitTextEffect, goodTextEffect, perfectTextEffect, missTextEffect;

    public Transform hitZone;

    [HideInInspector] public float beatTime;
    [HideInInspector] public float scrollSpeed;

    private bool _logged = false;

    void Update()
    {
        if (GameManager.instance.isPaused) return;

        // ← TARUH DI SINI
        if (!_logged)
        {
            Debug.Log($"[Note] HitZone Y:{hitZone.position.y} BeatTime:{beatTime} ScrollSpeed:{scrollSpeed}");
            _logged = true;
        }

        float timeUntilBeat = beatTime - GameManager.instance.theMusic.time;
        transform.position = new Vector3(transform.position.x, hitZone.position.y + timeUntilBeat * scrollSpeed, 0f);

        if (Input.GetKeyDown(keyToPress))
        {
            if (canBePressed)
            {
                gameObject.SetActive(false);

                float distanceFromHitZone = Mathf.Abs(transform.position.y - hitZone.position.y);

                if (distanceFromHitZone > 0.25f)
                {
                    Debug.Log("Hit");
                    GameManager.instance.NormalHit();
                    Instantiate(hitEffect, transform.position, hitEffect.transform.rotation);
                    Instantiate(hitTextEffect, transform.position, Quaternion.identity);
                }
                else if (distanceFromHitZone > 0.05f)
                {
                    Debug.Log("Good");
                    GameManager.instance.GoodHit();
                    Instantiate(goodEffect, transform.position, goodEffect.transform.rotation);
                    Instantiate(goodTextEffect, transform.position, Quaternion.identity);
                }
                else
                {
                    Debug.Log("Perfect");
                    GameManager.instance.PerfectHit();
                    Instantiate(perfectEffect, transform.position, perfectEffect.transform.rotation);
                    Instantiate(perfectTextEffect, transform.position, Quaternion.identity);
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Activator")
        {
            canBePressed = true;
        }
    }

    private static bool _isQuitting = false;

    void Awake() => _isQuitting = false;

    void OnApplicationQuit() => _isQuitting = true;

    void OnDestroy()
    {
        if (!gameObject.scene.isLoaded) _isQuitting = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Activator" && gameObject.activeSelf)
        {
            canBePressed = false;

            if (!GameManager.instance.isGameOver)
            {
                GameManager.instance.NoteMissed();
                if (!GameManager.instance.isGameOver && missTextEffect != null && !_isQuitting)
                    Instantiate(missTextEffect, hitZone.position, Quaternion.identity);
            }
        }
    }
}