using System.Collections.Generic;
using UnityEngine;

public class Room
{

    public int num;
    public List<Cell> cells = new List<Cell>();

    public Room(int num)
    {
        this.num = num;
    }

    public Cell GetRandomFloorCell()
    {
        List<Cell> floorCells = new List<Cell>();

        foreach (Cell cell in cells)
        {
            if (cell.cellType == CellType.Floor)
            {
                floorCells.Add(cell);
            }
        }

        if (floorCells.Count == 0)
            return null;

        return floorCells[Random.Range(0, floorCells.Count)];
    }
}
