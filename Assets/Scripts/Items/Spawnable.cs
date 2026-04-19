using UnityEngine;

public class Spawnable : MonoBehaviour
{
    public int x;
    public int y;
    public CellType CellType;
   [SerializeField] public bool walkable;
}
