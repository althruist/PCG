using System.Collections.Generic;
using UnityEngine;

public static class DungeonAlgorithm
{
    private enum splitType
    {
        Horizontal,
        Vertical
    }

    public static HashSet<Vector2Int> WalkGen(Vector2Int startPos, int walkLength)
    {
        HashSet<Vector2Int> path = new HashSet<Vector2Int>();
        path.Add(startPos);
        var previousPos = startPos;

        for (int i = 0; i < walkLength; i++)
        {
            var newPos = previousPos + Direction.GetRandomDirection();
            path.Add(newPos);
            previousPos = newPos;
        }
        return path;
    }

    public static List<Vector2Int> CorridorGen(Vector2Int startPos, int corridorLength)
    {
        List<Vector2Int> corridor = new List<Vector2Int>();
        var dir = Direction.GetRandomDirection();
        var currentPos = startPos;

        corridor.Add(currentPos);

        for (int i = 0; i < corridorLength; i++)
        {
            currentPos += dir;
            corridor.Add(currentPos);
        }
        return corridor;
    }
}

public static class Direction
{
    public static List<Vector2Int> dirList = new List<Vector2Int>
    {
        new Vector2Int(0,1),
        new Vector2Int(1,0),
        new Vector2Int(0,-1),
        new Vector2Int(-1,0)
    };

    public static Vector2Int GetRandomDirection()
    {
        return dirList[Random.Range(0, dirList.Count)];
    }
}