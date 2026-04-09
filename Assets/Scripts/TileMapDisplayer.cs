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


    public void PaintFloorTiles(IEnumerable<Vector2Int> floorPositions)
    {
        PaintFloorTiles(floorPositions, floorTileMap, floorTile);
    }

    public void PaintCoinTiles(IEnumerable<Vector2Int> floorPositions)
    {
        foreach (var position in floorPositions)
        {
            Vector3 pos = new Vector3(position.x, position.y, 0f);
            Instantiate(coin, pos, Quaternion.identity);
        }
       
    }

    private void PaintFloorTiles(IEnumerable<Vector2Int> floorPositions, Tilemap floorTileMap, TileBase floorTile)
    {
        foreach (var position in floorPositions) 
        {
            PaintSingleTile(floorTileMap, floorTile, position);
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

        Coin[] coins = FindObjectsOfType<Coin>();

        foreach (Coin coin in coins)
        {
            Destroy(coin.gameObject);
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
    }
}
