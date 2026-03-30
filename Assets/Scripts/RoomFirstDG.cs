using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoomFirstDG : RandomWalkDungeonGenerator 
{
    [SerializeField]
    private int minRoomWidth = 4, minRoomHeight = 4; // room size
    
    [SerializeField]
    private int dungeonWidth = 20, dungeonHeight = 20; // total dungeon size

    [SerializeField]
    private bool randomWalkRooms = false; // rectangle or random rooms

    [SerializeField]
    [Range(0, 10)]
    private int roomOffset = 1; // distance rooms are from each other

    protected override void RunProceduralGeneration()
    {
        CreateRooms();
    }

    private void CreateRooms()
    {
        var roomsList =  DungeonGeneration.BinarySpacePartitioning(new BoundsInt((Vector3Int)startPosition, 
            new Vector3Int(dungeonWidth, dungeonHeight, 0)), minRoomWidth, minRoomHeight);

        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();

        if (randomWalkRooms)
        {
           floor = CreateRoomsRandomly(roomsList);
        }
        else
        {
            floor = CreateSimpleRooms(roomsList);
        }


        List<Vector2Int> roomCenters = new List<Vector2Int>();
        foreach (var room in roomsList)
        {
            roomCenters.Add((Vector2Int)Vector3Int.RoundToInt(room.center));
        }

        HashSet<Vector2Int> corridors = ConnectRooms(roomCenters);
        floor.UnionWith(corridors);

        tileMapDisplayer.PaintFloorTiles(floor);
        WallGenerator.CreateWalls(floor, tileMapDisplayer);
    }

    private HashSet<Vector2Int> CreateSimpleRooms(List<BoundsInt> roomsList)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        foreach (var room in roomsList)
        {
            for (int col = roomOffset; col < room.size.x - roomOffset; col++)
            {
                for (int row = roomOffset; row < room.size.y - roomOffset; row++)
                {
                    Vector2Int position = (Vector2Int)room.min + new Vector2Int(col, row);
                    floor.Add(position);
                }
            }
        }
        return floor;
    }

    private HashSet<Vector2Int> CreateRoomsRandomly(List<BoundsInt> roomsList)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        for (int i = 0; i < roomsList.Count; i++)
        {
            var roomBounds = roomsList[i];
            var roomCenter = new Vector2Int(Mathf.RoundToInt(roomBounds.center.x), Mathf.RoundToInt(roomBounds.center.y));
            var roomFloor = RunRandomWalk(roomCenter, randomWalkParameters);
            foreach (var position in roomFloor)
            {
                if (position.x >= (roomBounds.xMin + roomOffset) && position.x <= (roomBounds.xMax - roomOffset) && position.y >= (roomBounds.yMin + roomOffset) && position.y <= (roomBounds.yMax - roomOffset))
                {
                    floor.Add(position);
                }
            }
        }
        return floor;
    }

    private HashSet<Vector2Int> ConnectRooms(List<Vector2Int> roomCenters)
    {
        HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();
        var currentRoomCenter = roomCenters[Random.Range(0, roomCenters.Count)];
        roomCenters.Remove(currentRoomCenter);

        while (roomCenters.Count > 0)
        {
            Vector2Int closestPoint = FindClosestPointTo(currentRoomCenter, roomCenters);
            roomCenters.Remove(closestPoint);
            HashSet<Vector2Int> newCorridor = CreateCorridor(currentRoomCenter, closestPoint);
            currentRoomCenter = closestPoint;
            corridors.UnionWith(newCorridor);
        }
        return corridors;
    }

    private HashSet<Vector2Int> CreateCorridor(Vector2Int currentRoomCenter, Vector2Int closestPoint)
    {

        HashSet<Vector2Int> corridor = new HashSet<Vector2Int>();
        var position = currentRoomCenter;
        corridor.Add(position);
        while (position.y != closestPoint.y)
        {
            if (closestPoint.y > position.y)
            {
                position += Vector2Int.up;
            }
            else if (closestPoint.y < position.y)
            {
                position += Vector2Int.down;
            }
            corridor.Add(position);
        }
        while (position.x != closestPoint.x)
        {
            if (closestPoint.x > position.x)
            {
                position += Vector2Int.right;
            }
            else if (closestPoint.x < position.x)
            {
                position += Vector2Int.left;
            }
            corridor.Add(position);
        }
        return corridor;

    }

    private Vector2Int FindClosestPointTo(Vector2Int currentRoomCenter, List<Vector2Int> roomCenters)
    {
        Vector2Int closestPoint = Vector2Int.zero;
        float distance = float.MaxValue;

        foreach (var position in roomCenters)
        {

            float currentDistance = Vector2.Distance (position, currentRoomCenter);
            if( currentDistance < distance)
            {
                distance = currentDistance;
                closestPoint = position;
            }
        }
        return closestPoint;
    }
}
