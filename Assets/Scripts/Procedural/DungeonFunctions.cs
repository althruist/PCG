using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class DungeonFunctions : MonoBehaviour
{
    [BoxGroup("Parameters")]
    [MinValue(0)]
    public int repetitions = 10, walkLength = 10, corridorLength = 10, corridorCount = 10;

    [SerializeField]
    protected TilemapVisualizer tilemapVisualizer = null;
    [SerializeField]
    protected Vector2Int startPos = Vector2Int.zero;
    [SerializeField]
    public int seed = 0;
    [SerializeField]
    public bool useRandomSeed = false;
    [SerializeField, ReadOnly]
    public int lastUsedSeed = 0;
    [BoxGroup("Floor Biomes")]
    [SerializeField]
    private bool useFloorBiomes = true;
    [BoxGroup("Floor Biomes")]
    [SerializeField, MinValue(1)]
    private int biomeRegionCount = 8;
    [BoxGroup("Floor Biomes")]
    [SerializeField, MinValue(0f)]
    private float biomeDitherWidth = 3f;
    [BoxGroup("Liquids")]
    [SerializeField]
    private bool generateLiquids = true;
    [BoxGroup("Liquids")]
    [SerializeField, MinValue(0)]
    private int ivyWaterPondCount = 2;
    [BoxGroup("Liquids")]
    [SerializeField, MinValue(0)]
    private int obsidianLavaPondCount = 2;
    [BoxGroup("Liquids")]
    [SerializeField, MinValue(1)]
    private int minPondSize = 5;
    [BoxGroup("Liquids")]
    [SerializeField, MinValue(1)]
    private int maxPondSize = 14;
    [BoxGroup("Decorations")]
    [SerializeField]
    private bool generateDecorations = true;
    [BoxGroup("Decorations")]
    [SerializeField, Range(0f, 1f)]
    private float decorationChance = 0.06f;

    private static readonly TilemapVisualizer.BiomeType[] floorBiomes =
    {
        TilemapVisualizer.BiomeType.Obsidian,
        TilemapVisualizer.BiomeType.Slate,
        TilemapVisualizer.BiomeType.Ivy
    };

    private struct BiomeSite
    {
        public Vector2Int Position;
        public TilemapVisualizer.BiomeType Biome;
    }

    [Button("Generate Dungeon", ButtonSizes.Gigantic), GUIColor(0.5f, 0.5f, 1f)]
    public void GenerateDungeon()
    {
        var previousRandomState = Random.state;

        try
        {
            lastUsedSeed = useRandomSeed ? Environment.TickCount : seed;
            Random.InitState(lastUsedSeed);

            tilemapVisualizer.Clear();
            RunDungeonGenerator();
        }
        finally
        {
            Random.state = previousRandomState;
        }
    }

    [Button("Reset Dungeon", ButtonSizes.Large), GUIColor(1f, 0.5f, 0.5f)]
    public void ResetDungeon()
    {
        tilemapVisualizer.Clear();
    }

    protected abstract void RunDungeonGenerator();

    protected HashSet<Vector2Int> RunRandomWalk(Vector2Int pos)
    {
        var currentPos = pos;
        HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
        for (int i = 0; i < repetitions; i++)
        {
            var path = DungeonAlgorithm.WalkGen(currentPos, walkLength);
            floorPositions.UnionWith(path);
            var orderedFloorPositions = floorPositions.OrderBy(position => position.x).ThenBy(position => position.y).ToList();
            currentPos = orderedFloorPositions[UnityEngine.Random.Range(0, orderedFloorPositions.Count)];
        }
        return floorPositions;
    }

    protected Dictionary<Vector2Int, TilemapVisualizer.BiomeType> GenerateFloorTiles(HashSet<Vector2Int> floorPositions)
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

        List<BiomeSite> biomeSites = CreateBiomeSites(floorPositions);

        foreach (var position in floorPositions.OrderBy(position => position.x).ThenBy(position => position.y))
        {
            TilemapVisualizer.BiomeType biome = GetBiomeForPosition(position, biomeSites);
            biomeByPosition[position] = biome;
            positionsByBiome[biome].Add(position);
        }

        foreach (var biome in floorBiomes)
        {
            tilemapVisualizer.GenerateTiles(TilemapVisualizer.TileType.Floor, biome, positionsByBiome[biome]);
        }

        return biomeByPosition;
    }

    private TilemapVisualizer.BiomeType GetBiomeForPosition(Vector2Int position, List<BiomeSite> biomeSites)
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

    protected HashSet<Vector2Int> GenerateLiquidPonds(
        HashSet<Vector2Int> floorPositions,
        Dictionary<Vector2Int, TilemapVisualizer.BiomeType> biomeByPosition,
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
            TilemapVisualizer.BiomeType.Ivy,
            TilemapVisualizer.TileType.Water,
            ivyWaterPondCount,
            floorPositions,
            biomeByPosition,
            occupiedLiquidPositions,
            protectedPositionSet,
            null);

        GeneratePondsForBiome(
            TilemapVisualizer.BiomeType.Obsidian,
            TilemapVisualizer.TileType.Lava,
            obsidianLavaPondCount,
            floorPositions,
            biomeByPosition,
            occupiedLiquidPositions,
            protectedPositionSet,
            lavaBlockedPositions);

        return occupiedLiquidPositions;
    }

    protected void GenerateDecorations(
        HashSet<Vector2Int> floorPositions,
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

    private void GeneratePondsForBiome(
        TilemapVisualizer.BiomeType biome,
        TilemapVisualizer.TileType liquidType,
        int pondCount,
        HashSet<Vector2Int> floorPositions,
        Dictionary<Vector2Int, TilemapVisualizer.BiomeType> biomeByPosition,
        HashSet<Vector2Int> occupiedLiquidPositions,
        HashSet<Vector2Int> protectedPositions,
        HashSet<Vector2Int> blockedPositions)
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
            HashSet<Vector2Int> pondPositions = CreatePond(validPositions);
            if (pondPositions.Count == 0)
            {
                break;
            }

            occupiedLiquidPositions.UnionWith(pondPositions);
            validPositions.ExceptWith(pondPositions);
            tilemapVisualizer.GenerateTiles(liquidType, biome, pondPositions);
        }
    }

    private HashSet<Vector2Int> GetLiquidCandidatePositions(
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

    private HashSet<Vector2Int> CreatePond(HashSet<Vector2Int> validPositions)
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

    private List<BiomeSite> CreateBiomeSites(HashSet<Vector2Int> floorPositions)
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
