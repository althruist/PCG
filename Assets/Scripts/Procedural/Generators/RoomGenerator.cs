using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public static class RoomGenerator
{
    // randomly chooses potential room anchors and creates rooms with random walks.
    public static List<DungeonRoom> CreateRooms(
        HashSet<Vector2Int> potentialRoomPositions,
        float roomPercent,
        int repetitions,
        int walkLength)
    {
        List<DungeonRoom> rooms = new List<DungeonRoom>();
        int roomsCount = Mathf.RoundToInt(potentialRoomPositions.Count * roomPercent);
        List<Vector2Int> roomPositionsToCreate = potentialRoomPositions
            .OrderBy(position => position.x)
            .ThenBy(position => position.y)
            .OrderBy(_ => Random.value)
            .Take(roomsCount)
            .ToList();

        foreach (var roomPosition in roomPositionsToCreate)
        {
            var roomFloor = RandomWalkGenerator.RunRandomWalk(roomPosition, repetitions, walkLength);
            rooms.Add(new DungeonRoom(roomPosition, roomFloor));
        }

        return rooms;
    }

    // adds extra rooms at corridor dead ends that are not already inside a room
    public static void CreateRoomsAtDeadEnds(
        List<Vector2Int> deadEnds,
        List<DungeonRoom> rooms,
        HashSet<Vector2Int> roomFloors,
        int repetitions,
        int walkLength)
    {
        foreach (var position in deadEnds)
        {
            if (roomFloors.Contains(position))
            {
                continue;
            }

            var roomFloor = RandomWalkGenerator.RunRandomWalk(position, repetitions, walkLength);
            rooms.Add(new DungeonRoom(position, roomFloor));
            roomFloors.UnionWith(roomFloor);
        }
    }

    // combines every room's floor positions into one set
    public static HashSet<Vector2Int> MergeRoomFloors(List<DungeonRoom> rooms)
    {
        HashSet<Vector2Int> roomFloors = new HashSet<Vector2Int>();

        foreach (var room in rooms)
        {
            roomFloors.UnionWith(room.FloorPositions);
        }

        return roomFloors;
    }
}
