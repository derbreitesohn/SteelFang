using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    private const string GameOverSceneName = "MainMenu";
    private bool hasTriggered;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryLoadGameOverScene(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryLoadGameOverScene(other.gameObject);
    }

    private void TryLoadGameOverScene(GameObject otherObject)
    {
        if (hasTriggered || !otherObject.CompareTag(playerTag))
        {
            return;
        }
        hasTriggered = true;
        SceneManager.LoadScene(GameOverSceneName);
    }
}
