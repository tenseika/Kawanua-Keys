using UnityEngine;
using TMPro;

public class RecordManager : MonoBehaviour
{
    [System.Serializable]
    public class SongRecord
    {
        public string songID;
        public TMP_Text accuracyText;
        public TMP_Text rankText;
        public TMP_Text scoreText;
    }

    public SongRecord[] songs;
    [Header("Average Display")]
    public TMP_Text avgAccuracyText;
    public TMP_Text avgRankText;

    void OnEnable()
    {
        RefreshUI();
    }

    void RefreshUI()
    {
        float totalAccuracy  = 0f;
        int   validSongCount = 0;
        float totalRankScore = 0f;

        string[] rankOrder = { "F", "D", "C", "B", "A", "S" };

        foreach (SongRecord song in songs)
        {
            float  acc   = PlayerPrefs.GetFloat(song.songID  + "_accuracy", -1f);
            string rank  = PlayerPrefs.GetString(song.songID + "_rank",     "");
            int    score = PlayerPrefs.GetInt(song.songID    + "_score",    0);

            song.accuracyText.text = acc   >= 0f    ? acc.ToString("F1") + "%" : "-";
            song.rankText.text     = rank  != ""    ? rank                     : "-";
            song.scoreText.text    = score > 0      ? score.ToString()         : "-";

            // Akumulasi untuk rata-rata (hanya lagu yang sudah pernah dimainkan)
            if (acc >= 0f)
            {
                totalAccuracy += acc;
                totalRankScore += System.Array.IndexOf(rankOrder, rank);
                validSongCount++;
            }
        }

        // Tampilkan rata-rata
        if (validSongCount > 0)
        {
            float avgAcc = totalAccuracy / validSongCount;
            avgAccuracyText.text = avgAcc.ToString("F1") + "%";

            int avgRankIndex = Mathf.RoundToInt(totalRankScore / validSongCount);
            avgRankIndex = Mathf.Clamp(avgRankIndex, 0, rankOrder.Length - 1);
            avgRankText.text = rankOrder[avgRankIndex];
        }
        else
        {
            avgAccuracyText.text = "-";
            avgRankText.text     = "-";
        }
    }
}