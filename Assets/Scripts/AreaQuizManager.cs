using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class AreaQuizManager : MonoBehaviour
{
    [SerializeField] private GameObject quizPanel;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private GameObject summaryPanel;

    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private AnswerButton[] answerButtons;

    [SerializeField] private GameObject retryButton;
    [SerializeField] private AreaChart areaChart;

    [SerializeField] private SummaryChart summaryChart;

    [SerializeField] private AudioClip quizPassedSound;
    [SerializeField] private AudioClip quizFailedSound;
    [SerializeField] private AudioClip finishGameSound;

    [SerializeField] private float delayFor = 1f;
    [SerializeField, Range(0f, 100f)] private float passingAccuracyPercent = 80f;

    private readonly List<WordData> questions = new List<WordData>();

    private int currentWordIndex;
    private int correctAnswers;
    private int areaNumber;

    private AreaData currentAreaData;
    private WordData currentWord;
    private GameObject lastSelectedObject;

    private bool isProcessingAnswer;
    private bool quizFinished;
    private bool quizPassed;

    public void StartAreaQuiz(int selectedAreaNumber, AreaData areaData)
    {
        if (areaData == null || answerButtons == null)
            return;

        if (GameProgressManager.Instance == null || !GameProgressManager.Instance.IsAreaQuizUnlocked(selectedAreaNumber))
            return;

        areaNumber = selectedAreaNumber;
        currentAreaData = areaData;
        questions.Clear();

        foreach (LevelData level in areaData.levels)
        {
            if (level == null || level.words == null)
                continue;

            foreach (WordData word in level.words)
            {
                if (word != null)
                    questions.Add(word);
            }
        }

        if (questions.Count == 0)
            return;

        currentWordIndex = 0;
        correctAnswers = 0;
        currentWord = null;
        isProcessingAnswer = false;
        quizFinished = false;
        quizPassed = false;
        lastSelectedObject = null;

        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (retryButton != null)
            retryButton.SetActive(false);

        if (quizPanel != null)
            quizPanel.SetActive(true);

        if (resultText != null)
            resultText.text = "";

        if (questionText != null)
            questionText.text = "";

        UpdateScoreText();
        ShuffleQuestions();
        SetupAnswerButtons();
        ShowCurrentWord();
        StartCoroutine(InitializeQuizSelection());
    }

    private void ShuffleQuestions()
    {
        for (int i = questions.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);

            WordData temp = questions[i];
            questions[i] = questions[j];
            questions[j] = temp;
        }
    }

    private void SetupAnswerButtons()
    {
        foreach (AnswerButton button in answerButtons)
        {
            if (button == null)
                continue;

            button.Setup(button.answer, () =>
            {
                SubmitAnswer(button.answer);
            });
        }
    }

    private void Update()
    {
        if (isProcessingAnswer || quizFinished || EventSystem.current == null)
            return;

        GameObject selectedObject = EventSystem.current.currentSelectedGameObject;

        if (selectedObject == lastSelectedObject)
            return;

        lastSelectedObject = selectedObject;

        AnswerButton selectedButton = selectedObject != null ? selectedObject.GetComponent<AnswerButton>() : null;

        if (selectedButton != null)
            PreviewAnswer(selectedButton.answer);
    }

    private void ShowCurrentWord()
    {
        currentWord = questions[currentWordIndex];

        questionText.text = currentWord.prefix + "___" + currentWord.suffix;
    }

    public void PreviewAnswer(string selectedAnswer)
    {
        if (currentWord == null || isProcessingAnswer || quizFinished)
            return;

        questionText.text = currentWord.prefix + selectedAnswer + currentWord.suffix;
    }

    public void SubmitAnswer(string selectedAnswer)
    {
        if (isProcessingAnswer || quizFinished || currentWord == null)
            return;

        StartCoroutine(ProcessAnswer(selectedAnswer));
    }

    private IEnumerator ProcessAnswer(string selectedAnswer)
    {
        isProcessingAnswer = true;

        bool isCorrect = selectedAnswer == currentWord.correctAnswer;

        if (isCorrect)
        {
            correctAnswers++;

            questionText.text = currentWord.prefix + currentWord.correctAnswer + currentWord.suffix;

            if (resultText != null)
                resultText.text = "HELYES!";
        }
        else
        {
            questionText.text = currentWord.prefix + currentWord.correctAnswer + currentWord.suffix;

            if (resultText != null)
                resultText.text = "HELYTELEN. A HELYES VÁLASZ A " + TextFormatter.HighlightLetters(currentWord.correctAnswer) + " VOLT.";
        }

        currentWordIndex++;
        UpdateScoreText();

        yield return new WaitForSeconds(delayFor);

        if (resultText != null)
            resultText.text = "";

        if (currentWordIndex >= questions.Count)
        {
            FinishQuiz();
            yield break;
        }

        ShowCurrentWord();

        if (EventSystem.current != null)
        {
            GameObject selectedObject = EventSystem.current.currentSelectedGameObject;

            AnswerButton selectedButton = selectedObject != null ? selectedObject.GetComponent<AnswerButton>() : null;

            if (selectedButton != null)
                PreviewAnswer(selectedButton.answer);
        }

        isProcessingAnswer = false;
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
            scoreText.text = correctAnswers + " / " + currentWordIndex;
    }

    private void FinishQuiz()
    {
        quizFinished = true;

        if (questionText != null)
            questionText.text = "A KVÍZ VÉGET ÉRT!";

        if (resultText != null)
            resultText.text = "EREDMÉNY: " + correctAnswers + " / " + questions.Count + " HELYES VÁLASZ.";

        if (scoreText != null)
            scoreText.text = correctAnswers + " / " + questions.Count;

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        float accuracy = 100f * correctAnswers / questions.Count;

        quizPassed = accuracy >= passingAccuracyPercent;

        if (GameProgressManager.Instance != null)
            GameProgressManager.Instance.RecordAreaQuizResult(areaNumber, correctAnswers, questions.Count, passingAccuracyPercent);

        if (retryButton != null)
            retryButton.SetActive(!quizPassed);

        if (resultPanel != null)
            resultPanel.SetActive(true);

        if (SoundManager.instance != null)
        {
            AudioClip resultSound = quizPassed ? quizPassedSound : quizFailedSound;

            if (resultSound != null)
                SoundManager.instance.PlaySound(resultSound);
        }

        if (areaChart != null)
            areaChart.RefreshChart(areaNumber);
    }

    public void RetryAreaQuiz()
    {
        if (!quizFinished || quizPassed || currentAreaData == null)
            return;

        StartAreaQuiz(areaNumber, currentAreaData);
    }

    private IEnumerator InitializeQuizSelection()
    {
        yield return null;

        if (EventSystem.current == null || answerButtons == null || answerButtons.Length == 0)
            yield break;

        AnswerButton firstButton = answerButtons[0];

        if (firstButton == null || !firstButton.gameObject.activeInHierarchy)
            yield break;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstButton.gameObject);

        lastSelectedObject = firstButton.gameObject;
        PreviewAnswer(firstButton.answer);

        yield return null;
    }

    public bool TryShowFinalPanel()
    {
        if (!quizFinished || !quizPassed || areaNumber != 5)
            return false;

        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (quizPanel != null)
            quizPanel.SetActive(false);

        if (summaryPanel != null)
            summaryPanel.SetActive(true);

        if (SoundManager.instance != null && finishGameSound != null)
            SoundManager.instance.PlaySound(finishGameSound);

        if (summaryChart != null)
            summaryChart.RefreshChart();

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        return true;
    }
}
