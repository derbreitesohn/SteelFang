using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private PlayerMovementController playerMovement;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject pauseButtonsPanel;

    public void ResumeGame()
    {
        playerMovement.ResumeGame();
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OpenSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);
        if (pauseButtonsPanel != null) pauseButtonsPanel.SetActive(false);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (pauseButtonsPanel != null) pauseButtonsPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f; 
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}