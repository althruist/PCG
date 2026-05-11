using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class DungeonGenerator : DungeonFunctions
{
    [SerializeField, Range(0.1f, 1f)]
    private float roomPercent = 0.8f;

    [SerializeField, ReadOnly]
    private Vector2Int spawnRoomCenter;

    [SerializeField, ReadOnly]
    private Vector2Int endRoomCenter;

    public Vector2Int SpawnRoomCenter => spawnRoomCenter;
    public Vector2Int EndRoomCenter => endRoomCenter;

    // start of the whole dungeon generation
    protected override void RunDungeonGenerator()
    {
        DungeonBuilder();
    }

    // builds the main corridor-first dungeon, then layers rooms, biomes, liquids, decorations, and walls
    private void DungeonBuilder()
    {
        HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
        HashSet<Vector2Int> potentialRoomPositions = new HashSet<Vector2Int>();
        tilemapVisualizer.Clear();
        EnemySpawner.Clear();
        CollectablesSpawner.Clear();
        CorridorGenerator.CreateCorridors(startPos, corridorCount, corridorLength, floorPositions, potentialRoomPositions);
        HashSet<Vector2Int> corridorPositions = new HashSet<Vector2Int>(floorPositions);

        List<DungeonRoom> rooms = RoomGenerator.CreateRooms(potentialRoomPositions, roomPercent, repetitions, walkLength);
        HashSet<Vector2Int> roomPos = RoomGenerator.MergeRoomFloors(rooms);

        List<Vector2Int> deadEnds = DeadEndGenerator.FindAllDeadEnds(floorPositions);

        RoomGenerator.CreateRoomsAtDeadEnds(deadEnds, rooms, roomPos, repetitions, walkLength);

        floorPositions.UnionWith(roomPos);
        SpawnEndRoomSelector.SelectSpawnAndEndRooms(
            rooms,
            floorPositions,
            startPos,
            out spawnRoomCenter,
            out endRoomCenter);

        var biomeByPosition = GenerateFloorTiles(floorPositions);
        HashSet<Vector2Int> liquidPositions = GenerateLiquidPonds(
            floorPositions,
            biomeByPosition,
            corridorPositions,
            new[] { spawnRoomCenter, endRoomCenter });
        GenerateDecorations(floorPositions, liquidPositions, new[] { spawnRoomCenter, endRoomCenter });

        HashSet<Vector2Int> blockedPositions = new HashSet<Vector2Int>(corridorPositions);
        blockedPositions.UnionWith(liquidPositions);
        blockedPositions.UnionWith(DecorationGenerator.getDecorationPositions());

        GenerateTraps(floorPositions, blockedPositions, new[] { spawnRoomCenter, endRoomCenter });
        WallGenerator.CreateWalls(floorPositions, tilemapVisualizer);

        tilemapVisualizer.GenerateTiles(TilemapVisualizer.TileType.Spawn, TilemapVisualizer.BiomeType.None, new[] { spawnRoomCenter });
        tilemapVisualizer.GenerateTiles(TilemapVisualizer.TileType.End, TilemapVisualizer.BiomeType.None, new[] { endRoomCenter });

        blockedPositions.UnionWith(TrapGenerator.getTrapPositions());

        SpawnRandomCollectables(floorPositions, collectablesAmount, collectable, blockedPositions, new[] { spawnRoomCenter, endRoomCenter });
        SpawnRandomEnemies(floorPositions);
    }
}
