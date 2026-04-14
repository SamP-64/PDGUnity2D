using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Vector2Int gridPos;

    public void SetPosition(Vector2Int pos)
    {
        gridPos = pos;
        transform.position = new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0f);
    }

    public void SetStartPosition(Vector2Int pos)
    {
        gridPos = pos;
    }
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

        foreach (var dir in dirs)   // try each direction until valid move found
        {
            Vector2Int newPos = gridPos + dir;

            if (IsValidMove(newPos))
            {
                Dungeon.Grid[newPos.x, newPos.y].cellType = CellType.Enemy;
                Dungeon.Grid[gridPos.x, gridPos.y].cellType = CellType.Floor;
                gridPos = newPos;
                transform.position = new Vector3(gridPos.x + 0.5f, gridPos.y + 0.5f, 0f);
         
               
                return;
            }

        }

        // fallback: stand still ONLY if completely blocked
    }

    bool IsValidMove(Vector2Int pos)
    {
        if (pos.x < 0 || pos.y < 0 )
            return false;

        return Dungeon.Grid[pos.x, pos.y].cellType == CellType.Floor;
    }
}
