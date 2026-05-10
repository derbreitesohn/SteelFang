using UnityEngine;
using TMPro;

public class TutorialTrigger : MonoBehaviour
{
    [SerializeField] private GameObject popupUI;
    [SerializeField] private float hideAfterSeconds = 3f;
    private bool triggered = false;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered || !other.CompareTag("Player")) return;
        triggered = true;
        popupUI.SetActive(true);
        Invoke(nameof(HidePopup), hideAfterSeconds);
    }
    private void HidePopup() => popupUI.SetActive(false);
}