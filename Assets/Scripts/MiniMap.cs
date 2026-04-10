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
    }
    public void DrawMinimap()
    {
        foreach (Transform child in minimapParent)
        {
            Destroy(child.gameObject);
        }

        for (int x = 0; x < 40; x++)
        {
            for (int y = 0; y < 40; y++)
            {
                Cell cell = Dungeon.Grid[x, y];

                GameObject tile = Instantiate(tilePrefab, minimapParent);
                RectTransform rect = tile.GetComponent<RectTransform>();
                Image image = tile.GetComponent<Image>();

                rect.anchoredPosition = new Vector2(x * tileSize, y * tileSize);
                rect.sizeDelta = new Vector2(tileSize, tileSize);

                if (cell.traversed == false) { continue; }

                if (cell.cellType == CellType.Wall)
                {
                    image.color = Color.red;
                }
                else if (cell.cellType == CellType.player)
                {
                    image.color = Color.white;
                }
                else
                {
                    image.color = Color.clear;
                }
            }
        }
    }
}

