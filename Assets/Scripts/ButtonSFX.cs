using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonSFX : MonoBehaviour
{
    public AudioClip tapSound;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayTapSound()
    {
        if (tapSound != null)
            audioSource.PlayOneShot(tapSound);
    }
}