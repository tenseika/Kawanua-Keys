using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonController : MonoBehaviour
{
    private SpriteRenderer theSR;
    public Sprite defaultImage;
    public Sprite pressedImage;

    // Nomor lane yang diwakili tombol ini (0 = paling kiri)
    // Harus sama dengan index di LaneInputManager.laneAreas
    public int laneIndex;

    void Start()
    {
        theSR = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (LaneInputManager.instance == null) return;

        if (LaneInputManager.instance.GetLaneDown(laneIndex))
            theSR.sprite = pressedImage;

        if (LaneInputManager.instance.GetLaneUp(laneIndex))
            theSR.sprite = defaultImage;
    }
}
