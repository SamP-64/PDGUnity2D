using UnityEngine;

public class PlayerController : MonoBehaviour
{


    public float moveSpeed;
    public float speedX, speedY;
    Rigidbody2D rb;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        Vector2 movement = input * moveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(rb.position + movement);
    }


    public int score = 0;

    void OnTriggerEnter2D(Collider2D other)
    {
        Coin coin = other.GetComponent<Coin>();

        if (coin != null)
        {
            Destroy(other.gameObject);
            Debug.Log("Score: " + score);
        }
    }

}
