using System.Collections.Generic;
using UnityEngine;

public static class CorridorGenerator
{
    // creates a chain of straight corridors and collects floor and potential room positions
    public static void CreateCorridors(
        Vector2Int startPosition,
        int corridorCount,
        int corridorLength,
        HashSet<Vector2Int> floorPositions,
        HashSet<Vector2Int> potentialRoomPositions)
    {
        Vector2Int currentPosition = startPosition;
        potentialRoomPositions.Add(currentPosition);

        for (int i = 0; i < corridorCount; i++)
        {
            var path = DungeonAlgorithm.CorridorGen(currentPosition, corridorLength);
            currentPosition = path[path.Count - 1];
            potentialRoomPositions.Add(currentPosition);
            floorPositions.UnionWith(path);
        }
    }
}
