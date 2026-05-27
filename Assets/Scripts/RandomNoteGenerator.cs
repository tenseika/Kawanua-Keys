using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomNoteGenerator : MonoBehaviour
{
    public GameObject notePrefab;

    public Transform beatScroller;

    public Transform[] lanes;

    public KeyCode[] laneKeys; // NEW

    public int numberOfNotes = 100;

    public float startY = 10f;
    public float spacing = 1f;

    void Start()
    {
        GenerateNotes();
    }

    void GenerateNotes()
    {
        int lastLane = -1; // remembers previous lane

        for (int i = 0; i < numberOfNotes; i++)
        {
            int randomLane;

            // keep picking until it's NOT the same as last lane
            do
            {
                randomLane = Random.Range(0, lanes.Length);
            }
            while (randomLane == lastLane);

            lastLane = randomLane;

            Vector3 spawnPos = new Vector3(
                lanes[randomLane].position.x,
                startY + (i * spacing),
                0f
            );

            GameObject newNote = Instantiate(notePrefab, spawnPos, Quaternion.identity);
            newNote.transform.SetParent(beatScroller);

            NoteObject note = newNote.GetComponent<NoteObject>();
            if (note != null)
            {
                note.keyToPress = laneKeys[randomLane];
            }
        }
    }
}