using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class CountdownTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] private float timeLimit = 120f; // 2 minutes default
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject gameOverUI; // optional popup
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color urgentColor = Color.red; // flashes red when low
    [SerializeField] private float urgentThreshold = 20f; // last 20 seconds

    private float timeRemaining;
    private bool isRunning = true;
    private bool hasEnded = false;

    private void Start()
    {
        timeRemaining = timeLimit;
        UpdateTimerDisplay();
    }

    private void Update()
    {
        if (!isRunning || hasEnded) return;
        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            TimerEnd();
        }
        UpdateTimerDisplay();
    }

    private void UpdateTimerDisplay()
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        timerText.text = $" {minutes:00}:{seconds:00}";

        if (timeRemaining <= urgentThreshold)
        {
            timerText.color = Mathf.Sin(Time.time * 5f) > 0 ? urgentColor : normalColor;
        }
        else
        {
            timerText.color = normalColor;
        }
    }

    private void TimerEnd()
    {
        hasEnded = true;
        isRunning = false;
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
            Time.timeScale = 0f;
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
    public void StopTimer()
    {
        isRunning = false;
    }

    public void SetPaused(bool paused)
    {
        isRunning = !paused;
    }
}