using UnityEngine;

public static class Dungeon
{
    
    public static Cell[,] Grid; // access any tile through Dungeon.Grid[x, y]

    public static void Initialize(int width, int height)  // Optional helper to initialize the grid
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
