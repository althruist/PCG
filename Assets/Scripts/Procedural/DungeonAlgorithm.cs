using System.Collections.Generic;
using UnityEngine;

public static class DungeonAlgorithm
{
    // creates a wandering path by repeatedly stepping in a random cardinal direction
    public static HashSet<Vector2Int> WalkGen(Vector2Int startPos, int walkLength)
    {
        HashSet<Vector2Int> path = new HashSet<Vector2Int>{startPos};
        var previousPos = startPos;

        for (int i = 0; i < walkLength; i++)
        {
            var newPos = previousPos + Direction.GetRandomDirection();
            path.Add(newPos);
            previousPos = newPos;
        }
        return path;
    }

    // creates a straight corridor by choosing one direction and walking that way
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
    // the four directions available to random walks and corridor logic
    public static List<Vector2Int> dirList = new List<Vector2Int>
    {
        new Vector2Int(0,1),
        new Vector2Int(1,0),
        new Vector2Int(0,-1),
        new Vector2Int(-1,0)
    };

    // returns one random direction from dirList
    public static Vector2Int GetRandomDirection()
    {
        return dirList[Random.Range(0, dirList.Count)];
    }
}
