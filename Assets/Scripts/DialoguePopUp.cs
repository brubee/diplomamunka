using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DialoguePopUp : MonoBehaviour, IInteractable
{
    public static DialoguePopUp activeDialogue { get; private set; }

    public NPCDialogue dialogueData;
    private DialogueManager dialogueManager;
    private int dialogueIndex;
    private bool isTyping, isDialogueActive;

    public PlayerMovement playerScript;
    public LevelSessionManager sessionManager;

    private void Start()
    {
        dialogueManager = DialogueManager.Instance;
        dialogueManager.ShowDialogueUI(false);
    }

    public bool canInteract()
    {
        return !isDialogueActive;
    }

    public void Interact()
    {
        if (dialogueData == null)
        {
            return;
        }
        if (isDialogueActive)
        {
            NextLine();
        }
        else
        {
            StartDialogue();
        }
    }

    private void StartDialogue()
    {
        isDialogueActive = true;
        activeDialogue = this;
        dialogueIndex = 0;

        playerScript.enabled = false;

        dialogueManager.SetNPCInfo(dialogueData.npcName);
        dialogueManager.ShowDialogueUI(true);

        DisplayCurrentLine();
    }

    private void NextLine()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueManager.dialogueText.maxVisibleCharacters = dialogueManager.dialogueText.textInfo.characterCount;
            isTyping = false;
            return;
        }

        dialogueManager.ClearChoices();

        if (dialogueData.endDialogueLines.Length > dialogueIndex && dialogueData.endDialogueLines[dialogueIndex])
        {
            StopInteracting();
            return;
        }

        foreach (DialogueChoice dialogueChoice in dialogueData.choices)
        {
            if (dialogueChoice.dialogueIndex == dialogueIndex)
            {
                DisplayChoices(dialogueChoice);
                return;
            }
        }

        if (++dialogueIndex < dialogueData.dialogueLines.Length)
        {
            DisplayCurrentLine();
        }
        else
        {
            StopInteracting();
        }
    }

    IEnumerator TypeLine()
    {
        isTyping = true;

        string raw = dialogueData.dialogueLines[dialogueIndex];


        string formatted = TextFormatter.HighlightLetters(raw);
        TMP_Text textComponent = dialogueManager.dialogueText;

        textComponent.text = formatted;
        textComponent.ForceMeshUpdate();

        int totalVisibleChar = textComponent.textInfo.characterCount;
        textComponent.maxVisibleCharacters = 0;

        while (textComponent.maxVisibleCharacters < totalVisibleChar)
        {
            textComponent.maxVisibleCharacters++;
            yield return new WaitForSeconds(dialogueData.typingSpeed);
        }

        isTyping = false;
    }

    private void DisplayChoices(DialogueChoice choice)
    {
        for (int i = 0; i < choice.choices.Length; i++)
        {
            int optionIndex = i;
            int nextIndex = choice.nextDialogueIndexes[i];
            dialogueManager.CreateChoiceButton(choice.choices[i], () => ChooseOption(optionIndex, nextIndex));
        }
    }

    private void ChooseOption(int optionIndex, int nextIndex)
    {
        DialogueChoice choice = null;

        foreach (DialogueChoice item in dialogueData.choices)
        {
            if (item.dialogueIndex == dialogueIndex)
            {
                choice = item;
                break;
            }
        }
        dialogueManager.ClearChoices();

        DialogueChoiceAction action = DialogueChoiceAction.None;

        if (choice != null && choice.actions != null && optionIndex < choice.actions.Length)
            action = choice.actions[optionIndex];

        if (action == DialogueChoiceAction.StartQuiz)
        {
            CloseDialogue(false);

            if (sessionManager != null)
                sessionManager.StartQuiz();
            else
                playerScript.enabled = true;

            return;
        }

        dialogueIndex = nextIndex;
        DisplayCurrentLine();
    }

    private void DisplayCurrentLine()
    {
        StopAllCoroutines();
        StartCoroutine(TypeLine());
    }

    public void StopInteracting()
    {
        CloseDialogue(true);
    }

    private void CloseDialogue(bool restorePlayerScript)
    {
        StopAllCoroutines();
        isDialogueActive = false;

        if (activeDialogue == this)
            activeDialogue = null;

        dialogueManager.ClearChoices();
        dialogueManager.SetDialogueText("");
        dialogueManager.ShowDialogueUI(false);

        if (restorePlayerScript && playerScript != null)
            playerScript.enabled = true;
    }

    public void CloseFromPause()
    {
        if (isDialogueActive)
            StopInteracting();
    }
}