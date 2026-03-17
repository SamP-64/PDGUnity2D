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
    private int iterations = 10;

    [SerializeField]
    public int walkLength = 10;

    [SerializeField]
    public bool startRandomly = true;


    protected override void RunProceduralGeneration()
    {
        HashSet<Vector2Int> floorPositions = RunRandomWalk();
        tileMapDisplayer.ClearTileMap();
        tileMapDisplayer.PaintFloorTiles(floorPositions);


        foreach (var position in floorPositions)
        {
            Debug.Log(position);
        }
    }

    protected HashSet<Vector2Int> RunRandomWalk()
    {
        var currentPosition = startPosition;

        HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();

        for (int i = 0; i < iterations; i++)
        {
            var path = DungeonGeneration.RandomWalk(currentPosition , walkLength);
            floorPositions.UnionWith(path);

            if(startRandomly)
            {
                currentPosition = floorPositions.ElementAt(Random.Range(0, floorPositions.Count));
            }

        }
        return floorPositions;
    }

}
