using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public static class TrapGenerator
{
    private static List<Vector2Int> trapPos;
    // randomly selects trap positions for decorations while skipping blocked/protected cells
    public static void GenerateTraps(
        TilemapVisualizer tilemapVisualizer,
        HashSet<Vector2Int> floorPositions,
        bool generateTraps,
        float trapsChance,
        HashSet<Vector2Int> blockedPositions = null,
        IEnumerable<Vector2Int> protectedPositions = null)
    {
        if (generateTraps == false
            || trapsChance <= 0f
            || floorPositions == null
            || floorPositions.Count == 0)
        {
            return;
        }

        HashSet<Vector2Int> blockedPositionSet = blockedPositions ?? new HashSet<Vector2Int>();
        HashSet<Vector2Int> protectedPositionSet = protectedPositions != null
            ? new HashSet<Vector2Int>(protectedPositions)
            : new HashSet<Vector2Int>();
        List<Vector2Int> trapPositions = new List<Vector2Int>();

        foreach (var position in floorPositions.OrderBy(position => position.x).ThenBy(position => position.y))
        {
            if (blockedPositionSet.Contains(position) || protectedPositionSet.Contains(position))
            {
                continue;
            }

            if (Random.value <= trapsChance)
            {
                trapPositions.Add(position);
            }
        }

        trapPos = trapPositions;

        tilemapVisualizer.GenerateTiles(
            TilemapVisualizer.TileType.Trap,
            TilemapVisualizer.BiomeType.None,
            trapPositions);
    }

    public static List<Vector2Int> getTrapPositions()
    {
        return trapPos;
    }
}
