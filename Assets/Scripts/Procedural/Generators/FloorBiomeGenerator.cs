using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public static class FloorBiomeGenerator
{
    // the biome rotation used when randomly seeding biome regions
    private static readonly TilemapVisualizer.BiomeType[] floorBiomes =
    {
        TilemapVisualizer.BiomeType.Obsidian,
        TilemapVisualizer.BiomeType.Slate,
        TilemapVisualizer.BiomeType.Ivy
    };

    // a seeded biome point used to choose the nearest biome for nearby floor cells
    private struct BiomeSite
    {
        public Vector2Int Position;
        public TilemapVisualizer.BiomeType Biome;
    }

    // paints floors and returns the biome chosen for each floor position
    public static Dictionary<Vector2Int, TilemapVisualizer.BiomeType> GenerateFloorTiles(
        TilemapVisualizer tilemapVisualizer,
        HashSet<Vector2Int> floorPositions,
        bool useFloorBiomes,
        int biomeRegionCount,
        float biomeDitherWidth)
    {
        Dictionary<Vector2Int, TilemapVisualizer.BiomeType> biomeByPosition = new Dictionary<Vector2Int, TilemapVisualizer.BiomeType>();

        if (floorPositions == null || floorPositions.Count == 0)
        {
            return biomeByPosition;
        }

        if (useFloorBiomes == false)
        {
            foreach (var position in floorPositions)
            {
                biomeByPosition[position] = TilemapVisualizer.BiomeType.Obsidian;
            }

            tilemapVisualizer.GenerateTiles(
                TilemapVisualizer.TileType.Floor,
                TilemapVisualizer.BiomeType.Obsidian,
                floorPositions);
            return biomeByPosition;
        }

        Dictionary<TilemapVisualizer.BiomeType, List<Vector2Int>> positionsByBiome = floorBiomes
            .ToDictionary(biome => biome, _ => new List<Vector2Int>());

        List<BiomeSite> biomeSites = CreateBiomeSites(floorPositions, biomeRegionCount);

        foreach (var position in floorPositions.OrderBy(position => position.x).ThenBy(position => position.y))
        {
            TilemapVisualizer.BiomeType biome = GetBiomeForPosition(position, biomeSites, biomeDitherWidth);
            biomeByPosition[position] = biome;
            positionsByBiome[biome].Add(position);
        }

        foreach (var biome in floorBiomes)
        {
            tilemapVisualizer.GenerateTiles(TilemapVisualizer.TileType.Floor, biome, positionsByBiome[biome]);
        }

        return biomeByPosition;
    }

    // chooses the closest biome site, with dithering near biome borders
    private static TilemapVisualizer.BiomeType GetBiomeForPosition(
        Vector2Int position,
        List<BiomeSite> biomeSites,
        float biomeDitherWidth)
    {
        BiomeSite nearestSite = biomeSites[0];
        BiomeSite secondNearestSite = nearestSite;
        float nearestDistance = float.MaxValue;
        float secondNearestDistance = float.MaxValue;

        foreach (var site in biomeSites)
        {
            float distance = (position - site.Position).sqrMagnitude;

            if (distance < nearestDistance)
            {
                secondNearestDistance = nearestDistance;
                secondNearestSite = nearestSite;
                nearestDistance = distance;
                nearestSite = site;
            }
            else if (distance < secondNearestDistance)
            {
                secondNearestDistance = distance;
                secondNearestSite = site;
            }
        }

        if (biomeDitherWidth <= 0f || secondNearestDistance == float.MaxValue)
        {
            return nearestSite.Biome;
        }

        float boundaryDistance = Mathf.Sqrt(secondNearestDistance) - Mathf.Sqrt(nearestDistance);
        if (boundaryDistance >= biomeDitherWidth)
        {
            return nearestSite.Biome;
        }

        float secondBiomeChance = 1f - (boundaryDistance / biomeDitherWidth);
        return Random.value < secondBiomeChance ? secondNearestSite.Biome : nearestSite.Biome;
    }

    // picks random floor positions as biome seeds and cycles through the available biomes
    private static List<BiomeSite> CreateBiomeSites(HashSet<Vector2Int> floorPositions, int biomeRegionCount)
    {
        int siteCount = Mathf.Clamp(biomeRegionCount, 1, floorPositions.Count);
        List<Vector2Int> sitePositions = floorPositions
            .OrderBy(position => position.x)
            .ThenBy(position => position.y)
            .OrderBy(_ => Random.value)
            .Take(siteCount)
            .ToList();

        List<BiomeSite> biomeSites = new List<BiomeSite>();
        for (int i = 0; i < sitePositions.Count; i++)
        {
            biomeSites.Add(new BiomeSite
            {
                Position = sitePositions[i],
                Biome = floorBiomes[i % floorBiomes.Length]
            });
        }

        return biomeSites;
    }
}
