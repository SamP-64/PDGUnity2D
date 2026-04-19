using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;
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

    [SerializeField]
    private GameObject potion;

    [SerializeField]
    private GameObject enemy;

    [SerializeField]
    private GameObject coin;

    [SerializeField]
    private GameObject npc;

    #region Dungeon Generation

    private void Start()
    {
        RunProceduralGeneration();
    }
    protected override void RunProceduralGeneration()
    {
        Dungeon.Initialize(dungeonHeight, dungeonWidth);
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


        SpawnSpawnables();

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

            rooms[i].roomCenter = roomCenter;

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

    #endregion

    #region Spawnables
    
    public int monsterRoom;
    private void SpawnSpawnables()
    {
        monsterRoom = Random.Range(1, rooms.Count);
        SpawnNPC();
        SpawnPlayer();
        SpawnStairs();
        SpawnPotions();
        SpawnCoins();
        SpawnEnemies();
      
    }

    private void SpawnPlayer()
    {
        Debug.Log("spawned player");

        Room firstRoom = rooms[0];
        Cell cell = Dungeon.Grid[firstRoom.roomCenter.x, firstRoom.roomCenter.y];
        player.transform.position = new Vector3(cell.x + 0.5f, cell.y + 0.5f, 0f);
        Dungeon.Grid[cell.x, cell.y].cellType = CellType.Player;

        PlayerController playerController = player.GetComponent<PlayerController >();
        playerController.cellX = Mathf.FloorToInt(player.transform.position.x);
        playerController.cellY = Mathf.FloorToInt(player.transform.position.y);

        Dungeon.RevealAroundPlayer(playerController.cellX, playerController.cellY);
    }

    private void SpawnPotions()
    {
        for (int i = 0; i < rooms.Count; i++)
        {
           Cell cell = rooms[i].GetRandomFloorCell();
           GameObject potionRef = Instantiate(potion, new Vector3(cell.x + 0.5f, cell.y + 0.5f, 0f), Quaternion.identity);
           potionRef.GetComponent<Spawnable>().x = cell.x;
           potionRef.GetComponent<Spawnable>().y = cell.y;
           Dungeon.Grid[cell.x, cell.y].cellType = CellType.Potion;
           Dungeon.Grid[cell.x, cell.y].itemOnCell = potionRef;
        }
    }
    private void SpawnNPC()
    {

       int npcRoom = Random.Range(0, rooms.Count);
        for (int i = 0; i < rooms.Count; i++)
        {
            if (i == npcRoom)
            {
                Cell cell = Dungeon.Grid[rooms[i].roomCenter.x, rooms[i].roomCenter.y];
                GameObject npcRef = Instantiate(npc, new Vector3(cell.x + 0.5f, cell.y + 0.5f, 0f), Quaternion.identity);
                npcRef.GetComponent<Spawnable>().x = cell.x;
                npcRef.GetComponent<Spawnable>().y = cell.y;
                Dungeon.Grid[cell.x, cell.y].cellType = CellType.npc;
            }
          
        }
    }

    private void SpawnCoins()
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            Cell cell = rooms[i].GetRandomFloorCell();
            GameObject coinRef = Instantiate(coin, new Vector3(cell.x + 0.5f, cell.y + 0.5f, 0f), Quaternion.identity);
            coinRef.GetComponent<Spawnable>().x = cell.x;
            coinRef.GetComponent<Spawnable>().y = cell.y;
            Dungeon.Grid[cell.x, cell.y].cellType = CellType.Coin;
            Dungeon.Grid[cell.x, cell.y].itemOnCell = coinRef;
        }
    }
    private void SpawnEnemies()
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            Cell cell = rooms[i].GetRandomFloorCell();
            GameObject enemyRef = Instantiate(enemy, new Vector3(cell.x + 0.5f, cell.y + 0.5f, 0f), Quaternion.identity);
            Dungeon.Grid[cell.x, cell.y].cellType = CellType.Enemy;
            Enemy enemyScript = enemyRef.GetComponent<Enemy>();
            enemyScript.SetStartPosition(new Vector2Int (cell.x, cell.y));
        }
    }
    private void SpawnStairs()
    {
        Cell cell = rooms[rooms.Count - 1].GetRandomFloorCell();
        Instantiate(stairs, new Vector3(cell.x + 0.5f, cell.y + 0.5f, 0f), Quaternion.identity);
        Dungeon.Grid[cell.x, cell.y].cellType = CellType.Stairs;
        
    }
    public void SpawnMonsterHouse()
    {
        for (int i = 0; i < 10; i++)
        {
            Cell cell = rooms[monsterRoom - 1].GetRandomFloorCell();
            GameObject enemyRef = Instantiate(enemy, new Vector3(cell.x + 0.5f, cell.y + 0.5f, 0f), Quaternion.identity);
            Dungeon.Grid[cell.x, cell.y].cellType = CellType.Enemy;
            Enemy enemyScript = enemyRef.GetComponent<Enemy>();
            enemyScript.SetStartPosition(new Vector2Int(cell.x, cell.y));
        }

        monsterRoom = -1;  // cannot be spawned until next floor;
    }
    #endregion
}