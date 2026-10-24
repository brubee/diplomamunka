using System;
using System.Collections.Generic;
using UnityEngine;

public class GameProgressManager : MonoBehaviour
{
    public static GameProgressManager Instance { get; private set; }

    private const string SaveKey = "GameProgress";

    public const float AreaTimeTargetSeconds = 120f;
    public const float AreaQuizPassingPercent = 80f;

    [Serializable]
    public class LevelResult
    {
        public string levelID;
        public float completionTime;
        public int correctAnswers;
        public int totalQuestions;
        public int requiredCorrectAnswers;
        public bool completed;
        public float accuracyPercent => totalQuestions > 0 ? 100f * correctAnswers / totalQuestions : 0f;
    }

    [Serializable]
    public class AreaQuizResult
    {
        public int areaNumber;
        public int correctAnswers;
        public int totalQuestions;
        public float passingAccuracyPercent;
        public bool passed;
        public float accuracyPercent => totalQuestions > 0 ? 100f * correctAnswers / totalQuestions : 0f;
    }

    [Serializable]
    private class SaveData
    {
        public List<string> unlockedLevelIDs = new List<string>();
        public List<LevelResult> levelResults = new List<LevelResult>();
        public List<int> unlockedAreaQuizzes = new List<int>();
        public List<int> passedAreaQuizzes = new List<int>();
        public List<AreaQuizResult> areaQuizResults = new List<AreaQuizResult>();
    }

    private SaveData saveData = new SaveData();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadProgress();
    }

    public bool IsAreaQuizUnlocked(int areaNumber)
    {
        return saveData.unlockedAreaQuizzes.Contains(areaNumber);
    }

    public void UnlockAreaQuiz(int areaNumber)
    {
        if (areaNumber < 1 || areaNumber > 5)
            return;

        if (!saveData.unlockedAreaQuizzes.Contains(areaNumber))
        {
            saveData.unlockedAreaQuizzes.Add(areaNumber);
            SaveProgress();
        }
    }

    public bool IsLevelUnlocked(LevelData level)
    {
        if (level == null)
            return false;

        return level.isUnlockedByDefault || saveData.unlockedLevelIDs.Contains(level.levelID);
    }

    public void UnlockLevel(string levelID)
    {
        if (string.IsNullOrEmpty(levelID))
            return;

        if (!saveData.unlockedLevelIDs.Contains(levelID))
        {
            saveData.unlockedLevelIDs.Add(levelID);
            SaveProgress();
        }
    }

    public bool IsLevelCompleted(string levelID)
    {
        LevelResult result = GetLevelResult(levelID);
        return result != null && result.completed;
    }

    public void RecordLevelResult(string levelID, float completionTime, int correctAnswers, int totalQuestions, float targetAccuracyPercent)
    {
        if (string.IsNullOrEmpty(levelID))
            return;

        LevelResult result = GetLevelResult(levelID);

        if (result == null)
        {
            result = new LevelResult();
            result.levelID = levelID;
            saveData.levelResults.Add(result);
        }

        result.completionTime = completionTime;
        result.correctAnswers = correctAnswers;
        result.totalQuestions = totalQuestions;
        result.requiredCorrectAnswers = Mathf.CeilToInt(totalQuestions * targetAccuracyPercent / 100f);
        result.completed = true;

        UnlockNextLevel(levelID);
        CheckAreaQuizUnlocked(levelID);
        SaveProgress();
    }

    public LevelResult GetLevelResult(string levelID)
    {
        return saveData.levelResults.Find(result =>  result.levelID == levelID);
    }

    private void UnlockNextLevel(string levelID)
    {
        string[] parts = levelID.Split('-');

        if (parts.Length != 2)
            return;

        if (!int.TryParse(parts[0], out int areaNumber))
            return;

        if (!int.TryParse(parts[1], out int levelNumber))
            return;

        if (levelNumber >= 1 && levelNumber < 5)
        {
            UnlockLevel($"{areaNumber}-{levelNumber + 1}");
        }
    }

    private void CheckAreaQuizUnlocked(string levelID)
    {
        string[] parts = levelID.Split('-');

        if (parts.Length != 2)
            return;

        if (!int.TryParse(parts[0], out int areaNumber))
            return;

        if (!int.TryParse(parts[1], out int levelNumber))
            return;

        if (levelNumber == 5)
            UnlockAreaQuiz(areaNumber);
    }

    public float GetAreaAverageTime(int areaNumber)
    {
        float totalTime = 0f;

        for (int level = 1; level <= 5; level++)
        {
            LevelResult levelResult = GetLevelResult($"{areaNumber}-{level}");
            if (levelResult == null || !levelResult.completed)
                return -1f;

            totalTime += levelResult.completionTime;
        }

        return totalTime / 5f;
    }

    public bool MetAreaSpeedRequirement(int areaNumber)
    {
        float average = GetAreaAverageTime(areaNumber);
        return average >= 0f && average <= AreaTimeTargetSeconds;
    }

    public AreaQuizResult GetAreaQuizResult(int areaNumber)
    {
        return saveData.areaQuizResults.Find(result => result.areaNumber == areaNumber);
    }

    public void RecordAreaQuizResult(int areaNumber, int correctAnswers, int totalQuestions, float passingAccuracyPercent)
    {
        if (areaNumber < 1 || areaNumber > 5 || totalQuestions <= 0)
            return;

        AreaQuizResult result = GetAreaQuizResult(areaNumber);

        if (result == null)
        {
            result = new AreaQuizResult();
            result.areaNumber = areaNumber;
            saveData.areaQuizResults.Add(result);
        }
        result.correctAnswers = correctAnswers;
        result.totalQuestions = totalQuestions;
        result.passingAccuracyPercent = passingAccuracyPercent;
        result.passed = 100f * correctAnswers / totalQuestions >= passingAccuracyPercent;

        if (result.passed)
        {
            if (!saveData.passedAreaQuizzes.Contains(areaNumber))
                saveData.passedAreaQuizzes.Add(areaNumber);
        }
        else
        {
            saveData.passedAreaQuizzes.Remove(areaNumber);
        }

        SaveProgress();
    }

    public bool HasPassedAreaQuiz(int areaNumber)
    {
        AreaQuizResult result = GetAreaQuizResult(areaNumber);

        if (result != null)
            return result.passed;

        return saveData.passedAreaQuizzes.Contains(areaNumber);
    }

    public bool CanAccessNextArea(int areaNumber)
    {
        return HasPassedAreaQuiz(areaNumber) && MetAreaSpeedRequirement(areaNumber);
    }

    public bool TryUnlockingNextArea(int areaNumber)
    {
        if (!CanAccessNextArea(areaNumber))
            return false;

        if (areaNumber >= 5)
            return false;

        UnlockLevel($"{areaNumber + 1}-1");
        return true;
    }

    public void RecordAreaQuizPassed(int areaNumber)
    {
        if (!saveData.passedAreaQuizzes.Contains(areaNumber))
        {
            saveData.passedAreaQuizzes.Add(areaNumber);
            SaveProgress();
        }
    }

    private void SaveProgress()
    {
        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    private void LoadProgress()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            saveData = new SaveData();
            return;
        }

        string json = PlayerPrefs.GetString(SaveKey);

        try
        {
            SaveData loadedData = JsonUtility.FromJson<SaveData>(json);

            saveData = loadedData ?? new SaveData();

            if (saveData.unlockedLevelIDs == null)
                saveData.unlockedLevelIDs = new List<string>();

            if (saveData.levelResults == null)
                saveData.levelResults = new List<LevelResult>();

            if (saveData.unlockedAreaQuizzes == null)
                saveData.unlockedAreaQuizzes = new List<int>();

            if (saveData.passedAreaQuizzes == null)
                saveData.passedAreaQuizzes = new List<int>();

            if (saveData.areaQuizResults == null)
                saveData.areaQuizResults = new List<AreaQuizResult>();
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to load game progress: " + e.Message);

            saveData = new SaveData();
        }
    }

    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();

        saveData = new SaveData();

        Debug.Log("Game progress reset!");
    }
}
