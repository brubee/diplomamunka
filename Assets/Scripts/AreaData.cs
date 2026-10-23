using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Level Data/Areas", fileName ="New Area")]

public class AreaData : ScriptableObject
{
    public string areaName;
    public AudioClip backgroundMusic;
    public Sprite backgroundSprite;
    public List<LevelData> levels = new List<LevelData>();
}
