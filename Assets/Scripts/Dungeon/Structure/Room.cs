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

}
