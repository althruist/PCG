using System.Collections.Generic;
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

    public static void CreateWalls(HashSet<Vector2Int> floorPos, TilemapVisualizer tilemapVisualizer)
    {
        var wallPositions = FindWallsInDirections(floorPos, allWallDirections);
        var sortedWallPositions = SortWallPositions(floorPos, wallPositions);

        foreach (var wallPosition in sortedWallPositions)
        {
            tilemapVisualizer.GenerateTiles(wallPosition.Key, TilemapVisualizer.BiomeType.None, wallPosition.Value);
        }
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

    private static Dictionary<TilemapVisualizer.TileType, HashSet<Vector2Int>> SortWallPositions(
     HashSet<Vector2Int> floorPos,
     HashSet<Vector2Int> wallPositions)
    {
        var sortedWallPositions = new Dictionary<TilemapVisualizer.TileType, HashSet<Vector2Int>>();

        foreach (var wallPosition in wallPositions)
        {
            // Pass wallPositions into the helper
            var wallType = GetWallType(floorPos, wallPositions, wallPosition);

            if (!sortedWallPositions.ContainsKey(wallType))
            {
                sortedWallPositions[wallType] = new HashSet<Vector2Int>();
            }

            sortedWallPositions[wallType].Add(wallPosition);
        }

        return sortedWallPositions;
    }

    private static TilemapVisualizer.TileType GetWallType(
        HashSet<Vector2Int> floorPos,
        HashSet<Vector2Int> wallPositions,
        Vector2Int wallPosition)
    {
        // 1. Neighbor Detection (Cardinals)
        bool hasFloorAbove = floorPos.Contains(wallPosition + Vector2Int.up);
        bool hasFloorBelow = floorPos.Contains(wallPosition + Vector2Int.down);
        bool hasFloorLeft = floorPos.Contains(wallPosition + Vector2Int.left);
        bool hasFloorRight = floorPos.Contains(wallPosition + Vector2Int.right);

        bool hasWallAbove = wallPositions.Contains(wallPosition + Vector2Int.up);
        bool hasWallBelow = wallPositions.Contains(wallPosition + Vector2Int.down);
        bool hasWallLeft = wallPositions.Contains(wallPosition + Vector2Int.left);
        bool hasWallRight = wallPositions.Contains(wallPosition + Vector2Int.right);

        // 2. CONFLICT RESOLUTION: "Surrounded by Walls" or "Squeezed by Floors"
        // If a wall tile is surrounded by other walls on opposite sides AND floors on other sides,
        // it's a conflict point. Force it to be a Full Wall.
        if ((hasFloorLeft && hasFloorRight) || (hasFloorAbove && hasFloorBelow))
        {
            return TilemapVisualizer.TileType.WallFull;
        }

        // Check if it's completely surrounded by other wall tiles (no floor touching cardinals)
        int wallNeighborCount = 0;
        if (hasWallAbove) wallNeighborCount++;
        if (hasWallBelow) wallNeighborCount++;
        if (hasWallLeft) wallNeighborCount++;
        if (hasWallRight) wallNeighborCount++;

        // If it's a "center" piece of a large wall mass
        if (wallNeighborCount == 4) return TilemapVisualizer.TileType.WallFull;

        // 3. DIAGONALS (For Inner Corners)
        bool hasFloorTopLeft = floorPos.Contains(wallPosition + new Vector2Int(-1, 1));
        bool hasFloorTopRight = floorPos.Contains(wallPosition + new Vector2Int(1, 1));
        bool hasFloorBottomLeft = floorPos.Contains(wallPosition + new Vector2Int(-1, -1));
        bool hasFloorBottomRight = floorPos.Contains(wallPosition + new Vector2Int(1, -1));

        // 4. OUTER CORNERS (Convex)
        if (hasFloorBelow && hasFloorRight) return TilemapVisualizer.TileType.WallTopLeft;
        if (hasFloorBelow && hasFloorLeft) return TilemapVisualizer.TileType.WallTopRight;
        if (hasFloorAbove && hasFloorRight) return TilemapVisualizer.TileType.WallBottomLeft;
        if (hasFloorAbove && hasFloorLeft) return TilemapVisualizer.TileType.WallBottomRight;

        // 5. STRAIGHT EDGES
        if (hasFloorBelow) return TilemapVisualizer.TileType.WallTop;
        if (hasFloorAbove) return TilemapVisualizer.TileType.WallBottom;
        if (hasFloorRight) return TilemapVisualizer.TileType.WallLeft;
        if (hasFloorLeft) return TilemapVisualizer.TileType.WallRight;

        // 6. INNER CORNERS (Concave)
        if (hasFloorBottomRight) return TilemapVisualizer.TileType.WallInnerTopLeft;
        if (hasFloorBottomLeft) return TilemapVisualizer.TileType.WallInnerTopRight;
        if (hasFloorTopRight) return TilemapVisualizer.TileType.WallInnerBottomLeft;
        if (hasFloorTopLeft) return TilemapVisualizer.TileType.WallInnerBottomRight;

        return TilemapVisualizer.TileType.WallFull;
    }
}
