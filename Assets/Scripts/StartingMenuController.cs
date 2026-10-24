using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartingMenuController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private float moveSpeed = 5.0f;

    [SerializeField] private Transform startPos;
    [SerializeField] private Transform idlePos;
    [SerializeField] private Transform exitPos;

    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsMenuPanel;
    [SerializeField] private GameProgressManager gameProgressManager;

    private bool startPressed = false;

    private void Start()
    {
        mainMenuPanel.SetActive(true);
        settingsMenuPanel.SetActive(false);

        if (SoundManager.instance != null)
            SoundManager.instance.PlayMenuMusic();

        rb.position = startPos.position;
        animator.Play("player_walk");
        StartCoroutine(Intro());
    }

    private IEnumerator Intro()
    {
        yield return MoveTo(idlePos.position, moveSpeed);
        animator.Play("player_idle");
    }

    private IEnumerator MoveTo(Vector2 targetPos, float moveSpeed)
    {
        while (Vector2.Distance(rb.position, targetPos) > 0.02f)
        {
            Vector2 nextPos = Vector2.MoveTowards(rb.position, targetPos, moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(nextPos);
            yield return new WaitForFixedUpdate();
        }

        rb.MovePosition(targetPos);
    }

    public void OnStartClick()
    {
        if (!startPressed)
        {
            StartCoroutine(StartGame());
        }
    }

    private IEnumerator StartGame()
    {
        startPressed = true;

        mainMenuPanel.SetActive(false);
        settingsMenuPanel.SetActive(false);

        animator.Play("player_walk");
        yield return MoveTo(exitPos.position, moveSpeed);

        SceneManager.LoadScene("WorldMap");
    }

    public void OnSettingsClick()
    {
        if (startPressed) return;

        mainMenuPanel.SetActive(false);
        settingsMenuPanel.SetActive(true);
    }

    public void OnBackClick()
    {
        settingsMenuPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void OnExitClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    public void OnResetClick()
    {
        gameProgressManager.ResetProgress();
    }
}
