using UnityEngine;
using TMPro;

public class GemCounter : MonoBehaviour
{
    [SerializeField] private int totalGems;
    [SerializeField] private TextMeshProUGUI gemCounterText;
    [SerializeField] private GameObject levelCompleteUI;
    [SerializeField] private CountdownTimer countdownTimer; // drag GameManager here

    private int collectedGems = 0;

    private void OnEnable()
    {
        PlayerDetectAndCollect.onCollected += OnGemCollected;
    }

    private void OnDisable()
    {
        PlayerDetectAndCollect.onCollected -= OnGemCollected;
    }

    private void Start()
    {
        UpdateUI();
    }

    private void OnGemCollected()
    {
        collectedGems++;
        UpdateUI();
        if (collectedGems >= totalGems)
            LevelComplete();
    }

    private void UpdateUI()
    {
        if (gemCounterText == null) return;
        int remaining = totalGems - collectedGems;
        gemCounterText.text = remaining > 0
            ? $" {remaining} gems remaining"
            : "✨ All gems collected!";
    }

    private void LevelComplete()
    {
        // Stop the countdown timer
        if (countdownTimer != null)
            countdownTimer.StopTimer();

        if (levelCompleteUI != null)
        {
            levelCompleteUI.SetActive(true);
            Time.timeScale = 0f;
        }
    }
}