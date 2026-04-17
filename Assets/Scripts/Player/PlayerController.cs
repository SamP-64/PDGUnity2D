using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed;
    Rigidbody2D rb;

    public RoomFirstDG dg;
    public int cellX;
    public int cellY;

    public PlayerStats Stats;
    [SerializeField] public MiniMap MiniMap;
    [SerializeField] TurnManager TurnManager;


    float turnDelay = 0.1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Stats = GetComponent<PlayerStats>();
        Stats.IntializeStats(5, 20, 8, 8);
    }

    private Vector2Int lastCell = new Vector2Int(40, 40);

    Vector2Int inputDir = Vector2Int.zero;

    bool holding = false;
    float holdTime;

    public int score = 0;

    // -------------------------
    // REVEAL MAP
    // -------------------------
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

   
    [SerializeField] TextLog textLog;
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out Coin coin))
        {
            Vector2 pos = other.transform.position;
            int x = Mathf.FloorToInt(pos.x);
            int y = Mathf.FloorToInt(pos.y);

            Dungeon.Grid[x, y].cellType = CellType.Floor;
            Destroy(other.gameObject);
            textLog.AddMessage("Player Collected " + " 5 " + " Gold!");
            return;
        }

        if (other.TryGetComponent(out Stairs stairs))
        {
            GameManager.Instance.NextFloor(dg);
        }
    }

    void Attack(int x, int y)
    {
        if (x < 0 || x >= Dungeon.Grid.GetLength(0) ||
            y < 0 || y >= Dungeon.Grid.GetLength(1))
            return;

        var cell = Dungeon.Grid[x, y];

        if (cell.itemOnCell != null)
        {
            if(cell.itemOnCell.GetComponent <EnemyStats>() != null)
            {
                EnemyStats enemyStats = cell.itemOnCell.GetComponent<EnemyStats>();
                int damage = DamageCalculator.CalculateDamage(Stats.level, Stats.attack, 50, enemyStats.defence);
                enemyStats.ApplyDamage(damage);
            }

          
        }

        StartCoroutine(TurnManager.Instance.NextTurn(turnDelay));
    }

    
    void Update()
    {

        // HOLD tracking 
        holding = Input.GetKey(KeyCode.A) ||
                  Input.GetKey(KeyCode.D) ||
                  Input.GetKey(KeyCode.W) ||
                  Input.GetKey(KeyCode.S);

        if (holding)
            holdTime += Time.deltaTime;
        else
            holdTime = 0f;

        // step input
        MiniMap.DrawMinimap();
        if (inputDir == Vector2Int.zero)
        {
            turnDelay = 0.1f;
            if (Input.GetKeyDown(KeyCode.A)) inputDir = Vector2Int.left;
            else if (Input.GetKeyDown(KeyCode.D)) inputDir = Vector2Int.right;
            else if (Input.GetKeyDown(KeyCode.W)) inputDir = Vector2Int.up;
            else if (Input.GetKeyDown(KeyCode.S)) inputDir = Vector2Int.down;
        }

        if (holding && holdTime > 0.3)
        {
            turnDelay = 0f;
            if (Input.GetKey(KeyCode.A)) inputDir = Vector2Int.left;
            else if (Input.GetKey(KeyCode.D)) inputDir = Vector2Int.right;
            else if (Input.GetKey(KeyCode.W)) inputDir = Vector2Int.up;
            else if (Input.GetKey(KeyCode.S)) inputDir = Vector2Int.down;
        }

        CheckAttack();
    }

    void CheckAttack()
    {
        // attack
        if (Input.GetKeyDown(KeyCode.UpArrow))
            Attack(cellX, cellY + 1);

        if (Input.GetKeyDown(KeyCode.DownArrow))
            Attack(cellX, cellY - 1);

        if (Input.GetKeyDown(KeyCode.LeftArrow))
            Attack(cellX - 1, cellY);

        if (Input.GetKeyDown(KeyCode.RightArrow))
            Attack(cellX + 1, cellY);
    }
    void FixedUpdate()
    {

        if (TurnManager.Instance.IsTurnRunning())
            return;

        if (inputDir == Vector2Int.zero)
            return;

        Vector2Int movement = inputDir;

        // IMPORTANT RULE:
        // before 1 second hold = single step only
        if (holdTime < 1f)
        {
            inputDir = Vector2Int.zero;
        }
        else
        {

        }
        Debug.Log(inputDir);

            cellX = Mathf.FloorToInt(transform.position.x);
        cellY = Mathf.FloorToInt(transform.position.y);

        Vector2Int currentCell = new Vector2Int(cellX, cellY);
        Vector2Int targetCell = currentCell + movement;

        // bounds check
        if (targetCell.x < 0 || targetCell.x >= Dungeon.Grid.GetLength(0) ||
            targetCell.y < 0 || targetCell.y >= Dungeon.Grid.GetLength(1))
            return;

        // collision check
        if (Dungeon.Grid[targetCell.x, targetCell.y].cellType == CellType.Wall ||
            Dungeon.Grid[targetCell.x, targetCell.y].cellType == CellType.Enemy)
            return;

        // move
        Vector3 worldPos = new Vector3(
    targetCell.x + 0.5f,
    targetCell.y + 0.5f,
    0f);

        rb.MovePosition(worldPos);


        cellX = targetCell.x;
        cellY = targetCell.y;



        // turn system
        //if (currentCell != lastCell)
        //{
        //    // 1. CLEAR OLD POSITION (ALWAYS FIRST)
        //    Dungeon.Grid[currentCell.x, currentCell.y].cellType = CellType.Floor;

        //    // 2. UPDATE POSITION (you already moved physically before this block)
        //    lastCell = currentCell;

        //    // 3. SET NEW POSITION
        //    Dungeon.Grid[cellX, cellY].cellType = CellType.player;

        //    StartCoroutine(TurnManager.Instance.NextTurn(turnDelay));

        //    RevealAroundPlayer(cellX, cellY);
        //    MiniMap.DrawMinimap();
        //}

        Vector2Int previousCell = lastCell;
        Vector2Int newCell = targetCell;

        if (previousCell != newCell)
        {
            // clear OLD (this is the key fix)
            Dungeon.Grid[previousCell.x, previousCell.y].cellType = CellType.Floor;

            // set NEW
            Dungeon.Grid[newCell.x, newCell.y].cellType = CellType.player;

            // update tracking
            lastCell = newCell;
            cellX = newCell.x;
            cellY = newCell.y;

            StartCoroutine(TurnManager.Instance.NextTurn(turnDelay));

            RevealAroundPlayer(cellX, cellY);
            MiniMap.DrawMinimap();
        }

    }
}