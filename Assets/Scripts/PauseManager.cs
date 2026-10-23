using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private InputActionReference pauseInput;

    [SerializeField] private MonoBehaviour playerScript;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;

    private bool isPaused;
    private bool wasPlayerEnabledBeforePause;
    private bool isInputEnabled;

    private void Awake()
    {
        isPaused = false;
        Time.timeScale = 1.0f;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    private void OnEnable()
    {
        if (pauseInput == null || pauseInput.action == null)
            return;

        isInputEnabled = !pauseInput.action.enabled;

        if (isInputEnabled)
            pauseInput.action.Enable();

        pauseInput.action.performed += OnPausePerformed;
    }

    private void OnDisable()
    {
        if (pauseInput != null && pauseInput.action != null)
        {
            pauseInput.action.performed -= OnPausePerformed;

            if (isInputEnabled)
                pauseInput.action.Disable();
        }

        isInputEnabled = false;
        Time.timeScale = 1.0f;
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "WorldMap")
        {
            ReturnToStartMenu();
            return;
        }

        if (settingsPanel != null && settingsPanel.activeSelf)
        {
            OnBackClick();
            return;
        }

        if (DialoguePopUp.activeDialogue != null)
        {
            DialoguePopUp.activeDialogue.CloseFromPause();
            return;
        }

        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        if (isPaused)
            return;

        isPaused = true;
        Time.timeScale = 0f;

        if (playerScript != null)
        {
            wasPlayerEnabledBeforePause = playerScript.enabled;
            playerScript.enabled = false;
        }

        playerScript.enabled = false;
        settingsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1.0f;

        if (playerScript != null)
            playerScript.enabled = wasPlayerEnabledBeforePause;

        playerScript.enabled = true;
        settingsPanel.SetActive(false);
        pausePanel.SetActive(false);
    }

    public void OnSettingsClick()
    {
        isPaused = true;
        Time.timeScale = 0f;

        playerScript.enabled = false;
        pausePanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void OnBackClick()
    {
        isPaused = true;
        Time.timeScale = 0f;

        playerScript.enabled = false;
        settingsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    public void ReturnToWorldMap()
    {
        isPaused = false;
        Time.timeScale = 1.0f;

        SceneManager.LoadScene("WorldMap");
    }

    public void ReturnToStartMenu()
    {
        isPaused = false;
        Time.timeScale = 1.0f;

        SceneManager.LoadScene("StartingMenu");
    }
}
