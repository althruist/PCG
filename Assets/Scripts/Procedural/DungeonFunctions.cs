using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class DungeonFunctions : MonoBehaviour
{
    private const int DefaultPlayerHealth = 100;

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
    [BoxGroup("Floor Biomes"), SerializeField]
    private bool useFloorBiomes = true;
    [BoxGroup("Floor Biomes"), SerializeField, MinValue(1)]
    private int biomeRegionCount = 8;
    [BoxGroup("Floor Biomes"), SerializeField, MinValue(0f)]
    private float biomeDitherWidth = 3f;
    [BoxGroup("Liquids"), SerializeField]
    private bool generateLiquids = true;
    [BoxGroup("Liquids"), SerializeField, MinValue(0)]
    private int ivyWaterPondCount = 2;
    [BoxGroup("Liquids"), SerializeField, MinValue(0)]
    private int obsidianLavaPondCount = 2;
    [BoxGroup("Liquids"), SerializeField, MinValue(1)]
    private int minPondSize = 5;
    [BoxGroup("Liquids"), SerializeField, MinValue(1)]
    private int maxPondSize = 14;
    [BoxGroup("Decorations"), SerializeField]
    private bool generateDecorations = true;
    [BoxGroup("Decorations"), SerializeField, Range(0f, 1f)]
    private float decorationChance = 0.06f;
    [BoxGroup("Traps"), SerializeField]
    private bool generateTraps = true;
    [BoxGroup("Traps"), SerializeField, Range(0f, 1f)]
    private float trapsChance = 0.06f;
    [BoxGroup("Enemies"), SerializeField, Range(0f, 1f)]
    private float enemy1SpawnRateChance = 0.05f;
    [BoxGroup("Enemies"), SerializeField, Range(0f, 1f)]
    private float enemy2SpawnRateChance = 0.05f;
    [BoxGroup("Enemies"), SerializeField]
    private GameObject enemy1;
    [BoxGroup("Enemies"), SerializeField]
    private GameObject enemy2;
    [BoxGroup("Enemies"), SerializeField]
    private float spawnRadiusSafeDistance;
    [BoxGroup("Collectables"), SerializeField]
    public int collectablesAmount = 3;
    [BoxGroup("Collectables"), SerializeField]
    public GameObject collectable;

    private bool adaptiveDefaultsCaptured = false;
    private int baseRepetitions;
    private int baseWalkLength;
    private int baseCorridorLength;
    private int baseCorridorCount;
    private int baseIvyWaterPondCount;
    private int baseObsidianLavaPondCount;
    private int baseMinPondSize;
    private int baseMaxPondSize;
    private float baseTrapsChance;
    private float baseEnemy1SpawnRateChance;
    private float baseEnemy2SpawnRateChance;
    private int baseCollectablesAmount;

    protected virtual void Awake()
    {
        CacheAdaptiveDefaults();
    }


    [Button("Generate Dungeon", ButtonSizes.Gigantic), GUIColor(0.5f, 0.5f, 1f)]
    // seeds random state, clears the current map, then runs the generator
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
    // clears every tilemap
    public void ResetDungeon()
    {
        tilemapVisualizer.Clear();
        EnemySpawner.Clear();
        CollectablesSpawner.Clear();
    }

    public void AdaptGenerationToPlayerStats(int playerHealth, int kills, int collectables, float clearTimeSeconds)
    {
        CacheAdaptiveDefaults();

        int difficultyShift = 0;

        if (playerHealth >= DefaultPlayerHealth * 0.75f)
        {
            difficultyShift++;
        }
        else if (playerHealth <= DefaultPlayerHealth * 0.35f)
        {
            difficultyShift--;
        }

        if (kills >= 8)
        {
            difficultyShift++;
        }
        else if (kills <= 2)
        {
            difficultyShift--;
        }

        if (collectables >= 3)
        {
            difficultyShift++;
        }
        else if (collectables == 0)
        {
            difficultyShift--;
        }

        if (clearTimeSeconds <= 45f)
        {
            difficultyShift++;
        }
        else if (clearTimeSeconds >= 120f)
        {
            difficultyShift--;
        }

        repetitions = SmoothAdjustInt(repetitions, baseRepetitions + difficultyShift, 1, 6, 18);
        walkLength = SmoothAdjustInt(walkLength, baseWalkLength + (difficultyShift * 2), 2, 6, 22);
        corridorLength = SmoothAdjustInt(corridorLength, baseCorridorLength + (difficultyShift * 2), 2, 6, 24);
        corridorCount = SmoothAdjustInt(corridorCount, baseCorridorCount + difficultyShift, 1, 5, 18);

        ivyWaterPondCount = SmoothAdjustInt(ivyWaterPondCount, baseIvyWaterPondCount + difficultyShift, 1, 0, 6);
        obsidianLavaPondCount = SmoothAdjustInt(obsidianLavaPondCount, baseObsidianLavaPondCount + difficultyShift, 1, 0, 6);
        minPondSize = SmoothAdjustInt(minPondSize, baseMinPondSize + difficultyShift, 1, 2, 16);
        maxPondSize = SmoothAdjustInt(maxPondSize, baseMaxPondSize + difficultyShift, 1, minPondSize + 1, 24);

        trapsChance = SmoothAdjustFloat(trapsChance, baseTrapsChance + (difficultyShift * 0.02f), 0.01f, 0.01f, 0.2f);
        enemy1SpawnRateChance = SmoothAdjustFloat(enemy1SpawnRateChance, baseEnemy1SpawnRateChance + (difficultyShift * 0.02f), 0.01f, 0.01f, 0.22f);
        enemy2SpawnRateChance = SmoothAdjustFloat(enemy2SpawnRateChance, baseEnemy2SpawnRateChance + (difficultyShift * 0.02f), 0.01f, 0.01f, 0.2f);
        collectablesAmount = SmoothAdjustInt(collectablesAmount, baseCollectablesAmount - difficultyShift, 1, 1, 8);
    }

    protected abstract void RunDungeonGenerator();

    private void CacheAdaptiveDefaults()
    {
        if (adaptiveDefaultsCaptured)
        {
            return;
        }

        adaptiveDefaultsCaptured = true;
        baseRepetitions = repetitions;
        baseWalkLength = walkLength;
        baseCorridorLength = corridorLength;
        baseCorridorCount = corridorCount;
        baseIvyWaterPondCount = ivyWaterPondCount;
        baseObsidianLavaPondCount = obsidianLavaPondCount;
        baseMinPondSize = minPondSize;
        baseMaxPondSize = maxPondSize;
        baseTrapsChance = trapsChance;
        baseEnemy1SpawnRateChance = enemy1SpawnRateChance;
        baseEnemy2SpawnRateChance = enemy2SpawnRateChance;
        baseCollectablesAmount = collectablesAmount;
    }

    private int SmoothAdjustInt(int currentValue, int targetValue, int maxStep, int minValue, int maxValue)
    {
        int clampedTarget = Mathf.Clamp(targetValue, minValue, maxValue);
        float nextValue = Mathf.MoveTowards(currentValue, clampedTarget, maxStep);
        return Mathf.Clamp(Mathf.RoundToInt(nextValue), minValue, maxValue);
    }

    private float SmoothAdjustFloat(float currentValue, float targetValue, float maxStep, float minValue, float maxValue)
    {
        float clampedTarget = Mathf.Clamp(targetValue, minValue, maxValue);
        return Mathf.Clamp(Mathf.MoveTowards(currentValue, clampedTarget, maxStep), minValue, maxValue);
    }

    // runs the random walk generator
    protected HashSet<Vector2Int> RunRandomWalk(Vector2Int pos)
    {
        return RandomWalkGenerator.RunRandomWalk(pos, repetitions, walkLength);
    }

    // paints floor tiles and returns the biome assigned to each floor position
    protected Dictionary<Vector2Int, TilemapVisualizer.BiomeType> GenerateFloorTiles(HashSet<Vector2Int> floorPositions)
    {
        return FloorBiomeGenerator.GenerateFloorTiles(
            tilemapVisualizer,
            floorPositions,
            useFloorBiomes,
            biomeRegionCount,
            biomeDitherWidth);
    }

    // places water and lava ponds, returning every liquid position so later passes can avoid them
    protected HashSet<Vector2Int> GenerateLiquidPonds(
        HashSet<Vector2Int> floorPositions,
        Dictionary<Vector2Int, TilemapVisualizer.BiomeType> biomeByPosition,
        HashSet<Vector2Int> lavaBlockedPositions = null,
        IEnumerable<Vector2Int> protectedPositions = null)
    {
        return PondGenerator.GenerateLiquidPonds(
            tilemapVisualizer,
            floorPositions,
            biomeByPosition,
            generateLiquids,
            ivyWaterPondCount,
            obsidianLavaPondCount,
            minPondSize,
            maxPondSize,
            lavaBlockedPositions,
            protectedPositions);
    }

    // places decoration tiles on valid floor positions
    protected void GenerateDecorations(
        HashSet<Vector2Int> floorPositions,
        HashSet<Vector2Int> blockedPositions = null,
        IEnumerable<Vector2Int> protectedPositions = null)
    {
        DecorationGenerator.GenerateDecorations(
            tilemapVisualizer,
            floorPositions,
            generateDecorations,
            decorationChance,
            blockedPositions,
            protectedPositions);
    }

    protected void GenerateTraps(
    HashSet<Vector2Int> floorPositions,
    HashSet<Vector2Int> blockedPositions = null,
    IEnumerable<Vector2Int> protectedPositions = null)
    {
        TrapGenerator.GenerateTraps(
            tilemapVisualizer,
            floorPositions,
            generateTraps,
            trapsChance,
            blockedPositions,
            protectedPositions);
    }

    protected void SpawnRandomCollectables(HashSet<Vector2Int> floorPositions, int collectablesAmount, GameObject collectable, HashSet<Vector2Int> blockedPositions, IEnumerable<Vector2Int> protectedPositions = null)
    {
        CollectablesSpawner.Spawn(floorPositions, collectablesAmount, collectable, blockedPositions, protectedPositions);
    }

    protected void SpawnRandomEnemies(HashSet<Vector2Int> floorPositions)
    {
        EnemySpawner.Spawn(floorPositions, enemy1SpawnRateChance, enemy2SpawnRateChance, enemy1, enemy2, SpawnEndRoomSelector.getSpawnCenter(), spawnRadiusSafeDistance);
    }
}
