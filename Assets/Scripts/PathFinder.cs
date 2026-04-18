using System.Collections.Generic;
using UnityEngine;

public static class Pathfinder
{
    static Vector2Int[] dirs =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    class Node
    {
        public Vector2Int pos;
        public int g;
        public int h;
        public int f => g + h;
        public Node parent;
    }

    public static List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal)
    {
        List<Node> open = new List<Node>();
        HashSet<Vector2Int> closed = new HashSet<Vector2Int>();

        Node startNode = new Node
        {
            pos = start,
            g = 0,
            h = Heuristic(start, goal)
        };

        open.Add(startNode);

        while (open.Count > 0)
        {
            Node current = GetLowestF(open);

            if (current.pos == goal)
                return ReconstructPath(current);

            open.Remove(current);
            closed.Add(current.pos);

            foreach (var dir in dirs)
            {
                Vector2Int nextPos = current.pos + dir;

                if (!IsWalkable(nextPos) || closed.Contains(nextPos))
                    continue;

                int newG = current.g + 1;

                Node existing = open.Find(n => n.pos == nextPos);

                if (existing == null)
                {
                    Node node = new Node
                    {
                        pos = nextPos,
                        g = newG,
                        h = Heuristic(nextPos, goal),
                        parent = current
                    };

                    open.Add(node);
                }
                else if (newG < existing.g)
                {
                    existing.g = newG;
                    existing.parent = current;
                }
            }
        }

        return null; // no path found
    }

    static int Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y); // Manhattan distance
    }

    static Node GetLowestF(List<Node> list)
    {
        Node best = list[0];

        for (int i = 1; i < list.Count; i++)
        {
            if (list[i].f < best.f)
                best = list[i];
        }

        return best;
    }

    static List<Vector2Int> ReconstructPath(Node node)
    {
        List<Vector2Int> path = new List<Vector2Int>();

        while (node != null)
        {
            path.Add(node.pos);
            node = node.parent;
        }

        path.Reverse();
        return path;
    }

    static bool IsWalkable(Vector2Int pos)
    {
        if (pos.x < 0 || pos.y < 0)
            return false;

        if (pos.x >= Dungeon.Grid.GetLength(0) ||
            pos.y >= Dungeon.Grid.GetLength(1))
            return false;

        CellType type = Dungeon.Grid[pos.x, pos.y].cellType;

        switch (type)
        {
            case CellType.Floor:
            case CellType.Coin:
            case CellType.Potion:
            case CellType.Player:
            case CellType.Enemy:
                return true;
        }

        return false;
    }
}