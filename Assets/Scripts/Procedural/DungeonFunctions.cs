using System;
using System.Collections.Generic;
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
    }

    protected abstract void RunDungeonGenerator();

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
}
