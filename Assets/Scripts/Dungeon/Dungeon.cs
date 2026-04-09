using UnityEngine;

public static class Dungeon
{
    // Static means you can access this from any script: Dungeon.Grid[x, y]
    public static Cell[,] Grid;

    // Optional helper to initialize the grid
    public static void Initialize(int width, int height)
    {
        Grid = new Cell[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Grid[x, y] = new Cell();
            }
        }
    }
}
