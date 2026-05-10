using UnityEngine;

public class MoveUpAndDown : MonoBehaviour
{
    [SerializeField] private float speed = 2f;
    [SerializeField] private float maxY = 3f;
    [SerializeField] private float minY = -3f;
    [SerializeField] private float waveHeight = 0.2f;
    [SerializeField] private float waveFrequency = 4f;

    private bool moveUp;
    private float startX;
    private float randomOffset;

    private void Start()
    {
        moveUp = false;
        startX = transform.position.x;
        randomOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update()
    {
        Vector3 nextPosition = transform.position;

        if (!moveUp)
        {
            nextPosition += Vector3.down * Time.deltaTime * speed;
            if (nextPosition.y <= minY)
            {
                nextPosition.y = minY;
                moveUp = true;
            }
        }
        else
        {
            nextPosition += Vector3.up * Time.deltaTime * speed;
            if (nextPosition.y >= maxY)
            {
                nextPosition.y = maxY;
                moveUp = false;
            }
        }

        nextPosition.x = startX + Mathf.Sin((Time.time + randomOffset) * waveFrequency) * waveHeight;
        transform.position = nextPosition;
    }
}
