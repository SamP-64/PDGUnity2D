using UnityEngine;

public class PlayerController : MonoBehaviour
{


    public float moveSpeed;
    public float speedX, speedY;
    Rigidbody2D rb;
    public RoomFirstDG dungeonGenerator;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    private Vector2Int lastCell = new Vector2Int(40, 40);

    void FixedUpdate()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        Vector2 movement = input * moveSpeed * Time.fixedDeltaTime;

        Vector2 newPosition = rb.position + movement;
        rb.MovePosition(newPosition);

        int cellX = Mathf.FloorToInt(newPosition.x);
        int cellY = Mathf.FloorToInt(newPosition.y);

        Dungeon.Grid[cellX, cellY].cellType = CellType.player;
        Vector2Int currentCell = new Vector2Int(cellX, cellY);

        if (currentCell != lastCell)
        {
            if (cellX >= 0 && cellX <= 40 && cellY >= 0 && cellY <= 40)
            {
                RevealAroundPlayer(cellX, cellY);
            }

            Dungeon.Grid[lastCell.x ,lastCell.y ].cellType = CellType.Empty;
            lastCell = currentCell;
           
        }
    }

    public void RevealAroundPlayer(int playerX, int playerY)
    {
  
        int startX = playerX - 5;
        int startY = playerY - 5;

        if (startX < 0) startX = 0;
        if (startY < 0) startY = 0;

        if (startX + 10 > 40) startX = 40 - 10;
        if (startY + 10 > 40) startY = 40 - 10;

        for (int x = startX; x < startX + 10; x++)
        {
            for (int y = startY; y < startY + 10; y++)
            {
                Dungeon.Grid[x, y].traversed = true;
            }
        }


    }

    public int score = 0;

    void OnTriggerEnter2D(Collider2D other)
    {

        if (other.TryGetComponent(out Coin coin))
        {
            Destroy(other.gameObject);
            // Debug.Log("Score: " + score);
            return;
        }

        if (other.TryGetComponent(out Stairs stairs))
        {
            Debug.Log(other.name);
            GameManager.Instance.NextFloor(dungeonGenerator);
        }
    }
}
