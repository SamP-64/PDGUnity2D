using UnityEngine;
using System.Collections;
public class PlayerController : MonoBehaviour
{

    [SerializeField] public MiniMap MiniMap;
    [SerializeField] TurnManager TurnManager;
    [SerializeField] TextLog textLog;
    public RoomFirstDG dg;
    public PlayerStats playerStats;
    Rigidbody2D rb;

    public float moveSpeed;
    public int cellX;
    public int cellY;

    float turnDelay = 0.1f;
    bool holding = false;
    float holdTime;
    public int score = 0;
    Vector2Int inputDir = Vector2Int.zero;
   [SerializeField] private int currentRoomNum;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerStats = GetComponent<PlayerStats>();
        playerStats.IntializeStats(5, 20, 8, 8);
    }

    void Update()
    {
        CheckHoldingKey();
        MiniMap.DrawMinimap();
        SetInputDirection();
        if (TurnManager.Instance.IsTurnRunning())
            return;

        CheckAttack();
    }

    private Vector2Int lastCell = new Vector2Int(0, 0);
    void FixedUpdate()
    {

        if (TurnManager.Instance.IsTurnRunning())
            return;

        if (inputDir == Vector2Int.zero)
            return;

        Vector2Int movement = inputDir;

        if (holdTime < 0.2f) 
        {
            inputDir = Vector2Int.zero;
        }

        cellX = Mathf.FloorToInt(transform.position.x);
        cellY = Mathf.FloorToInt(transform.position.y);

        Vector2Int currentCell = new Vector2Int(cellX, cellY);
        Vector2Int targetCell = currentCell + movement;

        // bounds check
        if (targetCell.x < 0 || targetCell.x >= Dungeon.Grid.GetLength(0) ||
            targetCell.y < 0 || targetCell.y >= Dungeon.Grid.GetLength(1))
            return;

        // collision check
        if (!Dungeon.IsValidMove(new Vector2Int(targetCell.x, targetCell.y)))
            return;

        Vector3 worldPos = new Vector3( targetCell.x + 0.5f, targetCell.y + 0.5f, 0f); // move
        transform.position = worldPos;
        // rb.MovePosition(worldPos);

        cellX = targetCell.x;
        cellY = targetCell.y;

        Vector2Int previousCell = lastCell;
        Vector2Int newCell = targetCell;

        if (previousCell != newCell)
        {
           
            Dungeon.Grid[previousCell.x, previousCell.y].cellType = CellType.Floor;
            Dungeon.Grid[newCell.x, newCell.y].cellType = CellType.Player;

            lastCell = newCell;
            cellX = newCell.x;
            cellY = newCell.y;

            currentRoomNum = Dungeon.Grid[newCell.x, newCell.y].roomNum;

          if (  Dungeon.Grid[newCell.x, newCell.y].roomNum == dg.monsterRoom)
            {
                textLog.AddMessage("You Found a Monster House!");
                dg.SpawnMonsterHouse();
            }
                    
            StartCoroutine(TurnManager.Instance.NextTurn(turnDelay));

            Dungeon.RevealAroundPlayer(cellX, cellY);
            MiniMap.DrawMinimap();
        }

    }

    #region Input
    void CheckHoldingKey()
    {
        // HOLD tracking 
        holding = Input.GetKey(KeyCode.A) ||
                  Input.GetKey(KeyCode.D) ||
                  Input.GetKey(KeyCode.W) ||
                  Input.GetKey(KeyCode.S);

        if (holding)
        {
            holdTime += Time.deltaTime;
        }
        else
        {
            holdTime = 0f;
        }
    }
    void SetInputDirection()
    {
        if (inputDir == Vector2Int.zero)
        {
            turnDelay = 0.3f;
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

    }
    #endregion 
    #region Collectables

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

    #endregion 
    #region Attack
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

    void Attack(int x, int y)
    {
        if (x < 0 || x >= Dungeon.Grid.GetLength(0) ||
            y < 0 || y >= Dungeon.Grid.GetLength(1))
            return;

        var cell = Dungeon.Grid[x, y];

        if (cell.itemOnCell != null)
        {
            if (cell.itemOnCell.GetComponent<EnemyStats>() != null)
            {
                EnemyStats enemyStats = cell.itemOnCell.GetComponent<EnemyStats>();
                int damage = DamageCalculator.CalculateDamage(playerStats.level, playerStats.attack, 50, enemyStats.defence);
                enemyStats.ApplyDamage(damage);
            }
        }

        StartCoroutine(TurnManager.Instance.NextTurn(turnDelay));
    }
    #endregion
    #region Range Attack

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

            if (pos.x < 0 || pos.y < 0 ||
                pos.x >= Dungeon.Grid.GetLength(0) ||
                pos.y >= Dungeon.Grid.GetLength(1))
                break; // break if out of bounds

            SpawnFX(pos, dir); // spawn arrow

            var cell = Dungeon.Grid[pos.x, pos.y];

            if (cell.cellType == CellType.Wall)
                break;

            if (cell.itemOnCell != null && cell.itemOnCell.TryGetComponent<EnemyStats>(out var enemy))
            {
                int dmg = DamageCalculator.CalculateDamage(playerStats.level, playerStats.attack, 50, enemy.defence);

                enemy.ApplyDamage(dmg);
                break;
            }

            yield return new WaitForSeconds(0.07f); // travel speed
        }

        TurnManager.Instance.EndTurn();
        StartCoroutine(TurnManager.Instance.NextTurn(0f));
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

    #endregion


}