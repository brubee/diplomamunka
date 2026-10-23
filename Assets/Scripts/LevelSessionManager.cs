using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class LevelSessionManager : MonoBehaviour
{
    [SerializeField] private GameObject quizPanel;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private LevelQuizManager quizManager;
    [SerializeField] private LevelData levelData;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private Collider2D exitBarrier;

    [SerializeField] private PlayerMovement playerScript;
    [SerializeField] private Transform playerTrans;
    [SerializeField] private Rigidbody2D playerRB;
    [SerializeField] private Animator animator;
    [SerializeField] private float moveSpeed = 5.0f;

    [SerializeField, Range(0f, 100f)] private float targetAccuracyPercent = 80f;

    private float levelStartTime;
    private float platformingTime;
    private bool quizStarted;
    private bool levelCompleted;

    private void Start()
    {
        if (playerScript != null && levelData != null)
            playerScript.ConfigureForLevel(levelData.levelID);

        if (exitBarrier != null)
            exitBarrier.enabled = true;

        levelStartTime = Time.time;

        if (quizPanel != null)
            quizPanel.SetActive(false);

        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (SoundManager.instance != null)
            SoundManager.instance.PlayAreaMusicInLevel(levelData);
    }

    public void StartQuiz()
    {
        if (quizStarted || levelCompleted)
            return;

        quizStarted = true;
        platformingTime = Time.time - levelStartTime;

        if (playerScript != null)
            playerScript.enabled = false;

        if (quizPanel != null)
        {
            quizPanel.SetActive(true);
            quizManager.InitializeQuizSelection();
        }
    }

    public void CompleteLevel(int correctAnswers, int totalQuestions)
    {
        if (!quizStarted || levelCompleted)
            return;

        levelCompleted = true;

        if (exitBarrier != null)
            exitBarrier.enabled = false;

        if (quizPanel != null)
            quizPanel.SetActive(false);

        StartCoroutine(ExitLevelAnimation(correctAnswers, totalQuestions));
    }

    private IEnumerator ExitLevelAnimation(int correctAnswers, int totalQuestions)
    {
        if (playerScript != null)
            playerScript.enabled = false;

        if (playerTrans == null)
        {
            ShowResults(correctAnswers, totalQuestions);
            yield break;
        }

        if (virtualCamera != null)
            virtualCamera.Follow = null;

        if (playerRB  != null)
        {
            playerRB.velocity = Vector2.zero;
            playerRB.simulated = false;
        }

        float fixedY = playerTrans.position.y;
        float targetX = playerTrans.position.x + 10f;

        Camera cam = Camera.main;

        if (cam != null)
        {
            float screenRight = cam.ViewportToWorldPoint(new Vector3(1.2f, 0.5f, 0f)).x;
            targetX = Mathf.Max(targetX, screenRight);
        }

        if (animator != null)
            animator.Play("player_walk");

        while (playerTrans.position.x < targetX)
        {
            Vector3 position = playerTrans.position;

            position.x = Mathf.MoveTowards(position.x, targetX, moveSpeed * Time.deltaTime);
            position.y = fixedY;
            playerTrans.position = position;

            yield return null;
        }

        if (animator != null)
            animator.Play("player_idle");

        ShowResults(correctAnswers, totalQuestions);
    }

    private void ShowResults(int correctAnswers, int totalQuestions)
    {
        float accuracy = totalQuestions > 0 ? 100f * correctAnswers / totalQuestions : 0f;

        if (GameProgressManager.Instance != null && levelData != null)
        {
            GameProgressManager.Instance.RecordLevelResult(levelData.levelID, platformingTime, correctAnswers, totalQuestions, targetAccuracyPercent);
        }

        if (resultPanel != null)
            resultPanel.SetActive(true);
    }

    public void ReturnToWorldMap()
    {
        SceneManager.LoadScene("WorldMap");
    }
}
