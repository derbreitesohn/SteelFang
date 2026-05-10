using System;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    private int groundCounter;

    public bool isGrounded => groundCounter > 0;

    public void OnTriggerEnter2D(Collider2D collission)
    {
        if(!collission.CompareTag("Ground")) {return;}
        groundCounter++;
    }
    public void OnTriggerExit2D(Collider2D collission)
    {
        if(!collission.CompareTag("Ground")) {return;}
        groundCounter = Math.Max(groundCounter -1, 0);
    }
}

