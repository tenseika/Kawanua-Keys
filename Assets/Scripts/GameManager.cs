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
    public float maxHP = 100f;
    public float currentHP = 70f;

    public float hpPerPerfect = 3f;
    public float hpPerGood = 1f;
    public float hpPerNormal = -3f;
    public float hpPerMiss = -10f;

    public TMP_Text hpText;              // drag HP TMP Text UI here
    public Image heartImage;
    public GameObject gameOverScreen;   // drag Game Over UI panel here
    public TMP_Text gameOverScoreText;   // drag a TMP text for score on game over screen

    // Warna HP — atur bebas di Inspector
    public Color hpColorHigh   = new Color(0.27f, 0.85f, 0.40f);  // hijau  (> 60%)
    public Color hpColorMedium = new Color(1.00f, 0.80f, 0.10f);  // kuning (30–60%)
    public Color hpColorLow    = new Color(0.90f, 0.25f, 0.25f);  // merah  (< 30%)

    [HideInInspector] public bool isGameOver = false;

    void Start()
    {
        instance = this;

        scoreText.text = "Skor: 0";
        currentMultiplier = 1;

        // totalNotes = FindObjectsOfType<NoteObject>().Length; pindah ke function StartSong

        // Inisialisasi HP text
        UpdateHPDisplay();
    }

    void Update()
    {
        if (isGameOver) return;

        if (!startPlaying)
        {
            if (Input.anyKeyDown)
            {
                startPlaying = true;
                StartCoroutine(StartSong());
            }
        }
        else
        {
            if (!theMusic.isPlaying && !resultsScreen.activeInHierarchy && !isPaused)
            {
                resultsScreen.SetActive(true);

                // Weighted accuracy system:
                // Perfect = 100pts, Good = 75pts, Normal = 50pts, Miss = -25pts
                float weightedScore = (perfectHits * 100f) + (goodHits * 75f) + (normalHits * 50f) + (missedHits * -25f);
                float maxPossibleScore = totalNotes * 100f;
                float percentHit = Mathf.Clamp((weightedScore / maxPossibleScore) * 100f, 0f, 100f);

                percentHitText.text = percentHit.ToString("F1") + "%";

                string rankVal = "F";

                if (percentHit >= 40f)
                {
                    rankVal = "D";
                    if (percentHit >= 55f)
                    {
                        rankVal = "C";
                        if (percentHit >= 70f)
                        {
                            rankVal = "B";
                            if (percentHit >= 85f)
                            {
                                rankVal = "A";
                                if (percentHit >= 95f)
                                {
                                    rankVal = "S";
                                }
                            }
                        }
                    }
                }

                rankText.text = rankVal;
                finalScoreText.text = currentScore.ToString();
            }
        }
    }

    IEnumerator StartSong()
    {
        double startTime = AudioSettings.dspTime + 1.0;
        theMusic.PlayScheduled(startTime);

        yield return new WaitForSeconds(1f);
        // Notes now position themselves via audio time — nothing else needed here
        totalNotes = FindObjectsOfType<NoteObject>().Length;
    }

    public void NoteHit()
    {
        Debug.Log("Hit On Time");

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
        Debug.Log("Missed Note");

        currentMultiplier = 1;
        multiplierTracker = 0;

        multiText.text = "x" + currentMultiplier;
        missedHits++;

        ModifyHP(hpPerMiss);
    }

    // -------------------------------------------------------------------------
    // Health
    // -------------------------------------------------------------------------
    void ModifyHP(float amount)
    {
        if (isGameOver) return;

        currentHP = Mathf.Clamp(currentHP + amount, 0f, maxHP);

        UpdateHPDisplay();

        if (currentHP <= 0f)
            TriggerGameOver();
    }

    void UpdateHPDisplay(bool isGain = false)
    {
        if (hpText == null) return;

        float ratio = currentHP / maxHP;

        hpText.text = Mathf.CeilToInt(currentHP).ToString();

        Color hpColor;

        if (ratio > 0.6f)
            hpColor = hpColorHigh;
        else if (ratio > 0.3f)
            hpColor = hpColorMedium;
        else
            hpColor = hpColorLow;

        hpText.color = hpColor;

        if (heartImage != null)
            heartImage.color = hpColor;   // ← sync warna ke sprite hati
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

        countdownText.text = "3";
        yield return new WaitForSecondsRealtime(1f);

        countdownText.text = "2";
        yield return new WaitForSecondsRealtime(1f);

        countdownText.text = "1";
        yield return new WaitForSecondsRealtime(1f);

        countdownText.gameObject.SetActive(false);
        isPaused = false;
        theMusic.UnPause();
    }
}