using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class RoomFirstDG : RandomWalkDungeonGenerator
{
    [SerializeField]
    private int minRoomWidth = 4, minRoomHeight = 4;

    [SerializeField]
    public int dungeonWidth = 20, dungeonHeight = 20;

    [SerializeField]
    private bool randomWalkRooms = false;

    [SerializeField]
    [Range(0, 10)]
    private int roomOffset = 1;

    [SerializeField]
    private GameObject player;

    [SerializeField]
    private GameObject stairs;


    protected override void RunProceduralGeneration()
    {

        Dungeon.Initialize(dungeonHeight , dungeonWidth );
        tileMapDisplayer.ClearTileMap();
        CreateRooms();
    }

    private void CreateRooms()
    {
        var roomsList = DungeonGeneration.BinarySpacePartitioning(
            new BoundsInt((Vector3Int)startPosition, new Vector3Int(dungeonWidth, dungeonHeight, 0)),
            minRoomWidth,
            minRoomHeight
        );

        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();
        HashSet<Vector2Int> coins = new HashSet<Vector2Int>();
        HashSet<Vector2Int> enemyLocations = new HashSet<Vector2Int>();

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
        //    coins.Add(new Vector2Int(Mathf.RoundToInt(room.center.x), Mathf.RoundToInt(room.center.y)));

          }

        foreach (Room room in rooms)
        {

        

            if (room.cells.Count == 0)
                continue;
            Debug.Log("Room num " + room.num);
            int index = Random.Range(0, room.cells.Count);
            Cell randomCell = room.cells[index];

            coins.Add(new Vector2Int(Mathf.RoundToInt(randomCell.x), Mathf.RoundToInt(randomCell.y)));
        }

        foreach (Room room in rooms)
        {

            if (room.cells.Count == 0)
                continue;
            Debug.Log("Room num " + room.num);
            int index = Random.Range(0, room.cells.Count);
            Cell randomCell = room.cells[index];

            enemyLocations.Add(new Vector2Int(Mathf.RoundToInt(randomCell.x), Mathf.RoundToInt(randomCell.y)));
        }

        SpawnPlayerInRoom( 0 , roomsList, floor);


        HashSet<Vector2Int> corridors = ConnectRooms(roomCenters);
        floor.UnionWith(corridors);

        tileMapDisplayer.PaintFloorTiles(floor);
        WallGenerator.CreateWalls(floor, tileMapDisplayer);

        tileMapDisplayer.PaintCoinTiles(coins);
        tileMapDisplayer.PaintEnemyTiles(enemyLocations);

        SpawnInRoom(stairs, roomsList.Count - 1, roomsList, floor);


    }
    private void SpawnPlayerInRoom(int roomIndex, List<BoundsInt> roomsList, HashSet<Vector2Int> floor)
    {
        BoundsInt firstRoom = roomsList[0]; // first room
        List<Vector2Int> firstRoomFloors = new List<Vector2Int>();

        foreach (var tile in floor)
        {
            // Check if the tile is within the BSP room bounds
            if (tile.x >= firstRoom.xMin && tile.x < firstRoom.xMax &&
                tile.y >= firstRoom.yMin && tile.y < firstRoom.yMax)
            {
                firstRoomFloors.Add(tile);
            }
        }

        // Pick a random floor tile inside the room
        Vector2Int spawnTile = firstRoomFloors[Random.Range(0, firstRoomFloors.Count)];

        // Set the player's position
        player.transform.position = new Vector3(spawnTile.x + 0.5f, spawnTile.y + 0.5f, 0f);
        Debug.Log("spawned player");
        Debug.Log(spawnTile.y);
        Debug.Log(spawnTile.x );

    }
    private void SpawnInRoom(GameObject obj, int roomIndex, List<BoundsInt> roomsList, HashSet<Vector2Int> floor)
    {


        Debug.Log("!");

        // Make sure the room exists
        if (roomIndex < 0 || roomIndex >= roomsList.Count)
        {
            Debug.Log("Room index out of range!");
            return;
        }

        BoundsInt room = roomsList[roomIndex];

        // Find all actual floor tiles inside this room
        List<Vector2Int> roomFloors = new List<Vector2Int>();
        foreach (var tile in floor)
        {
            if (tile.x >= room.xMin && tile.x < room.xMax &&
                tile.y >= room.yMin && tile.y < room.yMax)
            {
                roomFloors.Add(tile);
            }
        }

        // Make sure there are valid floor tiles
        if (roomFloors.Count == 0)
        {
            Debug.Log("No floor tiles in this room!");
            return;
        }

        // Pick a random floor tile
        Vector2Int spawnTile = roomFloors[Random.Range(0, roomFloors.Count)];

        // Spawn the object at that position
        Instantiate( obj, new Vector3(spawnTile.x + 0.5f, spawnTile.y + 0.5f, 0f), Quaternion.identity);

        Debug.Log("spawned");

        if (obj.GetComponent<Stairs>())
        {
            Debug.Log("Object has Stairs script");
            Dungeon.Grid[spawnTile.x, spawnTile.y].cellType = CellType.Stairs;
        }
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


    public List<Room> rooms = new List<Room>();

    private HashSet<Vector2Int> CreateRoomsRandomly(List<BoundsInt> roomsList)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();

        rooms.Clear();

        for (int i = 0; i < roomsList.Count; i++)
        {
            var roomBounds = roomsList[i];
            var roomCenter = new Vector2Int(Mathf.RoundToInt(roomBounds.center.x), Mathf.RoundToInt(roomBounds.center.y));
            var roomFloor = RunRandomWalk(roomCenter, randomWalkParameters);

            rooms.Add(new Room(i));
            Debug.Log(i);

            foreach (var position in roomFloor)
            {
                if (position.x >= (roomBounds.xMin + roomOffset) &&
                    position.x <= (roomBounds.xMax - roomOffset) &&
                    position.y >= (roomBounds.yMin + roomOffset) &&
                    position.y <= (roomBounds.yMax - roomOffset))
                {


                    rooms[i].cells.Add(Dungeon.Grid[position.x, position.y]);
                    floor.Add(position);
                    Dungeon.Grid[position.x, position.y].roomNum = i + 1;


                    //if(Dungeon.Grid[position.x, position.y].roomNum == 2)
                    //{
                    //    Instantiate(stairs, new Vector3(position.x + 0.5f, position.y + 0.5f, 0f), Quaternion.identity);
                    //}


                }
            }
        }

        return floor;
    }

    private HashSet<Vector2Int> ConnectRooms(List<Vector2Int> roomCenters)
    {
        HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();
        Dictionary<Vector2Int, HashSet<Vector2Int>> connections = new Dictionary<Vector2Int, HashSet<Vector2Int>>();

        foreach (var room in roomCenters)
        {
            connections[room] = new HashSet<Vector2Int>();
        }

        List<Vector2Int> connectedRooms = new List<Vector2Int>();
        List<Vector2Int> unconnectedRooms = new List<Vector2Int>(roomCenters);

        Vector2Int startRoom = unconnectedRooms[Random.Range(0, unconnectedRooms.Count)];
        connectedRooms.Add(startRoom);
        unconnectedRooms.Remove(startRoom);

        while (unconnectedRooms.Count > 0)
        {
            Vector2Int bestConnectedRoom = Vector2Int.zero;
            Vector2Int bestUnconnectedRoom = Vector2Int.zero;
            float bestDistance = float.MaxValue;

            foreach (var connectedRoom in connectedRooms)
            {
                foreach (var unconnectedRoom in unconnectedRooms)
                {
                    float currentDistance = Vector2.Distance(connectedRoom, unconnectedRoom);

                    if (currentDistance < bestDistance)
                    {
                        bestDistance = currentDistance;
                        bestConnectedRoom = connectedRoom;
                        bestUnconnectedRoom = unconnectedRoom;
                    }
                }
            }

            corridors.UnionWith(CreateCorridor(bestConnectedRoom, bestUnconnectedRoom));

            connections[bestConnectedRoom].Add(bestUnconnectedRoom);
            connections[bestUnconnectedRoom].Add(bestConnectedRoom);

            connectedRooms.Add(bestUnconnectedRoom);
            unconnectedRooms.Remove(bestUnconnectedRoom);
        }

        foreach (var roomCenter in roomCenters)
        {
            while (connections[roomCenter].Count < 2)
            {
                Vector2Int closestRoom = FindClosestRoomNotConnected(roomCenter, roomCenters, connections[roomCenter]);

                if (closestRoom == roomCenter)
                {
                    break;
                }

                corridors.UnionWith(CreateCorridor(roomCenter, closestRoom));

                connections[roomCenter].Add(closestRoom);
                connections[closestRoom].Add(roomCenter);
            }
        }

        return corridors;
    }

    private Vector2Int FindClosestRoomNotConnected(Vector2Int currentRoom, List<Vector2Int> roomCenters, HashSet<Vector2Int> alreadyConnected)
    {
        Vector2Int closestRoom = currentRoom;
        float closestDistance = float.MaxValue;

        foreach (var room in roomCenters)
        {
            if (room == currentRoom || alreadyConnected.Contains(room))
            {
                continue;
            }

            float currentDistance = Vector2.Distance(currentRoom, room);

            if (currentDistance < closestDistance)
            {
                closestDistance = currentDistance;
                closestRoom = room;
            }
        }

        return closestRoom;
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
            float currentDistance = Vector2.Distance(position, currentRoomCenter);

            if (currentDistance < distance)
            {
                distance = currentDistance;
                closestPoint = position;
            }
        }

        return closestPoint;
    }
}