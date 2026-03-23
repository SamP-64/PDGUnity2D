using System;
using System.Collections.Generic;
using UnityEngine;

public class CorridorFirstDG : RandomWalkDungeonGenerator
{
    [SerializeField]
    private int corridorLength = 10;
    [SerializeField]
    private int corridorCount = 0;
    [SerializeField]
    [Range(0.1f,1f)]
    private float roomPercent = 0.8f;
    [SerializeField]
    public RandomWalkData roomGenerationParameters;
    protected override void RunProceduralGeneration()
    {
        CorridorFirstGeneration();
    }

    private void CorridorFirstGeneration()
    {
        HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
        GenerateCorridors(floorPositions);

        tileMapDisplayer.PaintFloorTiles (floorPositions);
        WallGenerator.CreateWalls(floorPositions, tileMapDisplayer);
    }

    private void GenerateCorridors(HashSet<Vector2Int> floorPositions)
    {
        var currentPosition = startPosition;

        for (int i = 0; i < corridorCount; i++) 
        {
            var corridor = DungeonGeneration.CreateCorridor(currentPosition, corridorLength);
            currentPosition = corridor[corridor.Count  - 1];
            floorPositions.UnionWith (corridor);    
        }
    }
}
