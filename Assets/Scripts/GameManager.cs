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

    public float totalNotes;
    public float normalHits;
    public float goodHits;
    public float perfectHits;
    public float missedHits;

    public bool isPaused;
    public GameObject pauseScreen;
    public TMP_Text countdownText;

    public GameObject resultsScreen;
    public TMP_Text percentHitText, rankText, finalScoreText;

    [Header("Health Bar")]
    public float maxHP      = 100f;
    public float currentHP  = 70f;

    // Nilai-nilai ini akan di-override oleh SongHPConfig jika diisi
    public float hpPerPerfect = 3f;
    public float hpPerGood    = 1f;
    public float hpPerNormal  = -3f;
    public float hpPerMiss    = -10f;

    public TMP_Text hpText;
    public Image heartImage;
    public GameObject gameOverScreen;
    public TMP_Text gameOverScoreText;

    public TMP_Text tapToStartText;

    public Color hpColorHigh   = new Color(0.27f, 0.85f, 0.40f);
    public Color hpColorMedium = new Color(1.00f, 0.80f, 0.10f);
    public Color hpColorLow    = new Color(0.90f, 0.25f, 0.25f);

    [HideInInspector] public bool isGameOver = false;

    [Header("Song Record")]
    public string songID;

    // -------------------------------------------------------------------------
    // HP Config per lagu — drag asset SongHPConfig ke sini di Inspector
    // Kosongkan jika ingin pakai nilai default di atas
    // -------------------------------------------------------------------------
    [Header("HP Config (opsional)")]
    public SongHPConfig hpConfig;

    private bool recordSaved = false;

    void Start()
    {
        instance = this;

        // Terapkan config HP dari ScriptableObject jika ada
        ApplyHPConfig();

        scoreText.text = "Skor: 0";
        currentMultiplier = 1;

        UpdateHPDisplay();
    }

    // Membaca nilai dari SongHPConfig dan menimpa field lokal
    void ApplyHPConfig()
    {
        if (hpConfig == null) return;

        hpPerPerfect = hpConfig.hpPerPerfect;
        hpPerGood    = hpConfig.hpPerGood;
        hpPerNormal  = hpConfig.hpPerNormal;
        hpPerMiss    = hpConfig.hpPerMiss;

        Debug.Log($"[HP Config] Loaded '{hpConfig.name}' — " +
                  $"Perfect:{hpPerPerfect} Good:{hpPerGood} " +
                  $"Normal:{hpPerNormal} Miss:{hpPerMiss}");
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
                StartCoroutine(StartSong());
            }
        }
        else
        {
            if (!theMusic.isPlaying && !resultsScreen.activeInHierarchy && !isPaused)
            {
                resultsScreen.SetActive(true);

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

        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true);
            if (gameOverScoreText != null)
                gameOverScoreText.text = currentScore.ToString();
        }
    }

    public void Pause()
    {
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
        pauseScreen.SetActive(false);
        countdownText.gameObject.SetActive(true);

        countdownText.text = "3"; yield return new WaitForSecondsRealtime(1f);
        countdownText.text = "2"; yield return new WaitForSecondsRealtime(1f);
        countdownText.text = "1"; yield return new WaitForSecondsRealtime(1f);

        countdownText.gameObject.SetActive(false);
        isPaused = false;
        theMusic.UnPause();
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
}
