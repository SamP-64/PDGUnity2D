using System;
using System.Collections.Generic;
using System.Linq;
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
  
    protected override void RunProceduralGeneration()
    {
        CorridorFirstGeneration();
    }

    private void CorridorFirstGeneration()
    {
        HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
        HashSet<Vector2Int> potentialRoomPositions = new HashSet<Vector2Int>();

        GenerateCorridors(floorPositions, potentialRoomPositions );

        HashSet<Vector2Int> roomPositions = CreateRooms(potentialRoomPositions);

        List<Vector2Int> deadEnds = FindAllDeadEnds(floorPositions);

        CreateDeadEndRooms(deadEnds, roomPositions);

        floorPositions.UnionWith(roomPositions);

        tileMapDisplayer.PaintFloorTiles (floorPositions);
        WallGenerator.CreateWalls(floorPositions, tileMapDisplayer);
    }

    private void CreateDeadEndRooms(List<Vector2Int> deadEnds, HashSet<Vector2Int> roomFloors)
    {
         foreach (var position in deadEnds)
        {
            if(roomFloors.Contains(position) == false)
            {
                var room = RunRandomWalk(position, randomWalkParameters);
                roomFloors.UnionWith(room);
            }
        }
    }

    private List<Vector2Int> FindAllDeadEnds(HashSet<Vector2Int> floorPositions)
    {
        List<Vector2Int> deadEnds = new List<Vector2Int>();
        
        foreach (var position in floorPositions)
        {
            int neighbourCount = 0;
            foreach(var direction in Directions2D.directionsList)
            {
                if(floorPositions.Contains(position + direction))
                {
                    neighbourCount++;
                }
            }
            if(neighbourCount == 1)
            {
                deadEnds.Add(position);
            }
        }

        return deadEnds;
    }

    private HashSet<Vector2Int> CreateRooms(HashSet<Vector2Int> potentialRoomPositions)
    {
        HashSet<Vector2Int> roomPositions = new HashSet<Vector2Int>();
        int roomCount = Mathf.RoundToInt(potentialRoomPositions.Count * roomPercent);

        List<Vector2Int> roomToCreate = potentialRoomPositions.OrderBy(x => Guid.NewGuid()).Take(roomCount).ToList();

        foreach(var roomPosition in roomToCreate)
        {
            var roomFloor = RunRandomWalk(roomPosition, randomWalkParameters);
            roomPositions.UnionWith(roomFloor); // Avoid Duplicates
        }

        return roomPositions;
    }

    private void GenerateCorridors(HashSet<Vector2Int> floorPositions, HashSet<Vector2Int> roomPositions)
    {
        var currentPosition = startPosition;
        roomPositions.Add(currentPosition);

        for (int i = 0; i < corridorCount; i++) 
        {
            var corridor = DungeonGeneration.CreateCorridor(currentPosition, corridorLength);
            currentPosition = corridor[corridor.Count  - 1];
            roomPositions.Add(currentPosition);
            floorPositions.UnionWith (corridor);
           
        }
    }
}
