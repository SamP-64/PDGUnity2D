using UnityEngine;
using System.Collections;
public class PlayerController : MonoBehaviour
{
    public float moveSpeed;
    Rigidbody2D rb;

    public RoomFirstDG dg;
    public int cellX;
    public int cellY;

    public PlayerStats playerStats;
    [SerializeField] public MiniMap MiniMap;
    [SerializeField] TurnManager TurnManager;


    float turnDelay = 0.1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerStats = GetComponent<PlayerStats>();
        playerStats.IntializeStats(5, 20, 8, 8);
    }

    private Vector2Int lastCell = new Vector2Int(40, 40);

    Vector2Int inputDir = Vector2Int.zero;

    bool holding = false;
    float holdTime;

    public int score = 0;


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
            DestroyCollidedObject(other);
            textLog.AddMessage("Player Collected " + "5" + " Gold!");
            return;
        }
        else if (other.TryGetComponent(out Potion potion))
        {
            DestroyCollidedObject(other);
            playerStats.RestoreHealth(potion.healthToRestore);
            textLog.AddMessage("Player restored " + potion.healthToRestore + " HP!");
        }
        else if (other.TryGetComponent(out Stairs stairs))
        {
            GameManager.Instance.NextFloor(dg);
        }
    }

    void DestroyCollidedObject(Collider2D other)
    {
        Vector2 pos = other.transform.position;
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);

        Dungeon.Grid[x, y].cellType = CellType.Floor;
        Destroy(other.gameObject);
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
                int damage = DamageCalculator.CalculateDamage(playerStats.level, playerStats.attack, 50, enemyStats.defence);
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

        if (Input.GetKeyDown(KeyCode.I)) RangedAttack(Vector2Int.up);
        if (Input.GetKeyDown(KeyCode.K)) RangedAttack(Vector2Int.down);
        if (Input.GetKeyDown(KeyCode.J)) RangedAttack(Vector2Int.left);
        if (Input.GetKeyDown(KeyCode.L)) RangedAttack(Vector2Int.right);
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

    [SerializeField] GameObject rangedHitFX;

    void RangedAttack(Vector2Int dir)
    {
        StartCoroutine(RangedAttackRoutine(dir));
        TurnManager.Instance.StartTurn();
    }
    IEnumerator RangedAttackRoutine(Vector2Int dir)
    {
        Vector2Int pos = new Vector2Int(cellX, cellY);

        for (int i = 1; i <= 10; i++)
        {
            pos += dir;

            // bounds
            if (pos.x < 0 || pos.y < 0 ||
                pos.x >= Dungeon.Grid.GetLength(0) ||
                pos.y >= Dungeon.Grid.GetLength(1))
                break;

            SpawnFX(pos, dir); // instant visual per step

            var cell = Dungeon.Grid[pos.x, pos.y];

            if (cell.cellType == CellType.Wall)
                break;

            if (cell.itemOnCell != null &&
                cell.itemOnCell.TryGetComponent<EnemyStats>(out var enemy))
            {
                int dmg = DamageCalculator.CalculateDamage(
                    playerStats.level, playerStats.attack, 50, enemy.defence);

                enemy.ApplyDamage(dmg);
                break;
            }

            yield return new WaitForSeconds(0.07f); // travel speed
        }

        TurnManager.Instance.EndTurn();
        StartCoroutine(TurnManager.Instance.NextTurn(turnDelay));
    }

    void SpawnFX(Vector2Int cell, Vector2Int dir)
    {

        
        Vector3 pos = new Vector3(cell.x + 0.5f, cell.y + 0.5f, 0f);

        GameObject arrow = Instantiate(rangedHitFX, pos, Quaternion.identity);
        arrow.transform.rotation = Quaternion.Euler(0, 0, GetAngle(dir));
        Destroy(arrow, 0.1f);
    }

    float GetAngle(Vector2Int dir)
    {
        if (dir == Vector2Int.up) return 90f;
        if (dir == Vector2Int.down) return -90f;
        if (dir == Vector2Int.left) return 180f;
        if (dir == Vector2Int.right) return 0f;

        return 0f;
    }
}