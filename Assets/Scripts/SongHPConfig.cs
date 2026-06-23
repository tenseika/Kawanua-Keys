using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SongHPConfig", menuName = "Rhythm Game/Song HP Config")]
public class SongHPConfig : ScriptableObject
{
    [Header("HP Gain")]
    public float hpPerPerfect;
    public float hpPerGood;

    [Header("HP Damage")]
    public float hpPerNormal;   // hit biasa (terlambat/terlalu cepat)
    public float hpPerMiss;
}
