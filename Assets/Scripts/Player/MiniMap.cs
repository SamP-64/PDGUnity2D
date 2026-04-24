using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniMap : MonoBehaviour
{
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private RectTransform minimapParent;
    [SerializeField] private float tileSize = 6f;

    [SerializeField] private Vector2 offset; // Position offset on screen

    private void Start()
    {
        InitializeMinimap();
    }

    private Image[,] minimapTiles;

    void InitializeMinimap() 
    {
        minimapTiles = new Image[Dungeon.Grid.GetLength(0), Dungeon.Grid.GetLength(1)];

        for (int x = 0; x < Dungeon.Grid.GetLength(0); x++)
        {
            for (int y = 0; y < Dungeon.Grid.GetLength(1); y++)
            {
                GameObject tile = Instantiate(tilePrefab, minimapParent);
                RectTransform rect = tile.GetComponent<RectTransform>();
                Image image = tile.GetComponent<Image>();

                rect.anchoredPosition = new Vector2( x * tileSize + offset.x, y * tileSize + offset.y );  // Position tiles in grid layout

                rect.sizeDelta = new Vector2(tileSize, tileSize);

                minimapTiles[x, y] = image;
            }
        }
    }

    public void DrawMinimap() // Method to Draw Minimap on Screen
    {
        for (int x = 0; x < Dungeon.Grid.GetLength(0) ; x++)
        {
            for (int y = 0; y < Dungeon.Grid.GetLength(1); y++)
            {
                Cell cell = Dungeon.Grid[x, y];
                Image tile = minimapTiles[x, y];

                if (!cell.traversed)    // Hide unexplored tiles
                {
                    tile.color = Color.clear;
                    continue;
                }

                tile.color = GetColor(cell.cellType);  // Assign colour based on cell type
            }
        }
    }

    Color GetColor(CellType type) // Paints the map based on cell type
    {
        switch (type)
        {
            case CellType.Wall: return Color.gray;
            case CellType.Player: return Color.white;
            case CellType.Coin: return Color.yellow;
            case CellType.Stairs: return Color.blue;
            case CellType.Potion: return Color.green;
            case CellType.npc: return Color.cyan;
            case CellType.Enemy: return Color.red;
            case CellType.Empty: return Color.clear;
            default: return Color.clear;
        }
    }
}

