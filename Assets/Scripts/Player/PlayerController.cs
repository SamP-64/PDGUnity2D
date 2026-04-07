using UnityEngine;

public class PlayerController : MonoBehaviour
{


    public float moveSpeed;
    public float speedX, speedY;
    Rigidbody2D rigidbody2D;
    void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        speedX = Input.GetAxis("Horizontal") * moveSpeed;
        speedY = Input.GetAxis("Vertical") * moveSpeed;
        rigidbody2D.linearVelocity = new Vector2 (speedX, speedY);
    }
}
