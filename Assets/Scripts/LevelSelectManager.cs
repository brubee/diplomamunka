using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class LevelSelectManager : MonoBehaviour
{
    public Transform levelParent;
    public GameObject levelButtonPrefab;
    public LineRenderer lineConnectorPrefab;
    public TextMeshProUGUI areaHeader;
    public TextMeshProUGUI levelHeader;

    public AreaData currentArea;
    [SerializeField] private AreaData[] allAreas;
    private int currentAreaIndex;

    [SerializeField] private AreaQuizButton areaQuizButton;
    [SerializeField] private Button previousAreaButton;
    [SerializeField] private Button nextAreaButton;
    [SerializeField] private GameObject areaQuizPanel;
    [SerializeField] private GameObject quizResultPanel;
    [SerializeField] private AreaQuizManager areaQuizManager;
    [SerializeField] private CanvasGroup levelSelectCG;
    [SerializeField] private Image areaBackground;

    public GameObject playerPrefab;
    public RectTransform worldSpaceCanvas;
    public Vector2 playerPosOffset = new Vector2(0f, 0.2f);

    public HashSet<string> unlockedLevelIDs = new HashSet<string>();

    private LevelSelectEventSystemHandler _eventSystemHandler;

    private Camera _camera;

    private List<GameObject> _buttonObjects = new List<GameObject>();
    private Dictionary<GameObject, Vector3> _buttonLocations = new Dictionary<GameObject, Vector3>();

    public GameObject playerObj { get; set; }
    private bool _playerIsFacingRight;

    private void Awake()
    {
        _camera = Camera.main;
        _eventSystemHandler = GetComponentInChildren<LevelSelectEventSystemHandler>(true);
    }

    private void Start()
    {
        if (allAreas != null && allAreas.Length > 0)
        {
            currentAreaIndex = Mathf.Clamp(WorldMapNavigation.selectedAreaIndex, 0, allAreas.Length - 1);
            currentArea = allAreas[currentAreaIndex];
            UpdateAreaBackground();
        }

        AssignAreaText();
        LoadUnlockedLevels();
        CreateLevelButtons();
        SetupAreaQuizButton();
        SetupAreaNavigation();

        if (SoundManager.instance != null)
            SoundManager.instance.PlayAreaMusic(currentArea);
    }

    public void AssignAreaText()
    {
        areaHeader.SetText(currentArea.areaName);
    }

    private void LoadUnlockedLevels()
    {
        unlockedLevelIDs.Clear();

        foreach (var level in currentArea.levels)
        {
            if (GameProgressManager.Instance != null && GameProgressManager.Instance.IsLevelUnlocked(level))
            {
                unlockedLevelIDs.Add(level.levelID);
            }
            else if (level.isUnlockedByDefault)
            {
                unlockedLevelIDs.Add(level.levelID);
            }
        }
    }

    private void CreateLevelButtons()
    {
        for (int i = 0; i < currentArea.levels.Count; i++)
        {
            GameObject buttonActual = Instantiate(levelButtonPrefab, levelParent);
            _buttonObjects.Add(buttonActual);

            RectTransform buttonRect = buttonActual.GetComponent<RectTransform>();

            buttonActual.name = currentArea.levels[i].levelID;
            currentArea.levels[i].levelButton = buttonActual;

            LevelButton levelButton = buttonActual.GetComponent<LevelButton>();
            levelButton.Setup(currentArea.levels[i], unlockedLevelIDs.Contains(currentArea.levels[i].levelID));

            Selectable selectable = buttonActual.GetComponent<Selectable>();
            _eventSystemHandler.AddSelectable(selectable);

            StartCoroutine(AddLocationAfterDelay(buttonActual, buttonRect));

            if (i > 0)
            {
                LineRenderer line = Instantiate(lineConnectorPrefab, levelParent);

                line.transform.SetSiblingIndex(0);
                LineConnectorRenderer lineConnector = line.GetComponent<LineConnectorRenderer>();
                lineConnector.StartRectTrans = currentArea.levels[i - 1].levelButton.GetComponent<RectTransform>();
                lineConnector.EndRectTrans = buttonActual.GetComponent<RectTransform>();

                StartCoroutine(DelayedLineSetup(lineConnector));
            }
            else
            {
                StartCoroutine(SpawnInPlayerAfterDelay(buttonRect, worldSpaceCanvas));
            }
        }

        levelParent.gameObject.SetActive(true);
        _eventSystemHandler.InitSelectables();
        _eventSystemHandler.SetFirstSelected();
    }

    private IEnumerator DelayedLineSetup(LineConnectorRenderer lineConnector)
    {
        yield return null;
        lineConnector.UpdateLinePosition();
    }

    private IEnumerator AddLocationAfterDelay(GameObject buttonGO, RectTransform buttonRect)
    {
        yield return null;
        Vector2 buttonScreenPoint = RectTransformUtility.WorldToScreenPoint(_camera, buttonRect.position);
        Vector3 buttonWorldPos = _camera.ScreenToWorldPoint(new Vector3(buttonScreenPoint.x, buttonScreenPoint.y, _camera.nearClipPlane));
        _buttonLocations.Add(buttonGO, buttonWorldPos);
    }

    private IEnumerator SpawnInPlayerAfterDelay(RectTransform screenSpaceButton, RectTransform worldSpaceCanvas)
    {
        yield return null;
        SpawnInPlayer(screenSpaceButton, worldSpaceCanvas);
    }

    private void SpawnInPlayer(RectTransform screenSpaceUIObject, RectTransform worldSpaceUIObject)
    {
        _playerIsFacingRight = true;
        playerObj = Instantiate(playerPrefab, worldSpaceUIObject);

        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(_camera, screenSpaceUIObject.position);
        Vector3 worldPos = _camera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, _camera.nearClipPlane));
        worldPos.z = worldSpaceUIObject.position.z;

        Vector3 offsetPos = worldPos + (Vector3)playerPosOffset;

        playerObj.transform.position = offsetPos;

        if (_buttonObjects.Count > 1)
        {
            Vector2 secondScreenPoint = RectTransformUtility.WorldToScreenPoint(_camera, _buttonObjects[1].GetComponent<RectTransform>().position);
            Vector3 secondWorldPoint = _camera.ScreenToWorldPoint(new Vector3(secondScreenPoint.x, secondScreenPoint.y, _camera.nearClipPlane));
            secondWorldPoint.z = worldSpaceUIObject.position.z;

            CheckForRightOrLeftTurn(playerObj, ref _playerIsFacingRight, secondWorldPoint);
        }
    }

    private void CheckForRightOrLeftTurn(GameObject player, ref bool isFacingRight, Vector3 targetWorldPos)
    {
        if (isFacingRight)
        {
            if (targetWorldPos.x < player.transform.position.x)
            {
                player.transform.Rotate(0f, 180f, 0f);
                isFacingRight = false;
            }
        }
        else
        {
            if (targetWorldPos.x > player.transform.position.x)
            {
                player.transform.Rotate(0f, -180f, 0f);
                isFacingRight = true;
            }
        }
    }

    public void MovePlayerToButton(GameObject player, RectTransform targetButton, RectTransform worldSpaceUIObject)
    {
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(_camera, targetButton.position);
        Vector3 worldPosition = _camera.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, _camera.nearClipPlane));
        worldPosition.z = worldSpaceUIObject.position.z;

        Vector3 endPos = worldPosition + (Vector3)playerPosOffset;

        CheckForRightOrLeftTurn(player, ref _playerIsFacingRight, worldPosition);

        player.transform.DOMove(endPos, 0.11f);
    }

    public void EnableAreaQuiz(int areaNumber)
    {
        if (GameProgressManager.Instance == null || !GameProgressManager.Instance.IsAreaQuizUnlocked(areaNumber))
            return;

        if (areaQuizPanel == null)
            return;

        SetWorldMapNavigation(false);
        SetPlayerIconVisible(false);

        if (quizResultPanel != null)
            quizResultPanel.SetActive(false);

        areaQuizPanel.SetActive(true);
        areaQuizManager.StartAreaQuiz(areaNumber, currentArea);
    }

    private void SetupAreaQuizButton()
    {
        if (areaQuizButton == null)
            return;

        if (areaQuizPanel != null)
            areaQuizPanel.SetActive(false);

        int areaNumber = GetCurrentAreaNumber();

        bool isUnlocked = GameProgressManager.Instance != null && GameProgressManager.Instance.IsAreaQuizUnlocked(areaNumber);

        areaQuizButton.Setup(areaNumber, isUnlocked, this);
    }

    private int GetCurrentAreaNumber()
    {
        if (currentArea == null || currentArea.levels == null)
            return -1;

        foreach (LevelData level in currentArea.levels)
        {
            if (level == null)
                continue;

            string[] parts = level.levelID.Split('-');

            if (parts.Length == 2 && int.TryParse(parts[0], out int areaNumber))
                return areaNumber;
        }

        return -1;
    }

    private void UpdateAreaBackground()
    {
        if (areaBackground == null || currentArea == null)
            return;

        areaBackground.sprite = currentArea.backgroundSprite;
    }

    public void SetWorldMapNavigation(bool enabled)
    {
        if (_eventSystemHandler != null)
            _eventSystemHandler.enabled = enabled;

        if (levelSelectCG != null)
        {
            levelSelectCG.interactable = enabled;
            levelSelectCG.blocksRaycasts = enabled;
        }

        if (!enabled && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        if (enabled && _eventSystemHandler != null)
            _eventSystemHandler.SetFirstSelected();
    }

    private void SetupAreaNavigation()
    {
        previousAreaButton.onClick.RemoveAllListeners();
        nextAreaButton.onClick.RemoveAllListeners();

        previousAreaButton.onClick.AddListener(() => SwitchArea(currentAreaIndex - 1));
        nextAreaButton.onClick.AddListener(() => SwitchArea(currentAreaIndex + 1));

        UpdateAreaNavigationButtons();
    }

    private void UpdateAreaNavigationButtons()
    {
        previousAreaButton.interactable = currentAreaIndex > 0;

        bool nextAreaUnlocked = currentAreaIndex < allAreas.Length - 1 && GameProgressManager.Instance != null && GameProgressManager.Instance.IsLevelUnlocked(allAreas[currentAreaIndex + 1].levels[0]);

        nextAreaButton.interactable = nextAreaUnlocked;
    }

    public void CloseAreaQuiz()
    {
        if (areaQuizManager != null && areaQuizManager.TryShowFinalPanel())
            return;

        int areaNumber = GetCurrentAreaNumber();

        if (GameProgressManager.Instance != null && areaNumber > 0)
        {
            GameProgressManager.Instance.TryUnlockingNextArea(areaNumber);

            LoadUnlockedLevels();
            UpdateAreaNavigationButtons();
            SetupAreaQuizButton();
        }

        if (areaQuizPanel != null)
            areaQuizPanel.SetActive(false);

        if (quizResultPanel != null)
            quizResultPanel.SetActive(false);

        SetPlayerIconVisible(true);
        SetWorldMapNavigation(true);
    }

    private void SetPlayerIconVisible(bool visible)
    {
        if (playerObj != null)
            playerObj.SetActive(visible);
    }

    public void SwitchArea(int areaNumber)
    {
        if (allAreas == null || areaNumber < 0 || areaNumber >= allAreas.Length || allAreas[areaNumber] == null)
            return;

        string firstLevelID = allAreas[areaNumber].levels[0].levelID;

        if (GameProgressManager.Instance == null || !GameProgressManager.Instance.IsLevelUnlocked(allAreas[areaNumber].levels[0]))
            return;

        if (playerObj != null)
        {
            Destroy(playerObj);
            playerObj = null;
        }

        foreach (Transform child in levelParent)
            Destroy(child.gameObject);

        _buttonObjects.Clear();
        _buttonLocations.Clear();
        unlockedLevelIDs.Clear();

        _eventSystemHandler.Selectables.Clear();

        currentAreaIndex = areaNumber;
        WorldMapNavigation.selectedAreaIndex = currentAreaIndex;
        currentArea = allAreas[areaNumber];
        UpdateAreaBackground();

        if (SoundManager.instance != null)
            SoundManager.instance.PlayAreaMusic(currentArea);

        AssignAreaText();
        LoadUnlockedLevels();
        CreateLevelButtons();
        SetupAreaQuizButton();
        UpdateAreaNavigationButtons();
    }

    public void ReturnToStartingMenu()
    {
        SceneManager.LoadScene("StartingMenu");
    }
}
