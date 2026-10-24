using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AreaQuizButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private GameObject lockedIndicator;

    [SerializeField] private Color lockedColor = Color.gray;
    [SerializeField] private Color unlockedColor = Color.white;

    private Button _button;
    private Image _image;
    private int _areaNumber;
    private LevelSelectManager _manager;

    public Color returnColor { get; private set; } = Color.gray;
    public int areaNumber => _areaNumber;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _image = GetComponent<Image>();
    }

    public void Setup(int areaNumber, bool isUnlocked, LevelSelectManager manager)
    {
        _areaNumber = areaNumber;
        _manager = manager;

        if (buttonText != null)
            buttonText.text = "A NAGY KVÍZ";

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(OnClicked);

        SetUnlocked(isUnlocked);
    }

    public void SetUnlocked(bool isUnlocked)
    {
        _button.interactable = isUnlocked;

        if(lockedIndicator != null)
            lockedIndicator.SetActive(!isUnlocked);

        if (_image != null)
            _image.color = isUnlocked ? unlockedColor : lockedColor;
    }

    private void OnClicked()
    {
        if (GameProgressManager.Instance == null || !GameProgressManager.Instance.IsAreaQuizUnlocked(_areaNumber))
            return;

        _manager.EnableAreaQuiz(_areaNumber);
    }
}
