using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonSFX : MonoBehaviour
{
    [SerializeField] private AudioClip pressSound;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            if (SoundManager.instance != null)
                SoundManager.instance.PlaySound(pressSound);
        });
    }
}
