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
        // Lepas listener lama dulu agar tidak double
        if (masterSlider != null) masterSlider.onValueChanged.RemoveAllListeners();
        if (musicSlider  != null) musicSlider.onValueChanged.RemoveAllListeners();
        if (sfxSlider    != null) sfxSlider.onValueChanged.RemoveAllListeners();

        // Sambung slider baru
        masterSlider = master;
        musicSlider  = music;
        sfxSlider    = sfx;

        // Sync nilai slider ke volume saat ini (baca dari PlayerPrefs)
        masterSlider.value = PlayerPrefs.GetFloat(MASTER_KEY, 0.75f);
        musicSlider.value  = PlayerPrefs.GetFloat(MUSIC_KEY,  0.75f);
        sfxSlider.value    = PlayerPrefs.GetFloat(SFX_KEY,    0.75f);

        // Daftarkan listener baru
        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    // Slider kirim nilai 0.0 – 1.0, AudioMixer pakai desibel
    // Rumus: dB = log10(linearValue) * 20
    // Saat value = 0, kita set ke -80 dB (senyap) untuk menghindari log(0)

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
        return linear > 0.0001f ? Mathf.Log10(linear) * 20f : -80f;
    }
}