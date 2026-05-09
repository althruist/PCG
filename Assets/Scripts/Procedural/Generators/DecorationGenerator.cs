using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public static class DecorationGenerator
{
    // randomly selects floor positions for decorations while skipping blocked/protected cells
    public static void GenerateDecorations(
        TilemapVisualizer tilemapVisualizer,
        HashSet<Vector2Int> floorPositions,
        bool generateDecorations,
        float decorationChance,
        HashSet<Vector2Int> blockedPositions = null,
        IEnumerable<Vector2Int> protectedPositions = null)
    {
        if (generateDecorations == false
            || decorationChance <= 0f
            || floorPositions == null
            || floorPositions.Count == 0)
        {
            return;
        }

        HashSet<Vector2Int> blockedPositionSet = blockedPositions ?? new HashSet<Vector2Int>();
        HashSet<Vector2Int> protectedPositionSet = protectedPositions != null
            ? new HashSet<Vector2Int>(protectedPositions)
            : new HashSet<Vector2Int>();
        List<Vector2Int> decorationPositions = new List<Vector2Int>();

        foreach (var position in floorPositions.OrderBy(position => position.x).ThenBy(position => position.y))
        {
            if (blockedPositionSet.Contains(position) || protectedPositionSet.Contains(position))
            {
                continue;
            }

            if (Random.value <= decorationChance)
            {
                decorationPositions.Add(position);
            }
        }

        tilemapVisualizer.GenerateTiles(
            TilemapVisualizer.TileType.Decoration,
            TilemapVisualizer.BiomeType.None,
            decorationPositions);
    }
}
