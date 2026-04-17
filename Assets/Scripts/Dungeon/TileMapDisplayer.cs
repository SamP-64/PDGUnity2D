using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileMapDisplayer : MonoBehaviour
{
  [SerializeField] private Tilemap floorTileMap, wallTileMap;

    [SerializeField] private TileBase floorTile;
    [SerializeField] private TileBase WallTile;

    [SerializeField]
    private TileBase wallTop, wallSideRight, wallSideLeft, wallBottom, wallFull,
       wallInnerCornerDownLeft, wallInnerCornerDownRight,
       wallDiagonalCornerDownRight, wallDiagonalCornerDownLeft, wallDiagonalCornerUpRight, wallDiagonalCornerUpLeft;

    [SerializeField] private GameObject coin;
    [SerializeField] private GameObject enemy;

    public void PaintFloorTiles(IEnumerable<Vector2Int> floorPositions)
    {
        PaintFloorTiles(floorPositions, floorTileMap, floorTile);
    }

    public void PaintCoinTiles(IEnumerable<Vector2Int> floorPositions)
    {
        foreach (var position in floorPositions)
        {
            Vector3 pos = new Vector3(position.x + 0.5f, position.y + 0.5f, 0f);
            GameObject coinRef = Instantiate(coin, pos, Quaternion.identity);
            coinRef.GetComponent<Spawnable>().x = position.x;
            coinRef.GetComponent<Spawnable>().y = position.y;
            Dungeon.Grid[position.x, position.y].cellType = CellType.Coin;
            Dungeon.Grid[position.x, position.y].itemOnCell = coinRef;
        }
       
    }

    public void PaintEnemyTiles(IEnumerable<Vector2Int> floorPositions)
    {
        foreach (var position in floorPositions)
        {
            Vector3 pos = new Vector3(position.x + 0.5f, position.y + 0.5f, 0f);
            GameObject enemyRef = Instantiate(enemy, pos, Quaternion.identity);
            Enemy enemyScript = enemyRef.GetComponent<Enemy>();
            enemyScript.SetStartPosition(position);
            // Dungeon.Grid[position.x, position.y].cellType = CellType.Coin;
        }

    }

    private void PaintFloorTiles(IEnumerable<Vector2Int> floorPositions, Tilemap floorTileMap, TileBase floorTile)
    {
        foreach (var position in floorPositions) 
        {
            PaintSingleTile(floorTileMap, floorTile, position);


           // Debug.Log(position.x + " " + position.y);
            Dungeon.Grid[position.x, position.y].cellType = CellType.Floor;
          //  Instantiate(coin, new Vector3(position.x + 0.5f, position.y + 0.5f, 0f), Quaternion.identity);
        } 
    }

    private void PaintSingleTile(Tilemap floorTileMap, TileBase floorTile, Vector2Int position)
    {
        var tilePosition = floorTileMap.WorldToCell((Vector3Int) position);
        floorTileMap.SetTile(tilePosition, floorTile);
    }

    public void ClearTileMap()
    {
        floorTileMap.ClearAllTiles();
        wallTileMap.ClearAllTiles();

        Spawnable[] spawnables = FindObjectsOfType<Spawnable>();

        foreach (Spawnable item in spawnables)
        {
            Destroy(item.gameObject);
        }
    }

    internal void PaintBasicWall(Vector2Int position, string neighboursValue)
    {
        int valueToInt = Convert.ToInt32(neighboursValue, 2); // convert the binary value to int
        TileBase tile = null;

        if(WallTypeFinder.wallTop.Contains(valueToInt ))
        {
            tile = wallTop;
        }
        else if (WallTypeFinder.wallSideRight.Contains(valueToInt))
        {
            tile = wallSideRight;
        }
        else if (WallTypeFinder.wallSideLeft.Contains(valueToInt))
        {
            tile = wallSideLeft;
        }
        else if (WallTypeFinder.wallBottm.Contains(valueToInt))
        {
            tile = wallBottom;
        }
        else if (WallTypeFinder.wallFull.Contains(valueToInt))
        {
            tile = wallFull;
        }

        if (tile!=null)

        PaintSingleTile(wallTileMap, tile , position);
        Dungeon.Grid[position.x, position.y].cellType = CellType.Wall;

    }

    internal void PaintCornerWall(Vector2Int position, string neighboursValue)
    {
        int typeASInt = Convert.ToInt32(neighboursValue, 2);
        TileBase tile = null;

        if (WallTypeFinder.wallInnerCornerDownLeft.Contains(typeASInt))
        {
            tile = wallInnerCornerDownLeft;
        }
        else if (WallTypeFinder.wallInnerCornerDownRight.Contains(typeASInt))
        {
            tile = wallInnerCornerDownRight;
        }
        else if (WallTypeFinder.wallDiagonalCornerDownLeft.Contains(typeASInt))
        {
            tile = wallDiagonalCornerDownLeft;
        }
        else if (WallTypeFinder.wallDiagonalCornerDownRight.Contains(typeASInt))
        {
            tile = wallDiagonalCornerDownRight;
        }
        else if (WallTypeFinder.wallDiagonalCornerUpRight.Contains(typeASInt))
        {
            tile = wallDiagonalCornerUpRight;
        }
        else if (WallTypeFinder.wallDiagonalCornerUpLeft.Contains(typeASInt))
        {
            tile = wallDiagonalCornerUpLeft;
        }
        else if (WallTypeFinder.wallAllDirections.Contains(typeASInt))
        {
            tile = wallFull;
        }
        else if (WallTypeFinder.wallBottomEightDirections.Contains(typeASInt))
        {
            tile = wallBottom;
        }

        if (tile != null)
            PaintSingleTile(wallTileMap, tile, position);
        Dungeon.Grid[position.x, position.y].cellType = CellType.Wall;

    }
}
