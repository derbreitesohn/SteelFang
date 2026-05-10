using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    public bool isGrounded;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float checkRadius = 0.2f;

    private void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(transform.position, checkRadius, groundLayer);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, checkRadius);
    }
}