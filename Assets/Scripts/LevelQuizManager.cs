using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class LevelQuizManager : MonoBehaviour
{
    [SerializeField] private LevelData levelData;
    [SerializeField] private LevelSessionManager sessionManager;

    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private AnswerButton[] answerButtons;

    [SerializeField] private float delayFor = 1f;

    [SerializeField] private AudioClip resultsSound;

    private int currentWordIndex;
    private int correctAnswers;

    private WordData currentWord;
    private GameObject lastSelectedObject;

    private bool isProcessingAnswer;
    private bool quizFinished;

    private void Start()
    {
        if (levelData == null || levelData.words.Length == 0)
            return;

        ShowCurrentWord();
        SetupAnswerButtons();

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(answerButtons[0].gameObject);
        }

        PreviewAnswer(answerButtons[0].answer);
        lastSelectedObject = answerButtons[0].gameObject;
    }

    private void Update()
    {
        if (isProcessingAnswer || quizFinished || EventSystem.current == null)
        {
            return;
        }

        GameObject selectedObject = EventSystem.current.currentSelectedGameObject;

        if (selectedObject != lastSelectedObject)
        {
            lastSelectedObject = selectedObject;

            AnswerButton selectedButton = selectedObject != null ? selectedObject.GetComponent<AnswerButton>() : null;

            if (selectedButton != null)
            {
                PreviewAnswer(selectedButton.answer);
            }
        }
    }

    private void ShowCurrentWord()
    {
        currentWord = levelData.words[currentWordIndex];

        questionText.text = currentWord.prefix + "___" + currentWord.suffix;
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

    public void PreviewAnswer(string selectedAnswer)
    {
        if (currentWord == null || isProcessingAnswer || quizFinished)
            return;

        questionText.text = currentWord.prefix + selectedAnswer + currentWord.suffix;
    }

    public void SubmitAnswer(string selectedAnswer)
    {
        if (isProcessingAnswer || quizFinished)
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
            {
                resultText.text = "HELYTELEN. A HELYES VÁLASZ A " + TextFormatter.HighlightLetters(currentWord.correctAnswer) + " VOLT.";
            }
        }

        yield return new WaitForSeconds(delayFor);

        if (resultText != null)
            resultText.text = "";

        currentWordIndex++;

        if (currentWordIndex >= levelData.words.Length)
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
            {
                PreviewAnswer(selectedButton.answer);
            }
        }

        isProcessingAnswer = false;
    }

    private void FinishQuiz()
    {
        quizFinished = true;

        questionText.text = "KÖSZÖNÖM! A HÍD SZABAD!";

        if (resultText != null)
        {
            resultText.text = "EREDMÉNY: " + correctAnswers + " / " + levelData.words.Length + " HELYES VÁLASZ";
        }

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        if (sessionManager != null)
        {
            sessionManager.CompleteLevel(correctAnswers, levelData.words.Length);
        }

        if (SoundManager.instance != null)
            SoundManager.instance.PlaySound(resultsSound);
    }

    public void InitializeQuizSelection()
    {
        if (answerButtons == null || answerButtons.Length == 0)
            return;

        if (EventSystem.current == null)
            return;

        GameObject firstButton = answerButtons[0].gameObject;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstButton);

        lastSelectedObject = firstButton;

        PreviewAnswer(answerButtons[0].answer);
    }
}
