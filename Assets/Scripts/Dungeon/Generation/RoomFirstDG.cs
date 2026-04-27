using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoomFirstDG : RandomWalkDungeonGenerator
{

    [Header("Dungeon Settings")]
   
    [SerializeField][Range(4, 40)] private int minRoomHeight = 4;
    [SerializeField][Range(4, 40)] private int minRoomWidth = 4;

    [SerializeField][Range(30, 120)] public int dungeonHeight = 30;
    [SerializeField][Range(30, 120)] public int dungeonWidth = 30;

    [SerializeField][Range(1, 5)] public int minRoomConnections = 2; // number of paths from each room (these can overlap currently)

    [SerializeField] private bool randomWalkRooms = false; // for each individual room to be generated randomly or not
    [SerializeField] private bool showStepByStep = false;

    [SerializeField][Range(0, 6)] private int roomOffset = 1; // offset from bsp bounds

    [Header("Spawned Objects")]

    [SerializeField] private Transform spawnedFolder;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject stairs;
    [SerializeField] private GameObject potion;
    [SerializeField] private GameObject coin;
    [SerializeField] private GameObject npc;
    [SerializeField] private GameObject[] enemy;

    #region Dungeon Generation

    private void Start()
    {
        RunProceduralGeneration();
    }
    protected override void RunProceduralGeneration()
    {
        Dungeon.Initialize(dungeonWidth, dungeonHeight);
        tileMapDisplayer.ClearTileMap();
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.EndTurn();
            TurnManager.Instance.ClearEnemies();
        }
        CreateRooms();
    }

    private void CreateRooms()
    {
        var roomsList = DungeonGeneration.BinarySpacePartitioning( 
            new BoundsInt((Vector3Int)startPosition, new Vector3Int(dungeonWidth, dungeonHeight, 0)),
            minRoomWidth,
            minRoomHeight
        );  // Generate initial room layout using Binary Space Partitioning

        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();

        if (randomWalkRooms) // choose between quad or random rooms
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
            roomCenters.Add((Vector2Int)Vector3Int.RoundToInt(room.center));   // Get room centres for corridor generation
        }
        HashSet<Vector2Int> corridors = ConnectRooms(roomCenters);
        floor.UnionWith(corridors); // Connect rooms using corridors


        if (generateRoutine != null)
        {
            StopCoroutine(generateRoutine);
            generateRoutine = null;
        }

        if (showStepByStep)
        {
            
            generateRoutine = StartCoroutine(tileMapDisplayer.PaintFloorTilesStepByStep(floor, this));
        }
        else
        {
            tileMapDisplayer.PaintAllFloorTiles(floor);   // Display final dungeon layout
            WallGenerator.CreateWalls(floor, tileMapDisplayer);
            SpawnSpawnables();
        }

    }
    private Coroutine generateRoutine;
    public List<Room> rooms = new List<Room>();

    private HashSet<Vector2Int> CreateRoomsRandomly(List<BoundsInt> roomsList)   // Random walk room generation
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

            foreach (var position in roomFloor)  // filter tiles to stay inside BSP room bounds
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
        rooms.RemoveAll(r => r.cells.Count == 0); // removes all rooms whith no floor

        return floor;
    }

    private HashSet<Vector2Int> CreateSimpleRooms(List<BoundsInt> roomsList)   // Simple quadrilatteral room shape
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();

        rooms.Clear();

        for (int i = 0; i < roomsList.Count; i++)
        {
            var roomBounds = roomsList[i];

            rooms.Add(new Room(i));

            var roomCenter = new Vector2Int(
                Mathf.RoundToInt(roomBounds.center.x),
                Mathf.RoundToInt(roomBounds.center.y)
            );

            rooms[i].roomCenter = roomCenter;

            for (int col = roomOffset; col < roomBounds.size.x - roomOffset; col++)   // fill area
            {
                for (int row = roomOffset; row < roomBounds.size.y - roomOffset; row++)
                {
                    Vector2Int position = (Vector2Int)roomBounds.min + new Vector2Int(col, row);
                    floor.Add(position);

                    Cell cell = Dungeon.Grid[position.x, position.y];
                    rooms[i].cells.Add(cell);
                    cell.roomNum = i + 1;
                }
            }
        }

        rooms.RemoveAll(r => r.cells.Count == 0);
        return floor;
    }
    private HashSet<Vector2Int> ConnectRooms(List<Vector2Int> roomCenters) // Connect all rooms 
    {
        HashSet<Vector2Int> corridors = new HashSet<Vector2Int>();
        Dictionary<Vector2Int, HashSet<Vector2Int>> connections = new Dictionary<Vector2Int, HashSet<Vector2Int>>(); // Tracks room connections 

        foreach (var room in roomCenters)
        {
            connections[room] = new HashSet<Vector2Int>();
        }

        List<Vector2Int> connectedRooms = new List<Vector2Int>();
        List<Vector2Int> unconnectedRooms = new List<Vector2Int>(roomCenters);

        Vector2Int startRoom = unconnectedRooms[Random.Range(0, unconnectedRooms.Count)];   // Start from a random room
        connectedRooms.Add(startRoom);
        unconnectedRooms.Remove(startRoom);

        while (unconnectedRooms.Count > 0)  // Connect all rooms (ensures full connectivity)
        {
            Vector2Int bestConnectedRoom = Vector2Int.zero;
            Vector2Int bestUnconnectedRoom = Vector2Int.zero;
            float bestDistance = float.MaxValue;

            foreach (var connectedRoom in connectedRooms) // Find closest pair of unconnected rooms
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

            corridors.UnionWith(CreateCorridor(bestConnectedRoom + new Vector2Int(1,0), bestUnconnectedRoom + new Vector2Int(-1, 0)));

            connections[bestConnectedRoom].Add(bestUnconnectedRoom);
            connections[bestUnconnectedRoom].Add(bestConnectedRoom);

            connectedRooms.Add(bestUnconnectedRoom);
            unconnectedRooms.Remove(bestUnconnectedRoom);
        }

        foreach (var roomCenter in roomCenters)  // Add extra connections for multiple paths
        {
            while (connections[roomCenter].Count < minRoomConnections)
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

    private Vector2Int FindClosestRoomNotConnected(Vector2Int currentRoom, List<Vector2Int> roomCenters, HashSet<Vector2Int> alreadyConnected) // Finds the nearest room that is not already connected to the current room
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

   
    private HashSet<Vector2Int> CreateCorridor(Vector2Int currentRoomCenter, Vector2Int closestPoint) // Creates a straight or L shaped corridoor
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
    
   [HideInInspector] public int monsterRoom;
    public void SpawnSpawnables()
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
        Room firstRoom = rooms[0];
        Cell cell = Dungeon.Grid[firstRoom.roomCenter.x, firstRoom.roomCenter.y];
        player.transform.position = new Vector3(cell.x + 0.5f, cell.y + 0.5f, 0f);

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
           if (cell == null) {continue;}
           GameObject potionRef = Instantiate(potion, new Vector3(cell.x + 0.5f, cell.y + 0.5f, 0f), Quaternion.identity, spawnedFolder);
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
                Cell cell = rooms[i].GetRandomFloorCell();
                GameObject npcRef = Instantiate(npc, new Vector3(cell.x + 0.5f, cell.y + 0.5f, 0f), Quaternion.identity, spawnedFolder);
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
            if (cell == null) { continue; }
            GameObject coinRef = Instantiate(coin, new Vector3(cell.x + 0.5f, cell.y + 0.5f, 0f), Quaternion.identity, spawnedFolder);
            coinRef.GetComponent<Spawnable>().x = cell.x;
            coinRef.GetComponent<Spawnable>().y = cell.y;
            Dungeon.Grid[cell.x, cell.y].cellType = CellType.Coin;
            Dungeon.Grid[cell.x, cell.y].itemOnCell = coinRef;
        }
    }
   
    private void SpawnStairs()
    {
        Cell cell = rooms[rooms.Count - 1].GetRandomFloorCell();
        Instantiate(stairs, new Vector3(cell.x + 0.5f, cell.y + 0.5f, 0f), Quaternion.identity, spawnedFolder );
        Dungeon.Grid[cell.x, cell.y].cellType = CellType.Stairs;
        Dungeon.Grid[cell.x, cell.y].isStairs = true;
    }

    private void SpawnEnemies()
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            Cell cell = rooms[i].GetRandomFloorCell();
            int index = Random.Range(0, enemy.Length); 
            GameObject enemyToSpawn = enemy[index];
            SpawnEnemyAtCell(enemyToSpawn, cell);
        }
    }
    public void SpawnMonsterHouse() // Room where 6 extra enemies spawn
    {
        for (int i = 0; i < 6; i++)
        {
            Cell cell = rooms[monsterRoom - 1].GetRandomFloorCell();
            int index = Random.Range(0, enemy.Length); 
            GameObject enemyToSpawn = enemy[index];
            SpawnEnemyAtCell(enemyToSpawn,cell);
        }

        monsterRoom = -1;
    }

    void SpawnEnemyAtCell(GameObject chosenEnemy, Cell cell)
    {
        if (cell == null) { return; }

        GameObject enemyRef = Instantiate(chosenEnemy, new Vector3(cell.x + 0.5f, cell.y + 0.5f, 0f), Quaternion.identity, spawnedFolder);

        Dungeon.Grid[cell.x, cell.y].cellType = CellType.Enemy;
        Dungeon.Grid[cell.x, cell.y].enemyOnCell = enemyRef;

        Enemy enemyScript = enemyRef.GetComponent<Enemy>();
        if (Application.isPlaying && TurnManager.Instance != null)
        {
            TurnManager.Instance.RegisterEnemy(enemyScript);
        }
        enemyScript.SetStartPosition(new Vector2Int(cell.x, cell.y));
    }
    #endregion
}