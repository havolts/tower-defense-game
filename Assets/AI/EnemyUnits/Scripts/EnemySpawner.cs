using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class WaveEnemy
    {
        public GameObject enemyPrefab;
        public int count = 1;
        public float spawnInterval = 0.5f;
    }

    [System.Serializable]
    public class Wave
    {
        public List<WaveEnemy> enemies = new List<WaveEnemy>();
    }

    [Header("Wave Settings")]
    public List<Wave> waves = new List<Wave>();
    public float timeBetweenWaves = 5f;

    [Header("Spawn Points")]
    public List<Transform> spawnPoints = new List<Transform>();

    [Header("Auto Wave Settings")]
    public List<GameObject> autoGeneratePrefabs = new List<GameObject>();
    public int autoWaveEnemyCountMin = 3;
    public int autoWaveEnemyCountMax = 8;
    public float autoWaveSpawnInterval = 0.5f;

    private int currentWaveIndex = 0;

    void Start()
    {
        StartCoroutine(SpawnWaves());
    }

    IEnumerator SpawnWaves()
    {
        while (true) // Loop indefinitely
        {
            Wave currentWave = null;

            // Determine wave type: predefined or auto-generated
            if (currentWaveIndex < waves.Count)
            {
                currentWave = waves[currentWaveIndex];
            }
            else
            {
                // Auto-generate wave
                if (autoGeneratePrefabs.Count == 0)
                {
                    Debug.LogWarning("No prefabs assigned for auto-generated waves!");
                    yield break;
                }

                currentWave = new Wave();
                int prefabCount = Random.Range(1, autoGeneratePrefabs.Count + 1);

                for (int i = 0; i < prefabCount; i++)
                {
                    WaveEnemy we = new WaveEnemy();
                    we.enemyPrefab = autoGeneratePrefabs[Random.Range(0, autoGeneratePrefabs.Count)];
                    we.count = Random.Range(autoWaveEnemyCountMin, autoWaveEnemyCountMax + 1);
                    we.spawnInterval = autoWaveSpawnInterval;
                    currentWave.enemies.Add(we);
                }
            }

            List<GameObject> spawnedEnemies = new List<GameObject>();

            // Spawn all enemies in the wave
            foreach (WaveEnemy waveEnemy in currentWave.enemies)
            {
                if (waveEnemy.enemyPrefab == null)
                {
                    Debug.LogWarning("WaveEnemy prefab is null, skipping.");
                    continue;
                }

                for (int i = 0; i < waveEnemy.count; i++)
                {
                    if (spawnPoints.Count == 0)
                    {
                        Debug.LogWarning("No spawn points assigned!");
                        yield break;
                    }

                    Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
                    GameObject clone = Instantiate(waveEnemy.enemyPrefab, spawnPoint.position, spawnPoint.rotation, GameController.Instance.transform);
                    spawnedEnemies.Add(clone);
                    GameController.Instance.enemyUnits.Add(clone);

                    Health health = clone.GetComponent<Health>();
                    if (health != null)
                    {
                        health.Died += () =>
                        {
                            GameController.Instance.enemyUnits.Remove(clone);
                            spawnedEnemies.Remove(clone);
                        };
                    }

                    yield return new WaitForSeconds(waveEnemy.spawnInterval);
                }

            }

            // Wait until all enemies are dead
            while (spawnedEnemies.Count > 0)
                yield return null;

            currentWaveIndex++;
            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }
}
