using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Vector2Int gridPos;
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
        if(Mathf.Abs(pc.cellX - gridPos.x) + Mathf.Abs(pc.cellY - gridPos.y) == 1)
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
        Vector2Int[] dirs =
        {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

        Vector2Int playerPos = new Vector2Int(pc.cellX, pc.cellY);

        foreach (var dir in dirs)
        {
            Vector2Int test = playerPos + dir;

            if (IsValidMove(test))
                return test;
        }

        return playerPos; // fallback
    }
    public void MoveTowardsPlayer()
    {
        Vector2Int start = gridPos;
        Vector2Int goal = new Vector2Int(pc.cellX, pc.cellY);

        List<Vector2Int> path = Pathfinder.FindPath(start, goal);

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
        if (newPos == gridPos)
            return;

        if (!IsValidMove(newPos))
            return;

        // block movement only if occupied by enemy
        if (Dungeon.Grid[newPos.x, newPos.y].itemOnCell != null &&
            Dungeon.Grid[newPos.x, newPos.y].itemOnCell != this.gameObject)
        {
            return;
        }

        // clear old tile
        Dungeon.Grid[gridPos.x, gridPos.y].cellType = CellType.Floor;
        Dungeon.Grid[gridPos.x, gridPos.y].itemOnCell = null;

        // set new tile
        Dungeon.Grid[newPos.x, newPos.y].cellType = CellType.Enemy;
        Dungeon.Grid[newPos.x, newPos.y].itemOnCell = this.gameObject;

        // update position
        gridPos = newPos;
        transform.position = new Vector3(gridPos.x + 0.5f, gridPos.y + 0.5f, 0f);

    }
    int sightRange = 9;
    //public void MoveTowardsPlayer()
    //{

    //    int distToPlayer = Mathf.Abs(pc.cellX - gridPos.x) + Mathf.Abs(pc.cellY - gridPos.y);

    //    if (distToPlayer > sightRange) // Only Follow if close enough
    //    {
    //        RandomMove();
    //        return; 
    //    }
    //    Vector2Int[] dirs =
    //    {
    //    Vector2Int.up,
    //    Vector2Int.down,
    //    Vector2Int.left,
    //    Vector2Int.right
    //};

    //    Vector2Int bestMove = gridPos;
    //    int bestDistance = int.MaxValue;

    //    foreach (var dir in dirs)
    //    {
    //        Vector2Int newPos = gridPos + dir;

    //        if (!IsValidMove(newPos))
    //            continue;

    //        int dist = Mathf.Abs(pc.cellX - newPos.x) +
    //                   Mathf.Abs(pc.cellY - newPos.y);

    //        if (dist < bestDistance)
    //        {
    //            bestDistance = dist;
    //            bestMove = newPos;
    //        }
    //    }

    //    // If a better move was found → move
    //    if (bestMove != gridPos)
    //    {
    //        Dungeon.Grid[bestMove.x, bestMove.y].cellType = CellType.Enemy;
    //        Dungeon.Grid[bestMove.x, bestMove.y].itemOnCell = this.gameObject;

    //        Dungeon.Grid[gridPos.x, gridPos.y].cellType = CellType.Floor;
    //        Dungeon.Grid[gridPos.x, gridPos.y].itemOnCell = null;

    //        gridPos = bestMove;
    //        transform.position = new Vector3(gridPos.x + 0.5f, gridPos.y + 0.5f, 0f);
    //    }
    //}

    bool IsValidMove(Vector2Int pos)
    {
        if (pos.x < 0 || pos.y < 0)
            return false;

        return Dungeon.Grid[pos.x, pos.y].cellType == CellType.Floor;
    }

    void RandomMove()
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

        foreach (var dir in dirs)   // try each direction until valid move found
        {
            Vector2Int newPos = gridPos + dir;

            if (IsValidMove(newPos))
            {
                Dungeon.Grid[newPos.x, newPos.y].cellType = CellType.Enemy;
                Dungeon.Grid[newPos.x, newPos.y].itemOnCell = this.gameObject;
                Dungeon.Grid[gridPos.x, gridPos.y].cellType = CellType.Floor;
                Dungeon.Grid[gridPos.x, gridPos.y].itemOnCell = null;
              gridPos = newPos;
                transform.position = new Vector3(gridPos.x + 0.5f, gridPos.y + 0.5f, 0f);
         
               
                return;
            }

        }

        // fallback: stand still ONLY if completely blocked
    }

  

    public void Attack()
    {
        Debug.Log("Attackinglol");
        int damage = DamageCalculator.CalculateDamage(enemyStats.level, enemyStats.attack, 50, ps.level);
        ps.ApplyDamage(damage);
        enemyStats.currentHP = enemyStats.currentHP - damage;
        textLog.AddMessage("Player took " + damage + " damage!");
    }

}
