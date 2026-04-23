using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;
//using static UnityEditor.Progress;

public class Enemy : MonoBehaviour
{
    public Vector2Int gridPos;
    public GameObject collectedItem;
    public EnemyType enemyType;

    [HideInInspector] public PlayerController pc;
    [HideInInspector] public PlayerStats ps;
    [HideInInspector] public EnemyStats enemyStats;
    
    [SerializeField] public int sightRange = 9;
    [SerializeField] int rangedAttackRange = 6;
    [SerializeField] int rangedPower = 30;
    [SerializeField] int meleePower = 50;

    public void SetPosition(Vector2Int pos)
    {
        gridPos = pos;
        transform.position = new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0f);
    }

    public void SetStartPosition(Vector2Int pos) // Initialise enemy 
    {
        enemyStats = GetComponent<EnemyStats>();
        gridPos = pos;
        pc = FindFirstObjectByType<PlayerController>();
        ps = FindFirstObjectByType<PlayerStats>();
    }

    #region Find Player
    public bool IsNextToPlayer()
    {
        if (Mathf.Abs(pc.cellX - gridPos.x) + Mathf.Abs(pc.cellY - gridPos.y) == 1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public IEnumerator RangedAttack() // Snake ranged attack
    {
        GameManager.Instance.textLog.AddMessage("Enemy Snake Shoots!");

        Vector2Int direction = Vector2Int.zero;

        int dx = pc.cellX - gridPos.x;
        int dy = pc.cellY - gridPos.y;

        if (gridPos.y == pc.cellY)
        {
            if (dx > 0)
            {
                direction = Vector2Int.right;
            }
            else
            {
                direction = Vector2Int.left;
            }
        }
        else if (gridPos.x == pc.cellX)
        {
            if (dy > 0)
            {
                direction = Vector2Int.up;
            }
            else
            {
                direction = Vector2Int.down;
            }
        }

        Vector2Int position = gridPos;

        for (int i = 1; i <= rangedAttackRange; i++)
        {
            position += direction;

            if (!Dungeon.IsInsideGrid(position))
            {
                break;
            }

            SpawnShootFX(position);

            var cell = Dungeon.Grid[position.x, position.y];

            if (position.x == pc.cellX && position.y == pc.cellY)
            {
                int damage = DamageCalculator.CalculateDamage(
                    enemyStats.level,
                    enemyStats.attack,
                    rangedPower,
                    ps.defence
                );

                ps.ApplyDamage(damage);
                break;
            }

            if (cell.cellType == CellType.Wall)
            {
                break;
            }

            yield return new WaitForSeconds(0.15f);
        }

        yield return new WaitForSeconds(0.4f);
    }
    void SpawnShootFX(Vector2Int pos) // Spawns projectiles
    {
        Vector3 worldPos = new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0f);

        GameObject fx = Instantiate( pc.snakeHitFX, worldPos, Quaternion.identity);

        Destroy(fx, 0.15f);
    }
    public bool CanShoot(int range) // Checks if the player is in range and in line
    {
        int dx = pc.cellX - gridPos.x;
        int dy = pc.cellY - gridPos.y;

        if (gridPos.y == pc.cellY)  // same row
        {
            int dist = Mathf.Abs(dx);

            if (dist > range || dist == 0)
                return false;

            int stepX;

            if (dx > 0)
            {
                stepX = 1;
            }
            else
            {
                stepX = -1;
            }

            for (int x = gridPos.x + stepX; x != pc.cellX; x += stepX)
            {
                if (!Dungeon.IsValidMove(new Vector2Int(x, gridPos.y)))
                    return false;
            }

            return true;
        }

        if (gridPos.x == pc.cellX)   // same column
        {
            int dist = Mathf.Abs(dy);

            if (dist > range || dist == 0)
                return false;

            int stepY;

            if (dy > 0)
            {
                stepY = 1;
            }
            else
            {
                stepY = -1;
            }

            for (int y = gridPos.y + stepY; y != pc.cellY; y += stepY)
            {
                if (!Dungeon.IsValidMove(new Vector2Int(gridPos.x, y)))
                    return false;
            }

            return true;
        }

        return false;
    }

    public int GetDistanceFromPlayer() // Returns the distance from enemy to player
    {
         int distToPlayer = Mathf.Abs(pc.cellX - gridPos.x) + Mathf.Abs(pc.cellY - gridPos.y);

         return distToPlayer;
    }
    #endregion 
    #region Enemy Movement
    void MoveTo(Vector2Int newPos) // Method that moves the enemy 1 tile
    {
        if (newPos == gridPos) { return; }

        if (!Dungeon.IsValidMove(newPos)) { return; }

        if (Dungeon.Grid[newPos.x, newPos.y].cellType == CellType.Player || Dungeon.Grid[newPos.x, newPos.y].cellType == CellType.Enemy || Dungeon.Grid[newPos.x, newPos.y].cellType == CellType.npc)
        {
            return;   // block movement if occupied by an enemy or player
        }

        var cell = Dungeon.Grid[newPos.x, newPos.y];

        if (cell.itemOnCell != null && cell.itemOnCell.TryGetComponent<Item>(out var item))
        {
            Debug.Log("Destroy");
            collectedItem = cell.itemOnCell;
            collectedItem.gameObject.SetActive(false);
        }

        Dungeon.Grid[gridPos.x, gridPos.y].cellType = CellType.Floor;  // clear old tile
        Dungeon.Grid[gridPos.x, gridPos.y].itemOnCell = null;
        Dungeon.Grid[gridPos.x, gridPos.y].enemyOnCell = null;

        Dungeon.Grid[newPos.x, newPos.y].cellType = CellType.Enemy;   // set new tile
        Dungeon.Grid[newPos.x, newPos.y].enemyOnCell = this.gameObject;
       
        gridPos = newPos;  // update position
        transform.position = new Vector3(gridPos.x + 0.5f, gridPos.y + 0.5f, 0f);

    }
    public IEnumerator MoveTowardsPlayer() // Enemy moves toward player
    {
        Vector2Int start = gridPos;
        //  Vector2Int goal = GetBestAdjacentToPlayer();
        Vector2Int goal = new Vector2Int(pc.cellX, pc.cellY);

        List<Vector2Int> path = Pathfinder.FindPath(start, goal);

        if (path == null || path.Count < 2)
        {
            yield return RandomMove();
            yield break;
        }

        MoveTo(path[1]);

        yield return null;
    }
    public IEnumerator RandomMove() // Enemy moves randomly if nothing nearby
    {
        Vector2Int[] directions =
        {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

        for (int i = 0; i < directions.Length; i++) // Shuffle Directions
        {
            Vector2Int temp = directions[i];
            int r = Random.Range(i, directions.Length);
            directions[i] = directions[r];
            directions[r] = temp;
        }

        foreach (var dir in directions)
        {
            Vector2Int newPos = gridPos + dir;

            if (!Dungeon.IsValidMove(newPos)) { continue; }

            MoveTo(newPos);
            break;
        }

        yield return null;
    }
    public IEnumerator MoveTowardsItem() // Methood that moves the enemy towards the nearest item
    {
        Vector2Int start = gridPos;
        Vector2Int target = FindNearestItem();

        List<Vector2Int> path = Pathfinder.FindPath(start, target);

        if (path == null || path.Count < 2)
        {
            if (GetDistanceFromPlayer() > sightRange)
            {
                yield return RandomMove();
            }

            yield break;
        }

        MoveTo(path[1]);

        yield return null;
    }
    #endregion
    #region Item Finding
    public Vector2Int FindNearestItem() // Finds the nearest item for the enemy to pick up
    {
        Vector2Int start = gridPos;

        int bestSteps = int.MaxValue;
        Vector2Int bestItem = new Vector2Int(0, 0);

        var items = FindItemsNearby();

        foreach (var item in items)
        {
            var path = Pathfinder.FindPath(start, new Vector2Int(item.x, item.y));

            if (path == null || path.Count < 2) { continue; }

            int steps = path.Count - 1;

            if (steps > 10) { continue; }

            if (steps < bestSteps)
            {
                bestSteps = steps;
                bestItem = new Vector2Int(item.x, item.y);
            }
        }

        return bestItem;
    }
    List<Vector2Int> FindItemsNearby() // Searches the grid around the enemy to find the nearest item
    {
        List<Vector2Int> items = new List<Vector2Int>();

        for (int x = gridPos.x - 10; x <= gridPos.x + 10; x++)
        {
            for (int y = gridPos.y - 10; y <= gridPos.y + 10; y++)
            {
                // bounds check
                if (x < 0 || y < 0 ||
                    x >= Dungeon.Grid.GetLength(0) ||
                    y >= Dungeon.Grid.GetLength(1))
                    continue;

                var cell = Dungeon.Grid[x, y];

                if (cell.itemOnCell != null &&
                    cell.itemOnCell.GetComponent<Item>() != null)
                {
                    items.Add(new Vector2Int(x, y));
                }
            }
        }

        return items;
    }
    #endregion
    #region Attack
    public IEnumerator Attack()
    {
        Vector3 originalPos = transform.position;
        Vector3 targetPos = new Vector3(pc.cellX + 0.5f, pc.cellY + 0.5f, 0f);

        transform.position = Vector3.Lerp(originalPos, targetPos, 0.3f);  // move halfway toward player

        yield return new WaitForSeconds(0.05f);

        int damage = DamageCalculator.CalculateDamage(enemyStats.level, enemyStats.attack, meleePower, ps.level); // Use the formula to calculate damage

        ps.ApplyDamage(damage); // apply damage to the player

        GameManager.Instance.textLog.AddMessage("Player took " + damage + " damage!");

        yield return new WaitForSeconds(0.5f);

        transform.position = originalPos; // move back

        yield return new WaitForSeconds(0.05f);
    }
    #endregion
}
