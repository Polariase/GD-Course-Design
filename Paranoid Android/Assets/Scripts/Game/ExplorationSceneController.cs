using MyPool;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class EnemySpawnWeight
{
    public string enemyName;
    public int weight;
}

public class ExplorationSceneController : MonoBehaviour
{
    public GameObject chestPrefab;

    private int _spawnCount = 6;

    public List<EnemySpawnWeight> enemyWeights = new List<EnemySpawnWeight>();

    public float spawnRadius = 10f;
    public int minEnemiesPerChest = 1;
    public int maxEnemiesPerChest = 2;

    public Transform SweeperPos;

    private void Start()
    {
        InitializeExplorationScene();
    }

    private void InitializeExplorationScene()
    {
        GameObject[] allPoints = GameObject.FindGameObjectsWithTag("ChestPoint");
        if (allPoints.Length == 0)
        {
            Debug.LogWarning("场景中未找到任何带有 'ChestPoint' 标签的物体！");
            return;
        }
        List<GameObject> availablePoints = new(allPoints);
        int actualSpawnCount = Mathf.Min(_spawnCount, availablePoints.Count);

        System.Random globalRand = new System.Random(Guid.NewGuid().GetHashCode());

        ShuffleList(availablePoints);
        SpawnChestsAndGuards(availablePoints, actualSpawnCount, globalRand);
        SpawnSweeper();
    }

    private void SpawnSweeper()
    {
        PoolManager.Instance.enemy.SpawnAt("Sweeper", SweeperPos.position, SweeperPos.rotation);
    }

    private void SpawnChestsAndGuards(List<GameObject> points, int count, System.Random rand)
    {
        for (int i = 0; i < count; i++)
        {
            Transform spawnTransform = points[i].transform;
            GameObject chestInstance = Instantiate(chestPrefab, spawnTransform.position, spawnTransform.rotation);
            chestInstance.transform.SetParent(spawnTransform);
            int enemyCountToSpawn = rand.Next(minEnemiesPerChest, maxEnemiesPerChest + 1);
            for (int j = 0; j < enemyCountToSpawn; j++)
            {
                SpawnRandomEnemyNear(spawnTransform.position, rand);
            }
        }
    }

    private void SpawnRandomEnemyNear(Vector3 centerPosition, System.Random rand)
    {
        EnemyPool pool = PoolManager.Instance.enemy;
        if (enemyWeights == null || enemyWeights.Count == 0) return;

        string selectedEnemyKey = GetRandomEnemyKeyByWeight();

        if (TryGetRandomNavMeshPosition(centerPosition, spawnRadius, out Vector3 spawnPos,rand))
        {
            pool.SpawnAt(selectedEnemyKey, spawnPos, Quaternion.LookRotation(centerPosition - spawnPos));
        }
    }

    private string GetRandomEnemyKeyByWeight()
    {
        int totalWeight = 0;
        foreach (var item in enemyWeights)
        {
            totalWeight += item.weight;
        }

        System.Random sysRandom = new System.Random(Guid.NewGuid().GetHashCode());
        int roll = sysRandom.Next(0, totalWeight);
        int cursor = 0;

        foreach (var item in enemyWeights)
        {
            cursor += item.weight;
            if (roll < cursor)
            {
                return item.enemyName;
            }
        }

        return enemyWeights[sysRandom.Next(0, enemyWeights.Count)].enemyName;
    }

    private bool TryGetRandomNavMeshPosition(Vector3 center, float radius, out Vector3 result, System.Random rand)
    {
        float minRadius = 3f;
        float randomRadius = (float)(rand.NextDouble() * (radius - minRadius) + minRadius);
        float randomAngle = (float)(rand.NextDouble() * 360.0) * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(Mathf.Cos(randomAngle), 0f, Mathf.Sin(randomAngle)) * randomRadius;
        Vector3 randomTargetPos = center + offset;

        if (NavMesh.SamplePosition(randomTargetPos, out NavMeshHit hit, 2f, 1 << NavMesh.GetAreaFromName("Walkable")))
        {
            result = hit.position;
            return true;
        }

        if (NavMesh.SamplePosition(randomTargetPos, out NavMeshHit hitBase, radius, NavMesh.AllAreas))
        {
            result = hitBase.position;
            return true;
        }

        result = center;
        return false;
    }

    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }
}