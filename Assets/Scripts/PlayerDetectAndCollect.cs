using UnityEngine;

public class PlayerDetectAndCollect : MonoBehaviour
{
    public delegate void OnCollected();
    public static event OnCollected onCollected;

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        onCollected?.Invoke();
        Destroy(gameObject);
    }
}