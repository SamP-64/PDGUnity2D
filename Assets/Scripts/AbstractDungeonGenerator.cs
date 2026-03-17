using UnityEngine;

public abstract class AbstractDungeonGenerator : MonoBehaviour
{
    [SerializeField]
    protected TileMapDisplayer tileMapDisplayer = null;
    [SerializeField]
    protected Vector2Int startPosition = Vector2Int.zero;

    public void GenerateDungeon()
    {
        tileMapDisplayer.ClearTileMap();
        RunProceduralGeneration();
    }

    protected abstract void RunProceduralGeneration();
}
