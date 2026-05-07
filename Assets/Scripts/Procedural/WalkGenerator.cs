using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class WalkGenerator : DungeonGenerator
{
    [SerializeField]
    protected DungeonSettings parameters;

    protected override void RunDungeonGenerator()
    {
        tilemapVisualizer.Clear();
        HashSet<Vector2Int> floorPositions = RunRandomWalk(parameters, startPos);
        tilemapVisualizer.GenerateTiles(TilemapVisualizer.TileType.Floor, TilemapVisualizer.BiomeType.Obsidian, floorPositions);
        WallGenerator.CreateWalls(floorPositions, tilemapVisualizer);
    }
    [Button("Reset Dungeon", ButtonSizes.Large), GUIColor(1f, 0.5f, 0.5f)]
    public void ResetDungeon()
    {
        tilemapVisualizer.Clear();
    }

    protected HashSet<Vector2Int> RunRandomWalk(DungeonSettings parameters, Vector2Int pos)
    {
        var currentPos = pos;
        HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
        for (int i = 0; i < parameters.repetitions; i++)
        {
            var path = DungeonAlgorithm.WalkGen(currentPos, parameters.walkLength);
            floorPositions.UnionWith(path);
            var orderedFloorPositions = floorPositions.OrderBy(position => position.x).ThenBy(position => position.y).ToList();
            currentPos = orderedFloorPositions[Random.Range(0, orderedFloorPositions.Count)];
        }
        return floorPositions;
    }
}
