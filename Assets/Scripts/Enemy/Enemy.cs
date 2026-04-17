using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static UnityEditor.Progress;

public class Enemy : MonoBehaviour
{
    public Vector2Int gridPos;
    PlayerController pc;
    PlayerStats ps;
    TextLog textLog;
    EnemyStats enemyStats;
    public GameObject collectedItem;

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

            if (Dungeon.IsValidMove(test))
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
    public void MoveTowardsCoin()
    {
        Vector2Int start = gridPos;

        List<Vector2Int> path = Pathfinder.FindPath(start, FindNearestCoin());

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

        if (!Dungeon.IsValidMove(newPos))
        {
            return;
        }
         

        // block movement only if occupied by enemy
        if (Dungeon.Grid[newPos.x, newPos.y].cellType == CellType.player || Dungeon.Grid[newPos.x, newPos.y].cellType == CellType.Enemy )
        {
            return;
        }



        var cell = Dungeon.Grid[newPos.x, newPos.y];

        if (cell.itemOnCell != null && cell.itemOnCell.TryGetComponent<Coin>(out var coin))
        {
            Debug.Log("Destroy");
            collectedItem = cell.itemOnCell;
            collectedItem.gameObject.SetActive(false);
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

   public  int GetDistanceFromPlayer()
    {
          int distToPlayer = Mathf.Abs(pc.cellX - gridPos.x) + Mathf.Abs(pc.cellY - gridPos.y);

          return distToPlayer;
    }

    public int sightRange = 9;
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

            if (Dungeon.IsValidMove(newPos))
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


    List<Vector2Int> FindCoinsNearby()
    {
        List<Vector2Int> coins = new List<Vector2Int>();

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
                    cell.itemOnCell.GetComponent<Coin>() != null)
                {
                    coins.Add(new Vector2Int(x, y));
                }
            }
        }

        return coins;
    }

   public  Vector2Int FindNearestCoin()
    {
        Vector2Int start = gridPos;

        int bestSteps = int.MaxValue;
        Vector2Int bestCoin = new Vector2Int(0,0);


        var coins = FindCoinsNearby();
        foreach (var coin in coins)
        {
            var path = Pathfinder.FindPath(start, new Vector2Int(coin.x, coin.y));

            if (path == null || path.Count < 2)
            { 
            continue;
             }


            int steps = path.Count - 1;

            if (steps > 10)
            {
                continue;
            }
               

            if (steps < bestSteps)
            {
                bestSteps = steps;
                bestCoin = new Vector2Int(coin.x, coin.y);
            }
        }

        return bestCoin;
    }
}
