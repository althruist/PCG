using System;
using System.Collections.Generic;
using UnityEngine;

public static class WallGenerator
{
    public static void CreateWalls(HashSet<Vector2Int> floorPos, TilemapVisualizer tilemapVisualizer)
    {
        var basicWallPos = FindWallsInDirections(floorPos, Direction.dirList);
        tilemapVisualizer.GenerateTiles(TilemapVisualizer.TileType.Wall, TilemapVisualizer.BiomeType.None, basicWallPos);
    }

    private static HashSet<Vector2Int> FindWallsInDirections(HashSet<Vector2Int> floorPos, List<Vector2Int> dirList)
    {
        HashSet<Vector2Int> wallPositions = new HashSet<Vector2Int>();
        foreach (var pos in floorPos)
        {
            foreach (var dir in dirList)
            {
                var neighbourPos = pos + dir;
                if (floorPos.Contains(neighbourPos) == false)
                {
                    wallPositions.Add(neighbourPos);
                }
            }
        }
        return wallPositions;
    }
}
