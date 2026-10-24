using UnityEngine;
using UnityEngine.UI;

public class AnswerButton : MonoBehaviour
{
    public string answer;

    public void Setup(string answerText, UnityEngine.Events.UnityAction onClick)
    {
        answer = answerText;
        GetComponent<Button>().onClick.RemoveAllListeners();
        GetComponent<Button>().onClick.AddListener(onClick);
    }
}
