using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Tilemaps;
public class TileMapDisplayer : MonoBehaviour
{
    [SerializeField] private Tilemap floorTileMap, wallTileMap;
    [SerializeField] private TileBase floorTile;
    [SerializeField] private TileBase WallTile;

    [SerializeField]
    private TileBase wallTop, wallSideRight, wallSideLeft, wallBottom, wallFull,   // Wall variants based on neighbours
    wallInnerCornerDownLeft, wallInnerCornerDownRight, wallDiagonalCornerDownRight, 
    wallDiagonalCornerDownLeft, wallDiagonalCornerUpRight, wallDiagonalCornerUpLeft;

    #region Paint Tiles
    public void PaintAllFloorTiles(IEnumerable<Vector2Int> floorPositions)
    {
         PaintFloorTiles(floorPositions, floorTileMap, floorTile);
    }

    private void PaintFloorTiles(IEnumerable<Vector2Int> floorPositions, Tilemap floorTileMap, TileBase floorTile) // paints all floors in floorpositions
    {
        foreach (var position in floorPositions)
        {
            PaintSingleTile(floorTileMap, floorTile, position);
            Dungeon.Grid[position.x, position.y].cellType = CellType.Floor;
        }
    }

    public IEnumerator PaintFloorTilesStepByStep(IEnumerable<Vector2Int> floorPositions, RoomFirstDG dg) // shows generation step by step (only from play mode)
    {
        yield return new WaitForSeconds(5f);

        foreach (var position in floorPositions)
        {
            PaintSingleTile(floorTileMap, floorTile, position);
            Dungeon.Grid[position.x, position.y].cellType = CellType.Floor;

            yield return new WaitForSeconds(0.01f); 
        }

        yield return new WaitForSeconds(2f);

        WallGenerator.CreateWalls(new HashSet<Vector2Int>(floorPositions), this);

        yield return new WaitForSeconds(2f);
        dg.SpawnSpawnables();
    }

    private void PaintSingleTile(Tilemap floorTileMap, TileBase floorTile, Vector2Int position) // Paints a custom tile
    {
        var tilePosition = floorTileMap.WorldToCell((Vector3Int) position);
        floorTileMap.SetTile(tilePosition, floorTile);
    }

    #endregion 
    #region Walls
    internal void PaintBasicWall(Vector2Int position, string neighboursValue)
    {
        int valueToInt = Convert.ToInt32(neighboursValue, 2); // convert the binary value to int
        TileBase tile = null;

        if (WallTypeFinder.wallTop.Contains(valueToInt ))
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

        if (tile != null)
        {
            PaintSingleTile(wallTileMap, tile, position);
            Dungeon.Grid[position.x, position.y].cellType = CellType.Wall;
        }

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
        {
            PaintSingleTile(wallTileMap, tile, position);
            Dungeon.Grid[position.x, position.y].cellType = CellType.Wall;
        }
    }
    #endregion
    #region Dungeon Reset
    public void ClearTileMap()
    {
        floorTileMap.ClearAllTiles();
        wallTileMap.ClearAllTiles();
        DestroySpawnables();
    }

    private void DestroySpawnables()
    {
        Spawnable[] spawnables = FindObjectsByType<Spawnable>(FindObjectsSortMode.None);

        foreach (Spawnable item in spawnables)
        {
            if (item == null) continue;

            GameObject obj = item.gameObject; 

            if (Application.isPlaying)
            {
                Destroy(obj);
            }
            else
            {
                DestroyImmediate(obj);
            }
        }
    }
    #endregion
}
