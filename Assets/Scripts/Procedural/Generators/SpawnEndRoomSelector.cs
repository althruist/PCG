using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class SpawnEndRoomSelector
{
    // picks the room nearest to start as spawn and the farthest reachable room as the end
    public static void SelectSpawnAndEndRooms(
        List<DungeonRoom> rooms,
        HashSet<Vector2Int> floorPositions,
        Vector2Int startPosition,
        out Vector2Int spawnRoomCenter,
        out Vector2Int endRoomCenter)
    {
        if (rooms.Count == 0)
        {
            spawnRoomCenter = startPosition;
            endRoomCenter = startPosition;
            return;
        }

        DungeonRoom spawnRoom = rooms
            .Where(room => room.FloorPositions.Contains(startPosition))
            .OrderBy(room => Vector2Int.Distance(room.Center, startPosition))
            .FirstOrDefault();

        spawnRoom ??= rooms
            .OrderBy(room => Vector2Int.Distance(room.Center, startPosition))
            .First();

        Dictionary<Vector2Int, int> distanceFromSpawn = FindDistancesFrom(spawnRoom.Center, floorPositions);

        DungeonRoom endRoom = rooms
            .Where(room => room != spawnRoom)
            .OrderByDescending(room => GetBestRoomDistance(room, distanceFromSpawn, spawnRoom.Center))
            .FirstOrDefault();

        spawnRoomCenter = spawnRoom.Center;
        endRoomCenter = endRoom?.Center ?? spawnRoom.Center;
    }

    // flood-fills through floor positions to measure path distance from the spawn room
    private static Dictionary<Vector2Int, int> FindDistancesFrom(Vector2Int start, HashSet<Vector2Int> floorPositions)
    {
        Dictionary<Vector2Int, int> distances = new Dictionary<Vector2Int, int>
        {
            [start] = 0
        };
        Queue<Vector2Int> positionsToVisit = new Queue<Vector2Int>();
        positionsToVisit.Enqueue(start);

        while (positionsToVisit.Count > 0)
        {
            Vector2Int current = positionsToVisit.Dequeue();

            foreach (var direction in Direction.dirList)
            {
                Vector2Int next = current + direction;

                if (floorPositions.Contains(next) && distances.ContainsKey(next) == false)
                {
                    distances[next] = distances[current] + 1;
                    positionsToVisit.Enqueue(next);
                }
            }
        }

        return distances;
    }

    // scores a room by its farthest reachable tile, falling back to straight-line distance
    private static int GetBestRoomDistance(DungeonRoom room, Dictionary<Vector2Int, int> distances, Vector2Int spawnCenter)
    {
        int bestDistance = room.FloorPositions
            .Where(distances.ContainsKey)
            .Select(position => distances[position])
            .DefaultIfEmpty(-1)
            .Max();

        if (bestDistance >= 0)
        {
            return bestDistance;
        }

        return Mathf.RoundToInt(Vector2Int.Distance(room.Center, spawnCenter));
    }
}
