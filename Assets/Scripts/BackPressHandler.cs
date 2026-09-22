using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackPressHandler : MonoBehaviour
{
    [Tooltip("Pesan Toast yang ditampilkan saat pertama kali menekan back.")]
    public string toastMessage = "Tekan sekali lagi untuk keluar";

    [Tooltip("Jarak waktu (detik) antara dua tekanan agar dihitung double-press.")]
    public float exitWindow = 2f;

    private float _lastBackTime = -999f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            float now = Time.unscaledTime;

            if (now - _lastBackTime <= exitWindow)
            {
                Application.Quit();

#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            }
            else
            {
                // Tekan pertama
                _lastBackTime = now;
                ShowToast(toastMessage);
            }
        }
    }

    // ── Android Native Toast via JNI ─────────────────────────
    // Menggunakan android.widget.Toast sehingga tampilan
    // mengikuti UI / tema bawaan perangkat masing-masing pemain.

    void ShowToast(string message)
    {
    #if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaClass  unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity    = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
        {
            AndroidJavaClass  toastClass  = new AndroidJavaClass("android.widget.Toast");
            AndroidJavaObject context     = activity.Call<AndroidJavaObject>("getApplicationContext");
            AndroidJavaObject javaMessage = new AndroidJavaObject("java.lang.String", message);

            AndroidJavaObject toast = toastClass.CallStatic<AndroidJavaObject>(
                "makeText", context, javaMessage, 0
            );
            toast.Call("show");
        }));
    #else
            // Fallback di Editor: cetak ke Console
            Debug.Log($"[BackPressHandler] Toast: {message}");
    #endif
    }
}
