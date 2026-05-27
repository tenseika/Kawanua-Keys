using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteObject : MonoBehaviour
{
    public bool canBePressed;

    public KeyCode keyToPress;

    public GameObject hitEffect, goodEffect, perfectEffect, missEffect;

    public Transform hitZone; // drag your Activator object here in Inspector

    [HideInInspector] public float beatTime;    // assigned by NoteSpawner
    [HideInInspector] public float scrollSpeed; // assigned by NoteSpawner

    void Update()
    {
        if (GameManager.instance.isPaused) return; // add this line
        
        // Position note so it arrives exactly at hitZone.y on beat
        float timeUntilBeat = beatTime - GameManager.instance.theMusic.time;
        transform.position = new Vector3(transform.position.x, hitZone.position.y + timeUntilBeat * scrollSpeed, 0f);

        if (Input.GetKeyDown(keyToPress))
        {
            if (canBePressed)
            {
                gameObject.SetActive(false);

                // Judge hit quality based on distance from the hit zone center
                float distanceFromHitZone = Mathf.Abs(transform.position.y - hitZone.position.y);

                if (distanceFromHitZone > 0.25f)
                {
                    Debug.Log("Hit");
                    GameManager.instance.NormalHit();
                    Instantiate(hitEffect, transform.position, hitEffect.transform.rotation);
                }
                else if (distanceFromHitZone > 0.05f)
                {
                    Debug.Log("Good");
                    GameManager.instance.GoodHit();
                    Instantiate(goodEffect, transform.position, goodEffect.transform.rotation);
                }
                else
                {
                    Debug.Log("Perfect");
                    GameManager.instance.PerfectHit();
                    Instantiate(perfectEffect, transform.position, perfectEffect.transform.rotation);
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

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Activator" && gameObject.activeSelf)
        {
            canBePressed = false;
            GameManager.instance.NoteMissed();
        }
    }
}