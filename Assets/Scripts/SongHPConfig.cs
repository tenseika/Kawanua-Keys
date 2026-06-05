using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SongHPConfig", menuName = "Rhythm Game/Song HP Config")]
public class SongHPConfig : ScriptableObject
{
    [Header("HP Gain")]
    public float hpPerPerfect = 3f;
    public float hpPerGood    = 1f;

    [Header("HP Damage")]
    public float hpPerNormal  = -3f;   // hit biasa (terlambat/terlalu cepat)
    public float hpPerMiss    = -10f;
}
