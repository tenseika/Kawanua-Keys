using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class NoteSpawner : MonoBehaviour
{
    public GameObject[] notePrefabs;  // Assign 4 prefabs in Inspector, one per lane

    public Transform[] lanes;
    public KeyCode[] laneKeys;

    public Transform[] hitZone; // drag your Activator object here in Inspector

    public float scrollSpeed = 3f;

    public TextAsset chartFile;

    void Start()
    {
        List<float> timestamps = ParseChart(chartFile.text);

        int lastLane = -1;

        foreach (float beatTime in timestamps)
        {
            int randomLane;
            do
            {
                randomLane = Random.Range(0, lanes.Length);
            } while (randomLane == lastLane);

            lastLane = randomLane;

            float startY = beatTime * scrollSpeed;

            Vector3 spawnPos = new Vector3(lanes[randomLane].position.x, startY, 0f);

            // Use the prefab that matches the lane index
            GameObject newNote = Instantiate(notePrefabs[randomLane], spawnPos, Quaternion.identity);

            NoteObject note = newNote.GetComponent<NoteObject>();
            if (note != null)
            {
                note.beatTime = beatTime;
                note.scrollSpeed = scrollSpeed;
                note.keyToPress = laneKeys[randomLane];
                note.hitZone = hitZone[randomLane]; // assign hit zone to each spawned note
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
        // Format: M.SS.mmm (e.g. 1.00.024 = 1 min, 0 sec, 024 ms)
        string[] parts = value.Split('.');

        if (parts.Length == 3)
        {
            int minutes = int.Parse(parts[0]);
            int seconds = int.Parse(parts[1]);
            float milliseconds = float.Parse(parts[2]) / 1000f;

            return (minutes * 60f) + seconds + milliseconds;
        }
        else
        {
            // Plain seconds format (e.g. 4.989)
            return float.Parse(value);
        }
    }
}