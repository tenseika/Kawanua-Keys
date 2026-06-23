using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Globalization;

public class NoteSpawner : MonoBehaviour
{
    public GameObject[] notePrefabs;
    public Transform[] lanes;
    [HideInInspector] public KeyCode[] laneKeys;
    public Transform[] hitZone;
    public float scrollSpeed = 3f;
    public TextAsset chartFile;

    void Start()
    {
        List<float> timestamps = ParseChart(chartFile.text);

        int prevLane1 = -1;
        int prevLane2 = -1;

        foreach (float beatTime in timestamps)
        {
            int randomLane;
            do
            {
                randomLane = Random.Range(0, lanes.Length);
            } while (randomLane == prevLane1 || randomLane == prevLane2);

            prevLane2 = prevLane1;
            prevLane1 = randomLane;

            float startY = beatTime * scrollSpeed;
            Vector3 spawnPos = new Vector3(lanes[randomLane].position.x, startY, 0f);
            Debug.Log($"[Spawn] BeatTime:{beatTime} StartY:{startY} ScrollSpeed:{scrollSpeed}");

            GameObject newNote = Instantiate(notePrefabs[randomLane], spawnPos, Quaternion.identity);

            NoteObject note = newNote.GetComponent<NoteObject>();
            if (note != null)
            {
                note.beatTime    = beatTime;
                note.scrollSpeed = scrollSpeed;
                note.keyToPress  = laneKeys[randomLane];
                note.hitZone     = hitZone[randomLane];
                note.laneIndex   = randomLane;
            }
        }
    }

    List<float> ParseChart(string text)
    {
        List<float> timestamps = new List<float>();
        string[] lines = text.Split('\n');

        foreach (string line in lines)
        {
            string trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;
            timestamps.Add(ParseTimestamp(trimmed));
        }

        return timestamps;
    }

    float ParseTimestamp(string value)
    {
        string[] parts = value.Split('.');

        if (parts.Length == 3)
        {
            int minutes = int.Parse(parts[0]);
            int seconds = int.Parse(parts[1]);
            float milliseconds = float.Parse(parts[2], CultureInfo.InvariantCulture) / 1000f;
            return (minutes * 60f) + seconds + milliseconds;
        }
        else
        {
            return float.Parse(value, CultureInfo.InvariantCulture);
        }
    }
}
