using UnityEngine;
using System.Collections;
public class PlayerController : MonoBehaviour
{

    [SerializeField] public MiniMap MiniMap;
    [SerializeField] TurnManager TurnManager;
    public RoomFirstDG dg;
    public PlayerStats playerStats;

    public int cellX;
    public int cellY;

    public int coins = 0;
    Vector2Int inputDir = Vector2Int.zero;
 
    [SerializeField] private int currentRoomNum;

    void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        playerStats.IntializeStats(1, 30, 8, 8);
    }
    #region Process Input
    void Update()
    {
        CheckHoldingKey();
        MiniMap.DrawMinimap();
        SetInputDirection();
        if (TurnManager.Instance.IsTurnRunning()) { return; }
        CheckAttack();
    }

    private Vector2Int lastCell = new Vector2Int(0, 0); // holds the last cell the player was in
    float turnDelay = 0.1f;
    bool holding = false; // holding movement key
    float holdTime;

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

        if (!Dungeon.IsInsideGrid(targetCell)) { return; }
        if (!Dungeon.IsValidMove(new Vector2Int(targetCell.x, targetCell.y))) { return; }

        Vector3 worldPos = new Vector3( targetCell.x + 0.5f, targetCell.y + 0.5f, 0f); // move
        transform.position = worldPos;

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

          if(Dungeon.Grid[newCell.x, newCell.y].roomNum == dg.monsterRoom)
            {
                GameManager.Instance.textLog.AddMessage("You Found a Monster House!");
                dg.SpawnMonsterHouse();
            }
                    
            StartCoroutine(TurnManager.Instance.NextTurn(turnDelay));

            Dungeon.RevealAroundPlayer(cellX, cellY);
            MiniMap.DrawMinimap();
        }

    }
    #endregion 
    #region Input
    void CheckHoldingKey()
    {
        
        holding = Input.GetKey(KeyCode.A) || // Hold tracking 
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
    void SetInputDirection() // Speeds game up if the player holds down a movement key
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
                int coinValue = Random.Range(5, 16);
                GameManager.Instance.textLog.AddMessage("Player Collected " + coinValue + " Gold!");
                coins = coins + coinValue;
                Dungeon.Grid[cellX, cellY].cellType = CellType.Player;
                return;
        }
        else if (other.TryGetComponent(out Potion potion))
        {
            DestroyCollidedObject(other);
            playerStats.RestoreHealth(potion.healthToRestore);
            GameManager.Instance.textLog.AddMessage("Player restored " + potion.healthToRestore + " HP!");
        }
        else if (other.TryGetComponent(out Stairs stairs))
        {
            var cell = Dungeon.Grid[cellX, cellY];
            if (!cell.isStairs)
            {
                return;
            }
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
    void CheckAttack() // Cecks what way player is shooting
    {
        // attack
        if (Input.GetKeyDown(KeyCode.UpArrow))
            StartCoroutine(Attack(cellX, cellY + 1));

        if (Input.GetKeyDown(KeyCode.DownArrow))
            StartCoroutine(Attack(cellX, cellY - 1));

        if (Input.GetKeyDown(KeyCode.LeftArrow))
            StartCoroutine(Attack(cellX - 1, cellY));

        if (Input.GetKeyDown(KeyCode.RightArrow))
            StartCoroutine(Attack(cellX + 1, cellY));

        if (Input.GetKeyDown(KeyCode.I)) RangedAttack(Vector2Int.up);
        if (Input.GetKeyDown(KeyCode.K)) RangedAttack(Vector2Int.down);
        if (Input.GetKeyDown(KeyCode.J)) RangedAttack(Vector2Int.left);
        if (Input.GetKeyDown(KeyCode.L)) RangedAttack(Vector2Int.right);
    }

    IEnumerator Attack(int x, int y)
    {
        if (!Dungeon.IsInsideGrid(new Vector2Int(x, y))) { yield break; }

        TurnManager.Instance.StartTurn();

        var cell = Dungeon.Grid[x, y];

        Vector3 originalPos = transform.position;
        Vector3 targetPos = new Vector3(x + 0.5f, y + 0.5f, 0f);

        transform.position = Vector3.Lerp(originalPos, targetPos, 0.3f); // Move towards enemy

        yield return new WaitForSeconds(0.1f);  // Stay in attack position

        if (cell.enemyOnCell != null)
        {
           
            if (cell.enemyOnCell.TryGetComponent<EnemyStats>(out var enemyStats))
            {
                int damage = DamageCalculator.CalculateDamage(
                    playerStats.level,
                    playerStats.attack,
                    50,
                    enemyStats.defence
                );

                yield return StartCoroutine(enemyStats.ApplyDamage(damage));

                yield return new WaitForSeconds(0.1f);

            }
        }

        transform.position = originalPos;

        yield return new WaitForSeconds(0.05f);

        TurnManager.Instance.EndTurn();
        StartCoroutine(TurnManager.Instance.NextTurn(turnDelay));
    }
    #endregion
    #region Range Attack

    [SerializeField] GameObject rangedHitFX;
    [SerializeField] public GameObject snakeHitFX;

    void RangedAttack(Vector2Int dir)
    {
        StartCoroutine(RangedAttackRoutine(dir));
        TurnManager.Instance.StartTurn();
    }
    IEnumerator RangedAttackRoutine(Vector2Int dir) // Ranged attack
    {
        Vector2Int pos = new Vector2Int(cellX, cellY);

        for (int i = 1; i <= 10; i++)
        {
            pos += dir;

            if (!Dungeon.IsInsideGrid(pos)) { break; } // break if out of bounds
            
            SpawnFX(pos, dir); // spawn arrow

            var cell = Dungeon.Grid[pos.x, pos.y];

            if (cell.cellType == CellType.Wall) { break; }

            if (cell.enemyOnCell != null)
            {
                int dmg = DamageCalculator.CalculateDamage(playerStats.level, playerStats.attack, 50, cell.enemyOnCell.GetComponent<EnemyStats>().defence);
                yield return StartCoroutine(cell.enemyOnCell.GetComponent<EnemyStats>().ApplyDamage(dmg));

                break;
            }

            yield return new WaitForSeconds(0.07f); // travel speed
        }

        yield return new WaitForSeconds(0.2f);

        TurnManager.Instance.EndTurn();
        StartCoroutine(TurnManager.Instance.NextTurn(0.3f));
    }

    void SpawnFX(Vector2Int cell, Vector2Int dir) // Spawns arrow at position
    {
        Vector3 pos = new Vector3(cell.x + 0.5f, cell.y + 0.5f, 0f);

        GameObject arrow = Instantiate(rangedHitFX, pos, Quaternion.identity);
        arrow.transform.rotation = Quaternion.Euler(0, 0, GetAngle(dir));
        Destroy(arrow, 0.1f);
    }

    float GetAngle(Vector2Int dir) // Get angle of arrow
    {
        if (dir == Vector2Int.up) return 90f;
        if (dir == Vector2Int.down) return -90f;
        if (dir == Vector2Int.left) return 180f;
        if (dir == Vector2Int.right) return 0f;

        return 0f;
    }

    #endregion

}