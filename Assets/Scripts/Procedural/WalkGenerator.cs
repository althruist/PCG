using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class WalkGenerator : DungeonGenerator
{

    [BoxGroup("Parameters")]
    [MinValue(0)]
    public int repetitions = 10, walkLength = 10, corridorLength = 10, corridorCount = 10;

    protected override void RunDungeonGenerator()
    {
        tilemapVisualizer.Clear();
        HashSet<Vector2Int> floorPositions = RunRandomWalk(startPos);
        var biomeByPosition = GenerateFloorTiles(floorPositions);
        GenerateLiquidPonds(floorPositions, biomeByPosition);
        WallGenerator.CreateWalls(floorPositions, tilemapVisualizer);
    }
    [Button("Reset Dungeon", ButtonSizes.Large), GUIColor(1f, 0.5f, 0.5f)]
    public void ResetDungeon()
    {
        tilemapVisualizer.Clear();
    }

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
}
