using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public AudioSource theMusic;

    public bool startPlaying;

    public static GameManager instance;

    public int currentScore;
    public int scorePerNote;
    public int scorePerGoodNote;
    public int scorePerPerfectNote;

    public int currentMultiplier;
    public int multiplierTracker;
    public int[] multiplierThresholds;

    public TMP_Text scoreText;
    public TMP_Text multiText;
    public MultiplierEffect multiplierEffect;

    public float totalNotes;
    public float normalHits;
    public float goodHits;
    public float perfectHits;
    public float missedHits;

    public bool isPaused;
    [HideInInspector] public bool isCountingDown;
    public GameObject pauseScreen;
    public TMP_Text countdownText;
    public GameObject pauseButton;
    public GameObject resultsScreen;
    public GameObject gameOverScreen;
    public TMP_Text gameOverScoreText;
    public TMP_Text percentHitText, rankText, finalScoreText;

    [Header("Health Bar")]
    public float maxHP      = 100f;
    public float currentHP  = 70f;

    // Nilai-nilai ini akan di-override oleh SongHPConfig jika diisi
    [HideInInspector]public float hpPerPerfect = 3f;
    [HideInInspector]public float hpPerGood    = 1f;
    [HideInInspector]public float hpPerNormal  = -3f;
    [HideInInspector]public float hpPerMiss    = -10f;

    public TMP_Text hpText;
    public Image heartImage;

    [Header("Game Over SFX")]
    public AudioClip gameOverSFX;
    public AudioSource sfxSource;

    [Header("Result SFX")]
    public AudioClip resultSFX;

    [Header("Countdown SFX")]
    public AudioClip countdownSFX;

    public TMP_Text tapToStartText;

    public Color hpColorHigh   = new Color(0.27f, 0.85f, 0.40f);
    public Color hpColorMedium = new Color(1.00f, 0.80f, 0.10f);
    public Color hpColorLow    = new Color(0.90f, 0.25f, 0.25f);

    [HideInInspector] public bool isGameOver = false;

    [Header("Song Record")]
    public string songID;

    [Header("HP Config (opsional)")]
    public SongHPConfig hpConfig;

    private bool recordSaved  = false;
    private bool resultShown  = false;

    void Start()
    {
        instance = this;
        ApplyHPConfig();
        scoreText.text = "Skor: 0";
        currentMultiplier = 1;
        UpdateHPDisplay();
        if (pauseButton != null) pauseButton.SetActive(false);
    }

    // Membaca nilai dari SongHPConfig dan menimpa field lokal
    void ApplyHPConfig()
    {
        if (hpConfig == null) return;

        hpPerPerfect = hpConfig.hpPerPerfect;
        hpPerGood    = hpConfig.hpPerGood;
        hpPerNormal  = hpConfig.hpPerNormal;
        hpPerMiss    = hpConfig.hpPerMiss;
    }

    void Update()
    {
        if (isGameOver) return;

        if (!startPlaying)
        {
            if (Input.anyKeyDown)
            {
                startPlaying = true;
                tapToStartText.gameObject.SetActive(false);
                if (pauseButton != null) pauseButton.SetActive(true); // tampilkan sekarang
                StartCoroutine(StartSong());
            }
        }
        else
        {
            if (!theMusic.isPlaying && !resultsScreen.activeInHierarchy && !isPaused)
            {
                resultsScreen.SetActive(true);
                if (pauseButton != null) pauseButton.SetActive(false);

                float weightedScore    = (perfectHits * 100f) + (goodHits * 75f) + (normalHits * 50f) + (missedHits * -25f);
                float maxPossibleScore = totalNotes * 100f;
                float percentHit       = Mathf.Clamp((weightedScore / maxPossibleScore) * 100f, 0f, 100f);

                percentHitText.text = percentHit.ToString("F1") + "%";

                string rankVal = "F";
                if (percentHit >= 40f) { rankVal = "D";
                if (percentHit >= 55f) { rankVal = "C";
                if (percentHit >= 70f) { rankVal = "B";
                if (percentHit >= 85f) { rankVal = "A";
                if (percentHit >= 95f)   rankVal = "S"; }}}}

                rankText.text       = rankVal;
                finalScoreText.text = currentScore.ToString();

                if (!resultShown)
                {
                    resultShown = true;
                    if (sfxSource != null && resultSFX != null)
                        sfxSource.PlayOneShot(resultSFX);
                }

                if (!recordSaved)
                {
                    SaveRecord(percentHit, rankVal, currentScore);
                    recordSaved = true;
                }
            }
        }
    }

    IEnumerator StartSong()
    {
        double startTime = AudioSettings.dspTime + 1.0;
        theMusic.PlayScheduled(startTime);

        yield return new WaitForSeconds(1f);
        totalNotes = FindObjectsOfType<NoteObject>().Length;
    }

    public void NoteHit()
    {
        if (currentMultiplier - 1 < multiplierThresholds.Length)
        {
            multiplierTracker++;
            if (multiplierThresholds[currentMultiplier - 1] <= multiplierTracker)
            {
                multiplierTracker = 0;
                currentMultiplier++;
                if (multiplierEffect != null) multiplierEffect.PlayBlink();
            }
        }

        multiText.text = "x" + currentMultiplier;
        scoreText.text = "Skor: " + currentScore;
    }

    public void NormalHit()
    {
        currentScore += scorePerNote * currentMultiplier;
        NoteHit();
        normalHits++;
        ModifyHP(hpPerNormal);
    }

    public void GoodHit()
    {
        currentScore += scorePerGoodNote * currentMultiplier;
        NoteHit();
        goodHits++;
        ModifyHP(hpPerGood);
    }

    public void PerfectHit()
    {
        currentScore += scorePerPerfectNote * currentMultiplier;
        NoteHit();
        perfectHits++;
        ModifyHP(hpPerPerfect);
    }

    public void NoteMissed()
    {
        currentMultiplier = 1;
        multiplierTracker = 0;
        multiText.text = "x" + currentMultiplier;
        if (multiplierEffect != null) multiplierEffect.PlayShake();
        missedHits++;
        ModifyHP(hpPerMiss);
    }

    void ModifyHP(float amount)
    {
        if (isGameOver) return;
        currentHP = Mathf.Clamp(currentHP + amount, 0f, maxHP);
        UpdateHPDisplay();
        if (currentHP <= 0f) TriggerGameOver();
    }

    void UpdateHPDisplay(bool isGain = false)
    {
        if (hpText == null) return;

        float ratio = currentHP / maxHP;
        hpText.text = Mathf.CeilToInt(currentHP).ToString();

        Color hpColor = ratio > 0.6f ? hpColorHigh :
                        ratio > 0.3f ? hpColorMedium : hpColorLow;

        hpText.color = hpColor;
        if (heartImage != null) heartImage.color = hpColor;
    }

    void TriggerGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        theMusic.Stop();

        if (sfxSource != null && gameOverSFX != null)
            sfxSource.PlayOneShot(gameOverSFX);

        if (pauseButton != null) pauseButton.SetActive(false);

        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true);
            if (gameOverScoreText != null)
                gameOverScoreText.text = currentScore.ToString();
        }
    }

    public void Pause()
    {
        if (isCountingDown) return; 
        
        isPaused = true;
        theMusic.Pause();
        pauseScreen.SetActive(true);
    }

    public void Resume()
    {
        StartCoroutine(ResumeCountdown());
    }

    IEnumerator ResumeCountdown()
    {
        isCountingDown = true;
        if (pauseButton != null) pauseButton.SetActive(false);
        
        pauseScreen.SetActive(false);
        countdownText.gameObject.SetActive(true);

        countdownText.text = "3"; PlayCountdownSFX(); yield return new WaitForSecondsRealtime(1f);
        countdownText.text = "2"; PlayCountdownSFX(); yield return new WaitForSecondsRealtime(1f);
        countdownText.text = "1"; PlayCountdownSFX(); yield return new WaitForSecondsRealtime(1f);

        countdownText.gameObject.SetActive(false);
        isPaused = false;
        isCountingDown = false;
        if (pauseButton != null) pauseButton.SetActive(true);
        theMusic.UnPause();
    }

    void PlayCountdownSFX()
    {
        if (sfxSource != null && countdownSFX != null)
            sfxSource.PlayOneShot(countdownSFX);
    }

    void SaveRecord(float accuracy, string rank, int score)
    {
        if (string.IsNullOrEmpty(songID))
        {
            Debug.LogWarning("GameManager: songID belum diisi! Rekor tidak disimpan.");
            return;
        }

        string accKey   = songID + "_accuracy";
        string rankKey  = songID + "_rank";
        string scoreKey = songID + "_score";

        float savedAcc = PlayerPrefs.GetFloat(accKey, -1f);
        if (accuracy > savedAcc) PlayerPrefs.SetFloat(accKey, accuracy);

        string savedRank = PlayerPrefs.GetString(rankKey, "");
        if (IsHigherRank(rank, savedRank)) PlayerPrefs.SetString(rankKey, rank);

        int savedScore = PlayerPrefs.GetInt(scoreKey, 0);
        if (score > savedScore) PlayerPrefs.SetInt(scoreKey, score);

        PlayerPrefs.Save();
        Debug.Log($"[Record Saved] {songID} | Accuracy: {accuracy:F1}% | Rank: {rank} | Score: {score}");
    }

    bool IsHigherRank(string newRank, string oldRank)
    {
        string[] order = { "F", "D", "C", "B", "A", "S" };
        int newIndex = System.Array.IndexOf(order, newRank);
        int oldIndex = System.Array.IndexOf(order, oldRank);
        return newIndex > oldIndex;
    }

    // ── Cek apakah semua input lane harus diblokir ───────────
    public bool IsInputBlocked()
    {
        if (isCountingDown) return true;
        if (isPaused) return true;
        if (isGameOver) return true;
        if (resultsScreen != null && resultsScreen.activeInHierarchy) return true;

        return false;
    }
}