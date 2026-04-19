using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static UnityEditor.Progress;

public class Enemy : MonoBehaviour
{
    public Vector2Int gridPos;
    public GameObject collectedItem;

    PlayerController pc;
    PlayerStats ps;
    TextLog textLog;
    EnemyStats enemyStats;
    

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
    Vector2Int GetBestAdjacentToPlayer()
    {
        Vector2Int playerPos = new Vector2Int(pc.cellX, pc.cellY);

        Vector2Int[] cardinalDirs =
        {
        Vector2Int.up,
        Vector2Int.left,
        Vector2Int.right,
        Vector2Int.down
    };

        Vector2Int[] diagonalDirs =
        {
        new Vector2Int(-1, 1),
        new Vector2Int(1, 1),
        new Vector2Int(-1, -1),
        new Vector2Int(1, -1)
    };

        // -------- FIRST PASS: CARDINAL --------
        Vector2Int best = gridPos;
        int bestDist = int.MaxValue;
        bool found = false;

        foreach (var dir in cardinalDirs)
        {
            Vector2Int test = playerPos + dir;

            if (!Dungeon.IsValidMove(test)) continue;

            int dist = Mathf.Abs(test.x - gridPos.x) + Mathf.Abs(test.y - gridPos.y);

            if (dist < bestDist)
            {
                bestDist = dist;
                best = test;
                found = true;
            }
        }

        if (found)
            return best;

        // -------- SECOND PASS: DIAGONALS --------
        foreach (var dir in diagonalDirs)
        {
            Vector2Int test = playerPos + dir;

            if (!Dungeon.IsValidMove(test)) continue;

            int dist = Mathf.Abs(test.x - gridPos.x) + Mathf.Abs(test.y - gridPos.y);

            if (dist < bestDist)
            {
                bestDist = dist;
                best = test;
            }
        }

        return best;
    }

    public void MoveTowardsPlayer()
    {
        Vector2Int start = gridPos;
       // Vector2Int goal = new Vector2Int(pc.cellX, pc.cellY);
        Vector2Int goal = GetBestAdjacentToPlayer();

        List <Vector2Int> path = Pathfinder.FindPath(start, goal);

        if (path == null || path.Count < 2)
        {
            RandomMove();
            return;
        }

        Debug.Log("moved");
        MoveTo(path[1]);
    }
  


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

        Dungeon.Grid[newPos.x, newPos.y].cellType = CellType.Enemy;   // set new tile
        Dungeon.Grid[newPos.x, newPos.y].itemOnCell = this.gameObject;

        gridPos = newPos;  // update position
        transform.position = new Vector3(gridPos.x + 0.5f, gridPos.y + 0.5f, 0f);

    }

    public int GetDistanceFromPlayer()
    {
         int distToPlayer = Mathf.Abs(pc.cellX - gridPos.x) + Mathf.Abs(pc.cellY - gridPos.y);

         return distToPlayer;
    }

    public int sightRange = 9;
   
    public void RandomMove()
    {
        Vector2Int[] dirs =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        for (int i = 0; i < dirs.Length; i++) // shuffle directions
        {
            Vector2Int temp = dirs[i];
            int r = Random.Range(i, dirs.Length);
            dirs[i] = dirs[r];
            dirs[r] = temp;
        }

        foreach (var dir in dirs)
        {
            Vector2Int newPos = gridPos + dir;

            if (!Dungeon.IsValidMove(newPos))
                continue;

            MoveTo(newPos);
            return;
        }

        // stand still only if completely blocked
    }

    public void MoveTowardsItem()
    {
        Vector2Int start = gridPos;

        List<Vector2Int> path = Pathfinder.FindPath(start, FindNearestItem());

        if (path == null || path.Count < 2)
        {
            RandomMove();
            return;
        }

        Debug.Log("moved");
        MoveTo(path[1]);
    }
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

    public void Attack()
    {
        int damage = DamageCalculator.CalculateDamage(enemyStats.level, enemyStats.attack, 50, ps.level);
        ps.ApplyDamage(damage);
        enemyStats.currentHP = enemyStats.currentHP - damage;
        textLog.AddMessage("Player took " + damage + " damage!");
    }

}
