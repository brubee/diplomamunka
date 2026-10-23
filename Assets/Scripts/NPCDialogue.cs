using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="NPC Dialogue", fileName ="New NPC Dialogue")]

public class NPCDialogue : ScriptableObject
{
    public string npcName;
    public string[] dialogueLines;
    public bool[] endDialogueLines;
    public float typingSpeed = 0.05f;

    public DialogueChoice[] choices;
}

public enum DialogueChoiceAction
{
    None,
    StartQuiz
}

[System.Serializable]
public class DialogueChoice
{
    public int dialogueIndex;
    public string[] choices;
    public int[] nextDialogueIndexes;

    public DialogueChoiceAction[] actions;
}