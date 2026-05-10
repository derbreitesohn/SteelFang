using System.Collections;
using UnityEngine;

public class LevelIntro : MonoBehaviour
{
    [SerializeField] private GameObject introPanel;
    [SerializeField] private float displayDuration = 3f;

    private void Start()
    {
        Time.timeScale = 0f;
        if (introPanel != null) introPanel.SetActive(true);
        StartCoroutine(DismissAfterRealTime());
    }

    private IEnumerator DismissAfterRealTime()
    {
        // WaitForSecondsRealtime works even when timeScale = 0
        yield return new WaitForSecondsRealtime(displayDuration);
        if (introPanel != null) introPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}