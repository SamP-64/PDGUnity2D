using System;
using System.Collections.Generic;
using UnityEngine;

public static class WallGenerator 
{
   public static void CreateWalls(HashSet<Vector2Int> floorPositions, TileMapDisplayer tileMapDisplayer)
    {
        var wallPositions = FindWallinDirections(floorPositions, Directions2D.directionsList);

        foreach (var position in wallPositions)
        {
            tileMapDisplayer.PaintBasicWall(position);
        }
    }

    private static HashSet<Vector2Int> FindWallinDirections(HashSet<Vector2Int> floorPositions, List<Vector2Int> directionList)
    {
        HashSet<Vector2Int> wallPositions = new HashSet<Vector2Int>();
        foreach (var position in floorPositions)
        {
            foreach (var direction in directionList)
            {
                var neighbourPosition = position + direction;
                if (floorPositions.Contains(neighbourPosition) == false) 
                {
                    wallPositions.Add(neighbourPosition);
                }
            }
        }

        return wallPositions;
    }
}
