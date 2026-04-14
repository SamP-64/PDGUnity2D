using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public float moveSpeed;
    Rigidbody2D rb;
    public RoomFirstDG dg;
    public int cellX;
    public int cellY;

    public PlayerStats Stats;
    [SerializeField] MiniMap MiniMap;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Stats = GetComponent<PlayerStats>();
        Stats.IntializeStats(20, 5, 5);
    }

    // Update is called once per frame
    private Vector2Int lastCell = new Vector2Int(40, 40);

    void FixedUpdate()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        Vector2 movement = input * moveSpeed * Time.fixedDeltaTime;

        Vector2 newPosition = rb.position + movement;
        rb.MovePosition(newPosition);

        cellX = Mathf.FloorToInt(newPosition.x);
        cellY = Mathf.FloorToInt(newPosition.y);

        Dungeon.Grid[cellX, cellY].cellType = CellType.player;
        Vector2Int currentCell = new Vector2Int(cellX, cellY);

        if (currentCell != lastCell)
        {

            Dungeon.Grid[lastCell.x, lastCell.y].cellType = CellType.Floor;
            lastCell = currentCell;

            if (cellX >= 0 && cellX <= dg.dungeonWidth && cellY >= 0 && cellY <= dg.dungeonHeight )
            {

                TurnManager.NextTurn();

                RevealAroundPlayer(cellX, cellY);
                MiniMap.DrawMinimap();

            }

        }
    }

    public void RevealAroundPlayer(int playerX, int playerY)
    {
  
        int startX = playerX - 5;
        int startY = playerY - 5;

        if (startX < 0) startX = 0;
        if (startY < 0) startY = 0;

        if (startX + 10 > dg.dungeonWidth) startX = dg.dungeonWidth - 10;
        if (startY + 10 > dg.dungeonHeight) startY = dg.dungeonHeight - 10;

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

            Vector2 pos = other.transform.position;
            int x = Mathf.FloorToInt(pos.x);
            int y = Mathf.FloorToInt(pos.y);

            Dungeon.Grid[x, y].cellType = CellType.Floor;
            Destroy(other.gameObject);
            // Debug.Log("Score: " + score);
            return;
        }

        if (other.TryGetComponent(out Stairs stairs))
        {
            Debug.Log(other.name);
            GameManager.Instance.NextFloor(dg);
        }
    }
}
