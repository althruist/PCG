using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonRoom
{
    public Vector2Int Origin { get; }
    public HashSet<Vector2Int> FloorPositions { get; }
    public Vector2Int Center { get; }

    // creates a room record and picks a center tile closest to the room's average position
    public DungeonRoom(Vector2Int origin, HashSet<Vector2Int> floorPositions)
    {
        Origin = origin;
        FloorPositions = floorPositions;
        Center = FindClosestTileToAverage(floorPositions);
    }

    // finds an existing floor tile nearest to the average of all room floor positions
    private static Vector2Int FindClosestTileToAverage(HashSet<Vector2Int> floorPositions)
    {
        if (floorPositions.Count == 0)
        {
            return Vector2Int.zero;
        }

        float averageX = (float)floorPositions.Average(pos => pos.x);
        float averageY = (float)floorPositions.Average(pos => pos.y);
        Vector2 average = new Vector2(averageX, averageY);

        return floorPositions
            .OrderBy(pos => Vector2.SqrMagnitude((Vector2)pos - average))
            .First();
    }
}
