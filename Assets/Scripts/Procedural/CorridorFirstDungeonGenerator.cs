using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CorridorFirstDungeonGenerator : WalkGenerator
{
    [SerializeField]
    [Range(0.1f, 1f)]
    private float roomPercent = 0.8f;

    protected override void RunDungeonGenerator()
    {
        CorridorFirstDungeonGen();
    }

    private void CorridorFirstDungeonGen()
    {
        HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
        HashSet<Vector2Int> potentialRoomPositions = new HashSet<Vector2Int>();
        tilemapVisualizer.Clear();
        CreateCorridors(floorPositions, potentialRoomPositions);

        HashSet<Vector2Int> roomPos = CreateRooms(potentialRoomPositions);

        List<Vector2Int> deadEnds = FindAllDeadEnds(floorPositions);

        CreateRoomsAtDeadEnd(deadEnds, roomPos);

        floorPositions.UnionWith(roomPos);

        tilemapVisualizer.GenerateTiles(TilemapVisualizer.TileType.Floor, TilemapVisualizer.BiomeType.Obsidian, floorPositions);
        WallGenerator.CreateWalls(floorPositions, tilemapVisualizer);
    }

    private void CreateRoomsAtDeadEnd(List<Vector2Int> deadEnds, HashSet<Vector2Int> roomFloors)
    {
        foreach (var position in deadEnds)
        {
            if(roomFloors.Contains(position) == false)
            {
                var roomFloor = RunRandomWalk(parameters, position);
                roomFloors.UnionWith(roomFloor);
            }
        }
    }

    private List<Vector2Int> FindAllDeadEnds(HashSet<Vector2Int> floorPositions)
    {
        List<Vector2Int> deadEnds = new List<Vector2Int>();
        foreach (var position in floorPositions)
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

    private HashSet<Vector2Int> CreateRooms(HashSet<Vector2Int> potentialRoomPositions)
    {
        HashSet<Vector2Int> roomPos = new HashSet<Vector2Int>();
        int roomsCount = Mathf.RoundToInt(potentialRoomPositions.Count * roomPercent);
        List<Vector2Int> roomToCreate = potentialRoomPositions.OrderBy(x => Guid.NewGuid()).Take(roomsCount).ToList();

        foreach (var roomPosition in roomToCreate)
        {
            var roomFloor = RunRandomWalk(parameters, roomPosition);
            roomPos.UnionWith(roomFloor);
        }
        return roomPos;
    }

    private void CreateCorridors(HashSet<Vector2Int> floorPos, HashSet<Vector2Int> potentialRoomPos)
    {
        var currentPos = startPos;
        potentialRoomPos.Add(currentPos);

        for (int i = 0; i < parameters.corridorCount; i++)
        {
            var path = DungeonAlgorithm.CorridorGen(currentPos, parameters.corridorLength);
            tilemapVisualizer.GenerateTiles(TilemapVisualizer.TileType.Floor, TilemapVisualizer.BiomeType.Obsidian, path);
            currentPos = path[path.Count - 1];
            potentialRoomPos.Add(currentPos);
            floorPos.UnionWith(path);
        }
    }
}
