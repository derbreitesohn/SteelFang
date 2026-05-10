using UnityEngine;

public class MoveLeftAndRight : MonoBehaviour
{
    [SerializeField] private float speed = 2f;
    [SerializeField] private float minX = -5f;
    [SerializeField] private float maxX = 5f;
    [SerializeField] private float waveHeight = 0.2f;
    [SerializeField] private float waveFrequency = 4f;

    private bool movingRight;
    private float startY;
    private float randomOffset;
    private float originalXScale;

    private void Start()
    {
        movingRight = false;
        startY = transform.position.y;
        randomOffset = Random.Range(0f, Mathf.PI * 2f);
        originalXScale = Mathf.Abs(transform.localScale.x);
        UpdateFacingDirection();
    }

    private void Update()
    {
        Vector3 nextPosition = transform.position;

        if (!movingRight)
        {
            nextPosition += Vector3.left * Time.deltaTime * speed;
            if (nextPosition.x <= minX)
            {
                nextPosition.x = minX;
                movingRight = true;
                UpdateFacingDirection();
            }
        }
        else
        {
            nextPosition += Vector3.right * Time.deltaTime * speed;
            if (nextPosition.x >= maxX)
            {
                nextPosition.x = maxX;
                movingRight = false;
                UpdateFacingDirection();
            }
        }

        nextPosition.y = startY + Mathf.Sin((Time.time + randomOffset) * waveFrequency) * waveHeight;
        transform.position = nextPosition;
    }

    private void UpdateFacingDirection()
    {
        float directionScale = movingRight ? originalXScale : -originalXScale;
        transform.localScale = new Vector3(directionScale, transform.localScale.y, transform.localScale.z);
    }
}
