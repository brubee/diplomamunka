using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI levelName;
    [SerializeField] private TextMeshProUGUI clearTimeText;
    [SerializeField] private TMP_FontAsset fontAsset;
    private LevelData LevelData { get; set; }
    private Button _button;
    private Image _image;
    public Color ReturnColor { get; set; }

    private void Awake()
    {
        _button = GetComponent<Button>();
        _image = GetComponent<Image>();
        ReturnColor = Color.gray;
    }

    public void Setup(LevelData level, bool isUnlocked)
    {
        LevelData = level;
        levelName.SetText(level.levelID);
        levelName.font = fontAsset;

        _button.onClick.RemoveListener(LoadLevel);
        _button.interactable = isUnlocked;

        if (isUnlocked)
        {
            _button.onClick.AddListener(LoadLevel);
            ReturnColor = Color.white;
        }
        else
        {
            ReturnColor = Color.gray;
        }

        _image.color = ReturnColor;
        RefreshClearTime();
    }

    public void RefreshClearTime()
    {
        if (clearTimeText == null || LevelData == null)
            return;

        GameProgressManager manager = GameProgressManager.Instance;

        GameProgressManager.LevelResult result = manager != null ? manager.GetLevelResult(LevelData.levelID) : null;

        if (result == null || !result.completed)
        {
            clearTimeText.text = "";
            return;
        }

        int totalSeconds = Mathf.Max(0, Mathf.RoundToInt(result.completionTime));

        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        clearTimeText.text = $"{minutes}:{seconds:00}";
    }

    public void UnlockLevel()
    {
        _button.interactable = true;
        _button.onClick.RemoveListener(LoadLevel);
        ReturnColor = Color.white;
        _image.color = ReturnColor;
    }

    public void LoadLevel()
    {
        if (LevelData == null)
            return;

        string[] parts = LevelData.levelID.Split('-');

        if (parts.Length == 2 && int.TryParse(parts[0], out int areaNumber) && areaNumber >= 1)
        {
            WorldMapNavigation.selectedAreaIndex = areaNumber - 1;
        }
        else
        {
            return;
        }

        SceneManager.LoadScene(LevelData.scene);
    }
}
