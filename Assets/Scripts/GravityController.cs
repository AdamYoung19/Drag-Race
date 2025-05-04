using UnityEngine;

public class GravityController : MonoBehaviour
{
    public Rigidbody2D rb2d;
    public int gravity;

    void Update()
    {
        if (gravity == 1)
        {
            rb2d.gravityScale += 1; 
            gravity = 0;
        }
        else if (gravity == 2)
        {
            rb2d.gravityScale -= 1;
            gravity = 0; 
        }
    }
    public void SetGravityBasedOnSpeed(float speed)
    {
        rb2d.gravityScale = speed;
    }
}

