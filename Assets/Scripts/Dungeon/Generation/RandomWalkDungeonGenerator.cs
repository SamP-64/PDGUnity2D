using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomWalkDungeonGenerator : AbstractDungeonGenerator
{

    [SerializeField] protected RandomWalkData randomWalkParameters;

    protected override void RunProceduralGeneration()
    {
        Dungeon.Initialize(Dungeon.Grid.GetLength(0), Dungeon.Grid.GetLength(1));

        HashSet<Vector2Int> floorPositions = RunRandomWalk(startPosition, randomWalkParameters);    // Generate all floor tiles using the random walk algorithm
        tileMapDisplayer.ClearTileMap();
        tileMapDisplayer.PaintFloorTiles(floorPositions);
        WallGenerator.CreateWalls(floorPositions, tileMapDisplayer);   // Automatically place walls around the floor
    }

    protected HashSet<Vector2Int> RunRandomWalk(Vector2Int position, RandomWalkData parameters)
    {
        var currentPosition = position;

        HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();

        for (int i = 0; i < randomWalkParameters.iterations; i++) // Walks randomly for the number of iterations
        {
            var path = DungeonGeneration.RandomWalk(currentPosition , randomWalkParameters.walkLength);    // Generate a random path from the current position
            floorPositions.UnionWith(path);

            if(randomWalkParameters.startRandomly)
            {
                currentPosition = floorPositions.ElementAt(Random.Range(0, floorPositions.Count));
            }

        }
        return floorPositions;
    }

}
