using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniMap : MonoBehaviour
{
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private RectTransform minimapParent;
    [SerializeField] private float tileSize = 6f;

    [SerializeField] private RoomFirstDG dg;

    [SerializeField] private Vector2 offset;

    private void Start()
    {
        InitializeMinimap();
    }

    private Image[,] minimapTiles;

    void InitializeMinimap()
    {
        minimapTiles = new Image[dg.dungeonWidth, dg.dungeonHeight];

        for (int x = 0; x < dg.dungeonWidth; x++)
        {
            for (int y = 0; y < dg.dungeonHeight; y++)
            {
                GameObject tile = Instantiate(tilePrefab, minimapParent);
                RectTransform rect = tile.GetComponent<RectTransform>();
                Image image = tile.GetComponent<Image>();

                rect.anchoredPosition = new Vector2(
                    x * tileSize + offset.x,
                    y * tileSize + offset.y
                );

                rect.sizeDelta = new Vector2(tileSize, tileSize);

                minimapTiles[x, y] = image;
            }
        }
    }

    public void DrawMinimap()
    {
        for (int x = 0; x < dg.dungeonWidth ; x++)
        {
            for (int y = 0; y < dg.dungeonHeight; y++)
            {
                Cell cell = Dungeon.Grid[x, y];

                if (!cell.traversed)
                {
                    minimapTiles[x, y].color = Color.clear;
                    continue;
                }

                switch (cell.cellType)
                {
                    case CellType.Wall:
                        minimapTiles[x, y].color = Color.gray;
                        break;
                    case CellType.Player:
                        minimapTiles[x, y].color = Color.white;
                        break;
                    case CellType.Coin:
                        minimapTiles[x, y].color = Color.yellow;
                        break;
                    case CellType.Stairs:
                        minimapTiles[x, y].color = Color.blue;
                        break;
                    case CellType.Potion:
                        minimapTiles[x, y].color = Color.green;
                        break;
                    case CellType.npc:
                        minimapTiles[x, y].color = Color.cyan;
                        break;
                    case CellType.Empty:
                        minimapTiles[x, y].color = Color.clear;
                        break;
                    case CellType.Enemy:
                        minimapTiles[x, y].color = Color.red;
                        break;
                    default:
                        minimapTiles[x, y].color = Color.clear;
                        break;
                }
            }
        }
    }
}

