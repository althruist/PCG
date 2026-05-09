using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class DungeonGenerator : DungeonFunctions
{
    private class DungeonRoom
    {
        public Vector2Int Origin { get; }
        public HashSet<Vector2Int> FloorPositions { get; }
        public Vector2Int Center { get; }

        public DungeonRoom(Vector2Int origin, HashSet<Vector2Int> floorPositions)
        {
            Origin = origin;
            FloorPositions = floorPositions;
            Center = FindClosestTileToAverage(floorPositions);
        }

        private static Vector2Int FindClosestTileToAverage(HashSet<Vector2Int> floorPositions)
        {
            if (floorPositions.Count == 0)
            {
                return Vector2Int.zero;
            }

            float averageX = (float)floorPositions.Average(pos => pos.x);
            float averageY = (float)floorPositions.Average(pos => pos.y);
            Vector2 average = new Vector2(averageX, averageY);

            return floorPositions
                .OrderBy(pos => Vector2.SqrMagnitude((Vector2)pos - average))
                .First();
        }
    }

    [SerializeField]
    [Range(0.1f, 1f)]
    private float roomPercent = 0.8f;

    [SerializeField, ReadOnly]
    private Vector2Int spawnRoomCenter;

    [SerializeField, ReadOnly]
    private Vector2Int endRoomCenter;

    public Vector2Int SpawnRoomCenter => spawnRoomCenter;
    public Vector2Int EndRoomCenter => endRoomCenter;

    protected override void RunDungeonGenerator()
    {
        tilemapVisualizer.Clear();
        HashSet<Vector2Int> floorPositions = RunRandomWalk(startPos);
        var biomeByPosition = GenerateFloorTiles(floorPositions);
        HashSet<Vector2Int> liquidPositions = GenerateLiquidPonds(floorPositions, biomeByPosition);
        GenerateDecorations(floorPositions, liquidPositions, new[] { startPos });
        WallGenerator.CreateWalls(floorPositions, tilemapVisualizer);

        CorridorFirstDungeonGen();
    }

    private void CorridorFirstDungeonGen()
    {
        HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
        HashSet<Vector2Int> potentialRoomPositions = new HashSet<Vector2Int>();
        tilemapVisualizer.Clear();
        CreateCorridors(floorPositions, potentialRoomPositions);
        HashSet<Vector2Int> corridorPositions = new HashSet<Vector2Int>(floorPositions);

        List<DungeonRoom> rooms = CreateRooms(potentialRoomPositions);
        HashSet<Vector2Int> roomPos = MergeRoomFloors(rooms);

        List<Vector2Int> deadEnds = FindAllDeadEnds(floorPositions);

        CreateRoomsAtDeadEnd(deadEnds, rooms, roomPos);

        floorPositions.UnionWith(roomPos);
        SelectSpawnAndEndRooms(rooms, floorPositions);

        var biomeByPosition = GenerateFloorTiles(floorPositions);
        HashSet<Vector2Int> liquidPositions = GenerateLiquidPonds(
            floorPositions,
            biomeByPosition,
            corridorPositions,
            new[] { spawnRoomCenter, endRoomCenter });
        GenerateDecorations(floorPositions, liquidPositions, new[] { spawnRoomCenter, endRoomCenter });
        WallGenerator.CreateWalls(floorPositions, tilemapVisualizer);
        GenerateSpawnEndTiles();
    }

    private void CreateRoomsAtDeadEnd(List<Vector2Int> deadEnds, List<DungeonRoom> rooms, HashSet<Vector2Int> roomFloors)
    {
        foreach (var position in deadEnds)
        {
            if (roomFloors.Contains(position) == false)
            {
                var roomFloor = RunRandomWalk(position);
                rooms.Add(new DungeonRoom(position, roomFloor));
                roomFloors.UnionWith(roomFloor);
            }
        }
    }

    private List<Vector2Int> FindAllDeadEnds(HashSet<Vector2Int> floorPositions)
    {
        List<Vector2Int> deadEnds = new List<Vector2Int>();
        foreach (var position in floorPositions.OrderBy(position => position.x).ThenBy(position => position.y))
        {
            int neighboursCount = 0;
            foreach (var dir in Direction.dirList)
            {
                if (floorPositions.Contains(position + dir))
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

    private List<DungeonRoom> CreateRooms(HashSet<Vector2Int> potentialRoomPositions)
    {
        List<DungeonRoom> rooms = new List<DungeonRoom>();
        int roomsCount = Mathf.RoundToInt(potentialRoomPositions.Count * roomPercent);
        List<Vector2Int> roomToCreate = potentialRoomPositions
            .OrderBy(position => position.x)
            .ThenBy(position => position.y)
            .OrderBy(_ => Random.value)
            .Take(roomsCount)
            .ToList();

        foreach (var roomPosition in roomToCreate)
        {
            var roomFloor = RunRandomWalk(roomPosition);
            rooms.Add(new DungeonRoom(roomPosition, roomFloor));
        }
        return rooms;
    }

    private HashSet<Vector2Int> MergeRoomFloors(List<DungeonRoom> rooms)
    {
        HashSet<Vector2Int> roomPos = new HashSet<Vector2Int>();

        foreach (var room in rooms)
        {
            roomPos.UnionWith(room.FloorPositions);
        }

        return roomPos;
    }

    private void SelectSpawnAndEndRooms(List<DungeonRoom> rooms, HashSet<Vector2Int> floorPositions)
    {
        if (rooms.Count == 0)
        {
            spawnRoomCenter = startPos;
            endRoomCenter = startPos;
            return;
        }

        DungeonRoom spawnRoom = rooms
            .Where(room => room.FloorPositions.Contains(startPos))
            .OrderBy(room => Vector2Int.Distance(room.Center, startPos))
            .FirstOrDefault();

        spawnRoom ??= rooms
            .OrderBy(room => Vector2Int.Distance(room.Center, startPos))
            .First();

        Dictionary<Vector2Int, int> distanceFromSpawn = FindDistancesFrom(spawnRoom.Center, floorPositions);

        DungeonRoom endRoom = rooms
            .Where(room => room != spawnRoom)
            .OrderByDescending(room => GetBestRoomDistance(room, distanceFromSpawn, spawnRoom.Center))
            .FirstOrDefault();

        spawnRoomCenter = spawnRoom.Center;
        endRoomCenter = endRoom?.Center ?? spawnRoom.Center;
    }

    private Dictionary<Vector2Int, int> FindDistancesFrom(Vector2Int start, HashSet<Vector2Int> floorPositions)
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

    private int GetBestRoomDistance(DungeonRoom room, Dictionary<Vector2Int, int> distances, Vector2Int spawnCenter)
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

    private void GenerateSpawnEndTiles()
    {
        tilemapVisualizer.GenerateTiles(TilemapVisualizer.TileType.Spawn, TilemapVisualizer.BiomeType.None, new[] { spawnRoomCenter });
        tilemapVisualizer.GenerateTiles(TilemapVisualizer.TileType.End, TilemapVisualizer.BiomeType.None, new[] { endRoomCenter });
    }

    private void CreateCorridors(HashSet<Vector2Int> floorPos, HashSet<Vector2Int> potentialRoomPos)
    {
        var currentPos = startPos;
        potentialRoomPos.Add(currentPos);

        for (int i = 0; i < corridorCount; i++)
        {
            var path = DungeonAlgorithm.CorridorGen(currentPos, corridorLength);
            currentPos = path[path.Count - 1];
            potentialRoomPos.Add(currentPos);
            floorPos.UnionWith(path);
        }
    }
}
