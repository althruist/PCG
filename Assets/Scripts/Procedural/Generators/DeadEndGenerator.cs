using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class DeadEndGenerator
{
    // returns floor cells with only one neighbor
    public static List<Vector2Int> FindAllDeadEnds(HashSet<Vector2Int> floorPositions)
    {
        List<Vector2Int> deadEnds = new List<Vector2Int>();

        foreach (var position in floorPositions.OrderBy(position => position.x).ThenBy(position => position.y))
        {
            int neighboursCount = 0;
            foreach (var direction in Direction.dirList)
            {
                if (floorPositions.Contains(position + direction))
                {
                    neighboursCount++;
                }
            }

            if (neighboursCount == 1)
            {
                deadEnds.Add(position);
            }
        }

        return deadEnds;
    }
}
