using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
 
    // Panggil method ini dari onClick sebagai pengganti LoadScene.LoadScenes()
    // SFX diputar dulu, scene dimuat setelah durasi clip selesai
    public void PlayTapSoundThenLoad(string sceneName)
    {
        StartCoroutine(PlayThenLoad(sceneName));
    }
 
    IEnumerator PlayThenLoad(string sceneName)
    {
        if (tapSound != null)
        {
            audioSource.PlayOneShot(tapSound);
            yield return new WaitForSeconds(tapSound.length);
        }
 
        SceneManager.LoadScene(sceneName);
    }
}