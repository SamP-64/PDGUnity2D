using System;
using System.Collections.Generic;
using UnityEngine;

public static class WallGenerator 
{
   public static void CreateWalls(HashSet<Vector2Int> floorPositions, TileMapDisplayer tileMapDisplayer)
    {
        var wallPositions = FindWallinDirections(floorPositions, Directions2D.directionsList);
        var cornerWallPositions = FindWallinDirections(floorPositions, Directions2D.diagonalDirectionsList);

        CreateBasicWalls(tileMapDisplayer, wallPositions, floorPositions);
        CreateCornerWalls(tileMapDisplayer, cornerWallPositions, floorPositions);
    }

    private static void CreateCornerWalls(TileMapDisplayer tileMapDisplayer, HashSet<Vector2Int> cornerWallPositions, HashSet<Vector2Int> floorPositions)
    {
        foreach (var position in cornerWallPositions) // check all walls
        {
            string neighboursValue = string.Empty;

            foreach (var direction in Directions2D.allDirectionsList)
            {
                var neighbourPosition = position + direction; // check all of the walls diagonal neighbours

                if (floorPositions.Contains(neighbourPosition))
                {
                    neighboursValue += "1"; // add 1 to the binary value if the checked position is a floor
                }
                else
                {
                    neighboursValue += "0"; // add 0 to the binary value if the checked position is not a floor
                }
            }

            tileMapDisplayer.PaintCornerWall(position, neighboursValue);
        }
    }

    private static void CreateBasicWalls(TileMapDisplayer tileMapDisplayer, HashSet<Vector2Int> wallPositions, HashSet<Vector2Int> floorPositions)
    {
        foreach (var position in wallPositions) // check all walls
        {
            string neighboursValue = string.Empty;

            foreach (var direction in Directions2D.directionsList )
            {
                var neighbourPosition = position + direction; // check all of the walls neighbours
             
                if (floorPositions.Contains(neighbourPosition))
                {
                    neighboursValue += "1"; // add 1 to the binary value if the checked position is a floor
                }
                else
                {
                    neighboursValue += "0"; // add 0 to the binary value if the checked position is not a floor
                }
            }

            tileMapDisplayer.PaintBasicWall(position, neighboursValue);
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
