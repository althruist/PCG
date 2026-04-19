using System.Collections.Generic;
using UnityEngine;

public static class DungeonAlgorithm
{
    private enum splitType
    {
        Horizontal,
        Vertical
    }

    public static HashSet<Vector2Int> WalkGen(Vector2Int startPos, int walkLength)
    {
        HashSet<Vector2Int> path = new HashSet<Vector2Int>();
        path.Add(startPos);
        var previousPos = startPos;

        for (int i = 0; i < walkLength; i++)
        {
            var newPos = previousPos + Direction.GetRandomDirection();
            path.Add(newPos);
            previousPos = newPos;
        }
        return path;
    }

    public static List<Vector2Int> CorridorGen(Vector2Int startPos, int corridorLength)
    {
        List<Vector2Int> corridor = new List<Vector2Int>();
        var dir = Direction.GetRandomDirection();
        var currentPos = startPos;

        corridor.Add(currentPos);

        for (int i = 0; i < corridorLength; i++)
        {
            currentPos += dir;
            corridor.Add(currentPos);
        }
        return corridor;
    }

    public static List<BoundsInt> BinarySpacePartitioning(BoundsInt spaceToSplit, int minWidth, int minHeight)
    {
        Queue<BoundsInt> roomsQueue = new Queue<BoundsInt>();
        List<BoundsInt> roomsList = new List<BoundsInt>();
        roomsQueue.Enqueue(spaceToSplit);
        while (roomsQueue.Count > 0)
        {
            var room = roomsQueue.Dequeue();
            if (room.size.y >= minHeight && room.size.x >= minWidth)
            {
                if (Random.value < 0.5f)
                {
                    if (room.size.y >= minHeight * 2)
                    {
                        Split(splitType.Horizontal, roomsQueue, room);
                    }
                    else if (room.size.x >= minWidth * 2)
                    {
                        Split(splitType.Vertical, roomsQueue, room);
                    }
                    else if (room.size.x >= minWidth && room.size.y >= minHeight)
                    {
                        roomsList.Add(room);
                    }
                }
                else
                {
                    if (room.size.x >= minWidth * 2)
                    {
                        Split(splitType.Horizontal, roomsQueue, room);
                    }
                    else if (room.size.y >= minHeight * 2)
                    {
                        Split(splitType.Vertical, roomsQueue, room);
                    }
                    else if (room.size.x >= minWidth && room.size.y >= minHeight)
                    {
                        roomsList.Add(room);
                    }
                }
            }
        }

        if (roomsList.Count == 0)
        {
            roomsQueue.Enqueue(spaceToSplit);
            roomsList.Add(roomsQueue.Dequeue());
            if (roomsList.Count == 0) Debug.LogError("0 ROOM GENERATE");
            else Debug.Log("room generated -> " + roomsList.Count);
        }

        return roomsList;
    }

    private static void Split(splitType split, Queue<BoundsInt> roomQueue, BoundsInt room)
    {
        BoundsInt room1 = new BoundsInt();
        BoundsInt room2 = new BoundsInt();

        if (split == splitType.Vertical)
        {
            var xSplit = Random.Range(1, room.size.x);
            room1 = new BoundsInt(room.min, new Vector3Int(xSplit, room.size.y, room.size.z));
            room2 = new BoundsInt(new Vector3Int(room.min.x + xSplit, room.min.y, room.min.z), new Vector3Int(room.size.x - xSplit, room.size.y, room.size.z));
        }
        else if (split == splitType.Horizontal)
        {
            var ySplit = Random.Range(1, room.size.y);
            room1 = new BoundsInt(room.min, new Vector3Int(room.size.x, ySplit, room.size.z));
            room2 = new BoundsInt(new Vector3Int(room.min.x, room.min.y + ySplit, room.min.z), new Vector3Int(room.size.x, room.size.y - ySplit, room.size.z));
        }
        roomQueue.Enqueue(room1);
        roomQueue.Enqueue(room2);
    }
}

public static class Direction
{
    public static List<Vector2Int> dirList = new List<Vector2Int>
    {
        new Vector2Int(0,1),
        new Vector2Int(1,0),
        new Vector2Int(0,-1),
        new Vector2Int(-1,0)
    };

    public static Vector2Int GetRandomDirection()
    {
        return dirList[Random.Range(0, dirList.Count)];
    }
}