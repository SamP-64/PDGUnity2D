using UnityEngine;

public static class Dungeon
{
    
    public static Cell[,] Grid; // access any tile through Dungeon.Grid[x, y]
  
    public static void Initialize(int width, int height)  // Optional helper to initialize the grid
    {
        width = width + 5;
        height = height + 5;

        Grid = new Cell[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Grid[x, y] = new Cell();
                Grid[x, y].x = x;
                Grid[x, y].y = y;
            }
        }
    }

    public static void RevealAroundPlayer(int playerX, int playerY)
    {
        int startX = playerX - 5;
        int startY = playerY - 5;

        if (startX < 0) startX = 0;
        if (startY < 0) startY = 0;

        if (startX + 10 > Grid.GetLength(0)) startX = Grid.GetLength(0) - 10;
        if (startY + 10 > Grid.GetLength(1)) startY = Grid.GetLength(1) - 10;

        for (int x = startX; x < startX + 10; x++)
        {
            for (int y = startY; y < startY + 10; y++)
            {
                Grid[x, y].traversed = true;
            }
        }
    }
    public static void ResetGrid(int width, int height)  // Optional helper to initialize the grid
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {

                Grid[x, y].cellType = CellType.Empty;
                Grid[x, y].traversed = false;
                Grid[x, y].itemOnCell  = null;
                Grid[x, y].enemyOnCell = null;
                Grid[x, y].isStairs = false;
            }
        }
    }

    public static void ResetRoomNums(int width, int height)  // Reset the floors room nums
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {

                Grid[x, y].roomNum = 0;
            }
        }
    }

    public static bool IsValidMove(Vector2Int pos)
    {
        if (pos.x < 0 || pos.y < 0)
            return false;

        if (Dungeon.Grid[pos.x, pos.y].cellType == CellType.Floor || Dungeon.Grid[pos.x, pos.y].cellType == CellType.Coin || Dungeon.Grid[pos.x, pos.y].cellType == CellType.Potion || Dungeon.Grid[pos.x, pos.y].cellType == CellType.Stairs)
        {
            return true;
        }

        return false;
       }

    public static bool IsInsideGrid(Vector2Int position)
    {
        return position.x >= 0 && position.y >= 0 &&
               position.x < Grid.GetLength(0) &&
               position.y < Grid.GetLength(1);
    }
}

public enum CellType
{
    Empty,
    Floor,
    Wall,
    Stairs,
    Coin,
    Player,
    Enemy,
    Potion,
    npc
}
public enum EnemyType
{
    Bat,
    Snake
}