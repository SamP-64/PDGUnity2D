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
            }
        }
    }


}

public enum CellType
{
    Empty,
    Floor,
    Wall,
    Stairs,
    Coin,
    player
}
