using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class EnemySpawnConfig
{
    public GameObject enemyPrefab;
    public EnemyMovementType movementType;
    public AnimationCurve spawnRatioOverTime;
}

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] List<SpawnArea> spawnAreas;
    [SerializeField] List<EnemySpawnConfig> enemyTypes;
    [SerializeField] float spawnInterval = 5f;
    [SerializeField] AnimationCurve spawnRateOverTime;
    [SerializeField] AnimationCurve maxEnemiesOverTime;

    [Header("Tracking")]
    [SerializeField] private float difficultyTime = 0f;
    float spawnTimer = 0f;
    List<GameObject> aliveEnemies = new List<GameObject>();

    private void Update()
    {
        difficultyTime += Time.deltaTime;
        spawnTimer += Time.deltaTime;

        float intervalMod = spawnRateOverTime.Evaluate(difficultyTime);
        float interval = Mathf.Max(0.1f, spawnInterval / intervalMod);

        aliveEnemies.RemoveAll(e => e == null);

        int currentAlive = aliveEnemies.Count;
        int maxAllowed = Mathf.FloorToInt(maxEnemiesOverTime.Evaluate(difficultyTime));

        if (spawnTimer >= interval && currentAlive < maxAllowed)
        {
            spawnTimer = 0f;
            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        if (enemyTypes.Count == 0 || spawnAreas.Count == 0)
            return;

        GameObject enemyToSpawn = ChooseEnemyByWeightedRatio(out EnemyMovementType type);
        List<SpawnArea> validAreas = spawnAreas.FindAll(area => area.MovementType == type);

        if (validAreas.Count == 0)
        {
            Debug.LogWarning($"No spawn areas found for enemy type {type}");
            return;
        }

        SpawnArea area = validAreas[Random.Range(0, validAreas.Count)];
        Vector3 spawnPos = area.GetRandomPointInArea();

        GameObject newEnemy = Instantiate(enemyToSpawn, spawnPos, Quaternion.identity);
        aliveEnemies.Add(newEnemy);
    }

    GameObject ChooseEnemyByWeightedRatio(out EnemyMovementType selectedType)
    {
        List<float> weights = new List<float>();
        foreach (var type in enemyTypes)
        {
            float weight = type.spawnRatioOverTime.Evaluate(difficultyTime);
            weights.Add(weight);
        }

        float total = weights.Sum();
        float rand = Random.value * total;
        float cumulative = 0f;

        for (int i = 0; i < enemyTypes.Count; i++)
        {
            cumulative += weights[i];
            if (rand <= cumulative)
            {
                selectedType = enemyTypes[i].movementType;
                return enemyTypes[i].enemyPrefab;
            }
        }

        selectedType = enemyTypes[0].movementType;
        return enemyTypes[0].enemyPrefab;
    }
}
