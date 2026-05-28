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
    public GameObject pauseScreen; // drag your pause UI panel here in Inspector
    public TMP_Text countdownText; // drag a TMP text object here in Inspector

    public GameObject resultsScreen;
    public TMP_Text percentHitText, rankText, finalScoreText;

    void Start()
    {
        instance = this;

        scoreText.text = "Skor: 0";
        currentMultiplier = 1;

        // totalNotes = FindObjectsOfType<NoteObject>().Length; pindah ke function StartSong
    }

    void Update()
    {
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
    }

    public void GoodHit()
    {
        currentScore += scorePerGoodNote * currentMultiplier;
        NoteHit();
        goodHits++;
    }

    public void PerfectHit()
    {
        currentScore += scorePerPerfectNote * currentMultiplier;
        NoteHit();
        perfectHits++;
    }

    public void NoteMissed()
    {
        Debug.Log("Missed Note");

        currentMultiplier = 1;
        multiplierTracker = 0;

        multiText.text = "x" + currentMultiplier;
        missedHits++;
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