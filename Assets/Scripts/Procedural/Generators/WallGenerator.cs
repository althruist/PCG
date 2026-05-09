using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public static class WallGenerator
{
    private static readonly List<Vector2Int> allWallDirections = new List<Vector2Int>
    {
        new Vector2Int(0, 1),
        new Vector2Int(1, 1),
        new Vector2Int(1, 0),
        new Vector2Int(1, -1),
        new Vector2Int(0, -1),
        new Vector2Int(-1, -1),
        new Vector2Int(-1, 0),
        new Vector2Int(-1, 1)
    };

    // finds, classifies, and paints every wall position around the floor set
    public static void CreateWalls(HashSet<Vector2Int> floorPos, TilemapVisualizer tilemapVisualizer)
    {
        var wallPositions = FindWallsInDirections(floorPos, allWallDirections);
        var sortedWallPositions = SortWallPositions(floorPos, wallPositions);

        foreach (var wallPosition in sortedWallPositions.OrderBy(entry => entry.Key))
        {
            tilemapVisualizer.GenerateTiles(wallPosition.Key, TilemapVisualizer.BiomeType.None, wallPosition.Value);
        }
    }

    // finds empty neighboring cells that should become walls
    private static HashSet<Vector2Int> FindWallsInDirections(HashSet<Vector2Int> floorPos, List<Vector2Int> dirList)
    {
        HashSet<Vector2Int> wallPositions = new HashSet<Vector2Int>();
        foreach (var pos in floorPos.OrderBy(position => position.x).ThenBy(position => position.y))
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

    // groups wall positions by the wall tile type they should use
    private static Dictionary<TilemapVisualizer.TileType, HashSet<Vector2Int>> SortWallPositions(
     HashSet<Vector2Int> floorPos,
     HashSet<Vector2Int> wallPositions)
    {
        var sortedWallPositions = new Dictionary<TilemapVisualizer.TileType, HashSet<Vector2Int>>();

        foreach (var wallPosition in wallPositions.OrderBy(position => position.x).ThenBy(position => position.y))
        {
            // pass wallPositions into the helper
            var wallType = GetWallType(floorPos, wallPositions, wallPosition);

            if (!sortedWallPositions.ContainsKey(wallType))
            {
                sortedWallPositions[wallType] = new HashSet<Vector2Int>();
            }

            sortedWallPositions[wallType].Add(wallPosition);
        }

        return sortedWallPositions;
    }

    // chooses a wall tile shape based on adjacent floor and wall neighbors
    private static TilemapVisualizer.TileType GetWallType(
        HashSet<Vector2Int> floorPos,
        HashSet<Vector2Int> wallPositions,
        Vector2Int wallPosition)
    {
        bool hasFloorAbove = floorPos.Contains(wallPosition + Vector2Int.up);
        bool hasFloorBelow = floorPos.Contains(wallPosition + Vector2Int.down);
        bool hasFloorLeft = floorPos.Contains(wallPosition + Vector2Int.left);
        bool hasFloorRight = floorPos.Contains(wallPosition + Vector2Int.right);

        bool hasWallAbove = wallPositions.Contains(wallPosition + Vector2Int.up);
        bool hasWallBelow = wallPositions.Contains(wallPosition + Vector2Int.down);
        bool hasWallLeft = wallPositions.Contains(wallPosition + Vector2Int.left);
        bool hasWallRight = wallPositions.Contains(wallPosition + Vector2Int.right);

        if ((hasFloorLeft && hasFloorRight) || (hasFloorAbove && hasFloorBelow))
        {
            return TilemapVisualizer.TileType.WallFull;
        }

        int wallNeighborCount = 0;
        if (hasWallAbove) wallNeighborCount++;
        if (hasWallBelow) wallNeighborCount++;
        if (hasWallLeft) wallNeighborCount++;
        if (hasWallRight) wallNeighborCount++;

        if (wallNeighborCount == 4) return TilemapVisualizer.TileType.WallFull;

        bool hasFloorTopLeft = floorPos.Contains(wallPosition + new Vector2Int(-1, 1));
        bool hasFloorTopRight = floorPos.Contains(wallPosition + new Vector2Int(1, 1));
        bool hasFloorBottomLeft = floorPos.Contains(wallPosition + new Vector2Int(-1, -1));
        bool hasFloorBottomRight = floorPos.Contains(wallPosition + new Vector2Int(1, -1));

        if (hasFloorBelow && hasFloorRight) return TilemapVisualizer.TileType.WallTopLeft;
        if (hasFloorBelow && hasFloorLeft) return TilemapVisualizer.TileType.WallTopRight;
        if (hasFloorAbove && hasFloorRight) return TilemapVisualizer.TileType.WallBottomLeft;
        if (hasFloorAbove && hasFloorLeft) return TilemapVisualizer.TileType.WallBottomRight;

        if (hasFloorBelow) return TilemapVisualizer.TileType.WallTop;
        if (hasFloorAbove) return TilemapVisualizer.TileType.WallBottom;
        if (hasFloorRight) return TilemapVisualizer.TileType.WallLeft;
        if (hasFloorLeft) return TilemapVisualizer.TileType.WallRight;

        if (hasFloorBottomRight) return TilemapVisualizer.TileType.WallInnerTopLeft;
        if (hasFloorBottomLeft) return TilemapVisualizer.TileType.WallInnerTopRight;
        if (hasFloorTopRight) return TilemapVisualizer.TileType.WallInnerBottomLeft;
        if (hasFloorTopLeft) return TilemapVisualizer.TileType.WallInnerBottomRight;

        return TilemapVisualizer.TileType.WallFull;
    }
}
