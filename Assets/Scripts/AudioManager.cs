using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Mixer")]
    public AudioMixer mainMixer;

    [Header("Sliders")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Boost (dB tambahan di atas 0dB saat slider penuh)")]
    [Range(0f, 20f)] public float maxBoostDb = 6f; // mulai dari 6dB, naikkan jika masih kurang

    // PlayerPrefs keys
    const string MASTER_KEY = "vol_master";
    const string MUSIC_KEY  = "vol_music";
    const string SFX_KEY    = "vol_sfx";

    void Awake()
    {
        // Singleton — bertahan antar scene
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            instance.ReconnectSliders(masterSlider, musicSlider, sfxSlider);
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Load nilai tersimpan, default 0.75 jika belum pernah disimpan
        float master = PlayerPrefs.GetFloat(MASTER_KEY, 0.75f);
        float music  = PlayerPrefs.GetFloat(MUSIC_KEY,  0.75f);
        float sfx    = PlayerPrefs.GetFloat(SFX_KEY,    0.75f);

        // Set nilai slider (ini akan trigger OnValueChanged secara otomatis)
        masterSlider.value = master;
        musicSlider.value  = music;
        sfxSlider.value    = sfx;

        // Daftarkan listener
        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    public void ReconnectSliders(Slider master, Slider music, Slider sfx)
    {
        if (masterSlider != null) masterSlider.onValueChanged.RemoveAllListeners();
        if (musicSlider  != null) musicSlider.onValueChanged.RemoveAllListeners();
        if (sfxSlider    != null) sfxSlider.onValueChanged.RemoveAllListeners();

        masterSlider = master;
        musicSlider  = music;
        sfxSlider    = sfx;

        masterSlider.value = PlayerPrefs.GetFloat(MASTER_KEY, 0.75f);
        musicSlider.value  = PlayerPrefs.GetFloat(MUSIC_KEY,  0.75f);
        sfxSlider.value    = PlayerPrefs.GetFloat(SFX_KEY,    0.75f);

        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    public void SetMasterVolume(float value)
    {
        mainMixer.SetFloat("MasterVolume", LinearToDecibel(value));
        PlayerPrefs.SetFloat(MASTER_KEY, value);
    }

    public void SetMusicVolume(float value)
    {
        mainMixer.SetFloat("MusicVolume", LinearToDecibel(value));
        PlayerPrefs.SetFloat(MUSIC_KEY, value);
    }

    public void SetSFXVolume(float value)
    {
        mainMixer.SetFloat("SFXVolume", LinearToDecibel(value));
        PlayerPrefs.SetFloat(SFX_KEY, value);
    }

    float LinearToDecibel(float linear)
    {
        if (linear <= 0.0001f) return -80f;
        return Mathf.Log10(linear) * 20f + maxBoostDb;
    }
}