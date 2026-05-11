using System.Collections.Generic;
using UnityEngine;

public static class EnemySpawner
{
    // spawns enemies
    public static void Spawn(
        HashSet<Vector2Int> floorPositions,
        float enemySpawnRate1,
        float enemySpawnRate2,
        GameObject enemy1,
        GameObject enemy2,
        Vector2 playerSpawn,
        float protectedRadius)
    {
        foreach (var pos in floorPositions)
        {
            if (Vector2.Distance(pos, playerSpawn) <= protectedRadius)
            {
                continue;
            }

            Vector3 spawnPos = new Vector3(pos.x + 0.5f, pos.y + 0.5f, 0);

            // enemy 1
            if (Random.value < enemySpawnRate1)
            {
                Object.Instantiate(enemy1, spawnPos, Quaternion.identity);
            }

            // enemy 2
            else if (Random.value < enemySpawnRate2)
            {
                Object.Instantiate(enemy2, spawnPos, Quaternion.identity);
            }
        }
    }

    // clear enemies
    public static void Clear()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
        {
            Object.DestroyImmediate(enemy);
        }
    }
}