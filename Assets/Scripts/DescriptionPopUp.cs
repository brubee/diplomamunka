using UnityEngine;
using TMPro;

public class DescriptionPopUp : MonoBehaviour, IInteractable
{
    [SerializeField] public GameObject popUp;
    [SerializeField] private TMP_Text description;
    [SerializeField] private string text;
    [SerializeField] private GameObject outlineShine;

    public bool isShining { get; private set; } = true;

    private void Start()
    {
        popUp.SetActive(false);
        description.text = TextFormatter.HighlightLetters(text);

        if (outlineShine != null)
            outlineShine.SetActive(isShining);
    }

    public bool canInteract()
    {
        return true;
    }

    public void Interact()
    {
        if (!canInteract()) return;
        popUp.SetActive(true);

        if (isShining)
        {
            isShining = false;

            if (outlineShine != null)
                outlineShine.SetActive(false);
        }
    }

    public void StopInteracting()
    {
        popUp.SetActive(false);
    }
}
