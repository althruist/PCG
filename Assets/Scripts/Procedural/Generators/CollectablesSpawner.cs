using System.Collections.Generic;
using UnityEngine;

public static class CollectablesSpawner
{
    public static void Spawn(
        HashSet<Vector2Int> floorPositions,
        int collectablesAmount,
        GameObject collectable,
        HashSet<Vector2Int> blockedPositions = null,
        IEnumerable<Vector2Int> protectedPositions = null)
    {
        if (floorPositions == null || floorPositions.Count == 0)
            return;

        HashSet<Vector2Int> blocked =
            blockedPositions ?? new HashSet<Vector2Int>();

        HashSet<Vector2Int> protectedSet =
            protectedPositions != null
            ? new HashSet<Vector2Int>(protectedPositions)
            : new HashSet<Vector2Int>();

        List<Vector2Int> validPositions = new List<Vector2Int>();

        foreach (var pos in floorPositions)
        {
            if (!blocked.Contains(pos) &&
                !protectedSet.Contains(pos))
            {
                validPositions.Add(pos);
            }
        }

        if (validPositions.Count == 0)
            return;

        collectablesAmount =
            Mathf.Min(collectablesAmount, validPositions.Count);

        List<Vector2Int> spawnedPositions =
            new List<Vector2Int>();

        // ------------------------
        // FIRST SPAWN = RANDOM
        // ------------------------

        Vector2Int firstPos =
            validPositions[Random.Range(0, validPositions.Count)];

        SpawnCollectable(firstPos, collectable);

        spawnedPositions.Add(firstPos);

        validPositions.Remove(firstPos);

        // ------------------------
        // REMAINING SPAWNS
        // ------------------------

        while (spawnedPositions.Count < collectablesAmount)
        {
            Vector2Int bestPos = validPositions[0];

            float bestDistance = -1f;

            foreach (var candidate in validPositions)
            {
                float nearestDistance = float.MaxValue;

                // find closest existing collectable
                foreach (var spawned in spawnedPositions)
                {
                    float dist =
                        Vector2Int.Distance(candidate, spawned);

                    if (dist < nearestDistance)
                    {
                        nearestDistance = dist;
                    }
                }

                // maximize minimum distance
                if (nearestDistance > bestDistance)
                {
                    bestDistance = nearestDistance;
                    bestPos = candidate;
                }
            }

            SpawnCollectable(bestPos, collectable);

            spawnedPositions.Add(bestPos);

            validPositions.Remove(bestPos);
        }
    }

    private static void SpawnCollectable(
        Vector2Int pos,
        GameObject collectable)
    {
        Vector3 spawnPos = new Vector3(
            pos.x + 0.5f,
            pos.y + 0.5f,
            0);

        Object.Instantiate(
            collectable,
            spawnPos,
            Quaternion.identity);
    }

    public static void Clear()
    {
        GameObject[] collectables =
            GameObject.FindGameObjectsWithTag("Collectable");

        foreach (GameObject collectable in collectables)
        {
            Object.DestroyImmediate(collectable);
        }
    }
}