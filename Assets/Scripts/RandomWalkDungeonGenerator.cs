using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;

public class RandomWalkDungeonGenerator : AbstractDungeonGenerator
{

    [SerializeField]
    protected RandomWalkData randomWalkParameters;


    protected override void RunProceduralGeneration()
    {
        HashSet<Vector2Int> floorPositions = RunRandomWalk(startPosition, randomWalkParameters);
        tileMapDisplayer.ClearTileMap();
        tileMapDisplayer.PaintFloorTiles(floorPositions);
        WallGenerator.CreateWalls(floorPositions, tileMapDisplayer);

        foreach (var position in floorPositions)
        {
            Debug.Log(position);
        }
    }

    protected HashSet<Vector2Int> RunRandomWalk(Vector2Int position, RandomWalkData parameters)
    {
        var currentPosition = position;

        HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();

        for (int i = 0; i < randomWalkParameters.iterations; i++)
        {
            var path = DungeonGeneration.RandomWalk(currentPosition , randomWalkParameters.walkLength);
            floorPositions.UnionWith(path);

            if(randomWalkParameters.startRandomly)
            {
                currentPosition = floorPositions.ElementAt(Random.Range(0, floorPositions.Count));
            }

        }
        return floorPositions;
    }

}
