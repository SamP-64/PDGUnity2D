using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniMap : MonoBehaviour
{
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private RectTransform minimapParent;
    [SerializeField] private float tileSize = 6f;


    private void Start()
    {
        InitializeMinimap();
    }

    private Image[,] minimapTiles;

    void InitializeMinimap()
    {
        minimapTiles = new Image[40, 40];

        for (int x = 0; x < 40; x++)
        {
            for (int y = 0; y < 40; y++)
            {
                GameObject tile = Instantiate(tilePrefab, minimapParent);
                RectTransform rect = tile.GetComponent<RectTransform>();
                Image image = tile.GetComponent<Image>();

                rect.anchoredPosition = new Vector2(x * tileSize, y * tileSize);
                rect.sizeDelta = new Vector2(tileSize, tileSize);

                minimapTiles[x, y] = image;
            }
        }
    }
    public void DrawMinimap()
    {
        for (int x = 0; x < 40; x++)
        {
            for (int y = 0; y < 40; y++)
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
                        minimapTiles[x, y].color = Color.red;
                        break;
                    case CellType.player:
                        minimapTiles[x, y].color = Color.white;
                        break;
                    case CellType.Coin:
                        minimapTiles[x, y].color = Color.yellow;
                        break;
                    case CellType.Stairs:
                        minimapTiles[x, y].color = Color.blue;
                        break;
                    case CellType.Empty:
                        minimapTiles[x, y].color = Color.clear;
                        break;
                    default:
                        minimapTiles[x, y].color = Color.clear;
                        break;
                }
            }
        }
    }
}

