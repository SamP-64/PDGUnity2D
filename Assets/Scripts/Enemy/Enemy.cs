using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Rendering.Universal;
//using static UnityEditor.Progress;

public class Enemy : MonoBehaviour
{
    public Vector2Int gridPos;
    public GameObject collectedItem;

    public PlayerController pc;
    PlayerStats ps;
    TextLog textLog;
    public EnemyStats enemyStats;
    public int sightRange = 9;
    public EnemyType enemyType;

    public void SetPosition(Vector2Int pos)
    {
        gridPos = pos;
        transform.position = new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0f);
    }

    public void SetStartPosition(Vector2Int pos)
    {
        enemyStats = GetComponent<EnemyStats>();
        gridPos = pos;
        pc = FindObjectOfType<PlayerController>();
        ps = FindObjectOfType<PlayerStats>();
        textLog = FindObjectOfType<TextLog>();
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

    public IEnumerator SpitAttack()
    {
        textLog.AddMessage("Snake spits!");

        Vector2Int dir = Vector2Int.zero;

        int dx = pc.cellX - gridPos.x;
        int dy = pc.cellY - gridPos.y;

        if (gridPos.y == pc.cellY)
        {
            if (dx > 0)
            {
                dir = Vector2Int.right;
            }
            else
            {
                dir = Vector2Int.left;
            }
        }
        else if (gridPos.x == pc.cellX)
        {
            if (dy > 0)
            {
                dir = Vector2Int.up;
            }
            else
            {
                dir = Vector2Int.down;
            }
        }

        Vector2Int pos = gridPos;

        for (int i = 1; i <= 6; i++)
        {
            pos += dir;

            if (pos.x < 0 || pos.y < 0 ||
                pos.x >= Dungeon.Grid.GetLength(0) ||
                pos.y >= Dungeon.Grid.GetLength(1))
            {
                break;
            }

            SpawnSpitFX(pos);

            var cell = Dungeon.Grid[pos.x, pos.y];

            if (pos.x == pc.cellX && pos.y == pc.cellY)
            {
                int damage = DamageCalculator.CalculateDamage(
                    enemyStats.level,
                    enemyStats.attack,
                    30,
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
    void SpawnSpitFX(Vector2Int pos)
    {
        Vector3 worldPos = new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0f);

        GameObject fx = Instantiate( pc.snakeHitFX, worldPos, Quaternion.identity);

        Destroy(fx, 0.15f);
    }
    public bool CanSpit(int range)
    {
        int dx = pc.cellX - gridPos.x;
        int dy = pc.cellY - gridPos.y;

        // same row
        if (gridPos.y == pc.cellY)
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

        // same column
        if (gridPos.x == pc.cellX)
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

    public int GetDistanceFromPlayer()
    {
         int distToPlayer = Mathf.Abs(pc.cellX - gridPos.x) + Mathf.Abs(pc.cellY - gridPos.y);

         return distToPlayer;
    }
    #endregion 
    #region Enemy Movement
    void MoveTo(Vector2Int newPos)
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
    public IEnumerator MoveTowardsPlayer()
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
    public IEnumerator RandomMove()
    {
        Vector2Int[] directions =
        {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

        directions = directions.OrderBy(x => Random.value).ToArray(); // Randomize so direction doesnt have priority if equal

        for (int i = 0; i < directions.Length; i++)
        {
            Vector2Int temp = directions[i];
            int r = Random.Range(i, directions.Length);
            directions[i] = directions[r];
            directions[r] = temp;
        }

        foreach (var dir in directions)
        {
            Vector2Int newPos = gridPos + dir;

            if (!Dungeon.IsValidMove(newPos))
                continue;

            MoveTo(newPos);
            break;
        }

        yield return null;
    }
    public IEnumerator MoveTowardsItem()
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
    public Vector2Int FindNearestItem()
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
    List<Vector2Int> FindItemsNearby()
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
    #region Atatck
    public IEnumerator Attack()
    {

        Vector3 originalPos = transform.position;
        Vector3 targetPos = new Vector3(pc.cellX + 0.5f, pc.cellY + 0.5f, 0f);

        // move halfway toward player
        transform.position = Vector3.Lerp(originalPos, targetPos, 0.3f);

        yield return new WaitForSeconds(0.05f);

        int damage = DamageCalculator.CalculateDamage(enemyStats.level, enemyStats.attack, 50, ps.level);

        ps.ApplyDamage(damage);
        enemyStats.currentHP -= damage;

        textLog.AddMessage("Player took " + damage + " damage!");

        yield return new WaitForSeconds(0.5f);

        // move back
        transform.position = originalPos;

        yield return new WaitForSeconds(0.05f);
    }
    #endregion
}
