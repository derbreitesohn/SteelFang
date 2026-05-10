using UnityEngine;

public class BounceDownAndUp : MonoBehaviour
{
    [SerializeField] private float maxY;
    [SerializeField] private float minY;
    [SerializeField] private float speed;
    
    private bool goingDown;
    private int bounceCounter;
    void Start()
    {
        goingDown = true;
        bounceCounter = 0;
    }
    
    void Update()
    {
        if (bounceCounter >= 3) { return; }
        if (goingDown)
        {
            transform.position = transform.position + Vector3.down * Time.deltaTime * speed;
            if (transform.position.y <= minY)
            {
                goingDown = false;
                maxY = minY + (maxY-minY) / 3;
                bounceCounter++;
            }
        }
        else
        {
            transform.position = transform.position + Vector3.up * Time.deltaTime * speed;
            if (transform.position.y >= maxY)
            {
                goingDown = true;
            }
        }
    }
}
