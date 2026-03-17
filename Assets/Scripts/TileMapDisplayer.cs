using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileMapDisplayer : MonoBehaviour
{
  [SerializeField] private Tilemap floorTileMap;

    [SerializeField] private TileBase floorTile;

    public void PaintFloorTiles(IEnumerable<Vector2Int> floorPositions)
    {
        PaintFloorTiles(floorPositions, floorTileMap, floorTile);
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
    }
}
