using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public static class PondGenerator
{
    // generates all enabled liquid ponds and returns the occupied liquid positions
    public static HashSet<Vector2Int> GenerateLiquidPonds(
        TilemapVisualizer tilemapVisualizer,
        HashSet<Vector2Int> floorPositions,
        Dictionary<Vector2Int, TilemapVisualizer.BiomeType> biomeByPosition,
        bool generateLiquids,
        int ivyWaterPondCount,
        int obsidianLavaPondCount,
        int minPondSize,
        int maxPondSize,
        HashSet<Vector2Int> lavaBlockedPositions = null,
        IEnumerable<Vector2Int> protectedPositions = null)
    {
        HashSet<Vector2Int> occupiedLiquidPositions = new HashSet<Vector2Int>();

        if (generateLiquids == false || floorPositions == null || floorPositions.Count == 0)
        {
            return occupiedLiquidPositions;
        }

        HashSet<Vector2Int> protectedPositionSet = protectedPositions != null
            ? new HashSet<Vector2Int>(protectedPositions)
            : new HashSet<Vector2Int>();

        GeneratePondsForBiome(
            tilemapVisualizer,
            TilemapVisualizer.BiomeType.Ivy,
            TilemapVisualizer.TileType.Water,
            ivyWaterPondCount,
            floorPositions,
            biomeByPosition,
            occupiedLiquidPositions,
            protectedPositionSet,
            null,
            minPondSize,
            maxPondSize);

        GeneratePondsForBiome(
            tilemapVisualizer,
            TilemapVisualizer.BiomeType.Obsidian,
            TilemapVisualizer.TileType.Lava,
            obsidianLavaPondCount,
            floorPositions,
            biomeByPosition,
            occupiedLiquidPositions,
            protectedPositionSet,
            lavaBlockedPositions,
            minPondSize,
            maxPondSize);

        return occupiedLiquidPositions;
    }

    // generates a requested number of ponds for one biome and liquid tile type
    private static void GeneratePondsForBiome(
        TilemapVisualizer tilemapVisualizer,
        TilemapVisualizer.BiomeType biome,
        TilemapVisualizer.TileType liquidType,
        int pondCount,
        HashSet<Vector2Int> floorPositions,
        Dictionary<Vector2Int, TilemapVisualizer.BiomeType> biomeByPosition,
        HashSet<Vector2Int> occupiedLiquidPositions,
        HashSet<Vector2Int> protectedPositions,
        HashSet<Vector2Int> blockedPositions,
        int minPondSize,
        int maxPondSize)
    {
        if (pondCount <= 0)
        {
            return;
        }

        HashSet<Vector2Int> validPositions = GetLiquidCandidatePositions(
            biome,
            floorPositions,
            biomeByPosition,
            occupiedLiquidPositions,
            protectedPositions,
            blockedPositions);

        for (int i = 0; i < pondCount && validPositions.Count >= minPondSize; i++)
        {
            HashSet<Vector2Int> pondPositions = CreatePond(validPositions, minPondSize, maxPondSize);
            if (pondPositions.Count == 0)
            {
                break;
            }

            occupiedLiquidPositions.UnionWith(pondPositions);
            validPositions.ExceptWith(pondPositions);
            tilemapVisualizer.GenerateTiles(liquidType, biome, pondPositions);
        }
    }

    // filters floor positions down to valid cells for the requested biome/liquid pass
    private static HashSet<Vector2Int> GetLiquidCandidatePositions(
        TilemapVisualizer.BiomeType biome,
        HashSet<Vector2Int> floorPositions,
        Dictionary<Vector2Int, TilemapVisualizer.BiomeType> biomeByPosition,
        HashSet<Vector2Int> occupiedLiquidPositions,
        HashSet<Vector2Int> protectedPositions,
        HashSet<Vector2Int> blockedPositions)
    {
        HashSet<Vector2Int> validPositions = new HashSet<Vector2Int>();

        foreach (var position in floorPositions)
        {
            if (biomeByPosition.TryGetValue(position, out var positionBiome) == false
                || positionBiome != biome
                || occupiedLiquidPositions.Contains(position)
                || protectedPositions.Contains(position)
                || (blockedPositions != null && blockedPositions.Contains(position)))
            {
                continue;
            }

            validPositions.Add(position);
        }

        return validPositions;
    }

    // grows one pond from a random valid start position until it reaches the target size
    private static HashSet<Vector2Int> CreatePond(
        HashSet<Vector2Int> validPositions,
        int minPondSize,
        int maxPondSize)
    {
        HashSet<Vector2Int> pondPositions = new HashSet<Vector2Int>();
        List<Vector2Int> orderedValidPositions = validPositions
            .OrderBy(position => position.x)
            .ThenBy(position => position.y)
            .ToList();
        Vector2Int startPosition = orderedValidPositions[Random.Range(0, orderedValidPositions.Count)];
        List<Vector2Int> edgePositions = new List<Vector2Int> { startPosition };
        int targetSize = Random.Range(minPondSize, maxPondSize + 1);

        pondPositions.Add(startPosition);

        while (edgePositions.Count > 0 && pondPositions.Count < targetSize)
        {
            int edgeIndex = Random.Range(0, edgePositions.Count);
            Vector2Int currentPosition = edgePositions[edgeIndex];
            edgePositions.RemoveAt(edgeIndex);

            foreach (var direction in Direction.dirList.OrderBy(_ => Random.value))
            {
                if (pondPositions.Count >= targetSize)
                {
                    break;
                }

                Vector2Int nextPosition = currentPosition + direction;
                if (validPositions.Contains(nextPosition) == false || pondPositions.Add(nextPosition) == false)
                {
                    continue;
                }

                edgePositions.Add(nextPosition);
            }
        }

        if (pondPositions.Count < minPondSize)
        {
            return new HashSet<Vector2Int>();
        }

        return pondPositions;
    }
}
