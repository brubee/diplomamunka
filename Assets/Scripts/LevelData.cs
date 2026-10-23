using System;
using UnityEngine;

[CreateAssetMenu(menuName ="Level Data/Levels", fileName ="New level")]

public class LevelData : ScriptableObject
{
    [Header("Level stats")]
    public string levelID;
    [Tooltip("For Starting Levels")] public bool isUnlockedByDefault;
    public SceneField scene;

    [Header("Level Display Information")]
    public string levelName;

    public GameObject levelButton { get; set; }

    [Header("Quiz")]
    public WordData[] words;
}

[Serializable]
public class WordData
{
    public string prefix;
    public string suffix;
    public string correctAnswer;
}
