using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
public static class RandomWalkGenerator
{
    // runs repeated random walks, restarting each walk from a random existing floor tile
    public static HashSet<Vector2Int> RunRandomWalk(Vector2Int startPosition, int repetitions, int walkLength)
    {
        Vector2Int currentPosition = startPosition;
        HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();

        for (int i = 0; i < repetitions; i++)
        {
            var path = DungeonAlgorithm.WalkGen(currentPosition, walkLength);
            floorPositions.UnionWith(path);
            var orderedFloorPositions = floorPositions.OrderBy(position => position.x).ThenBy(position => position.y).ToList();
            currentPosition = orderedFloorPositions[Random.Range(0, orderedFloorPositions.Count)];
        }

        return floorPositions;
    }
}
