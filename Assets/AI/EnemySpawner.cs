using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public GameObject enemyPrefab;
        public int count = 5;
        public float spawnInterval = 1f;
    }

    public Transform parentObject;
    public Transform spawnPoint;
    public List<Wave> waves = new List<Wave>();
    public float timeBetweenWaves = 5f;

    private int currentWaveIndex = 0;
    private bool spawningWave = false;

    void Start()
    {
        StartCoroutine(SpawnWaves());
    }

    IEnumerator SpawnWaves()
    {
        while (currentWaveIndex < waves.Count)
        {
            Wave wave = waves[currentWaveIndex];
            spawningWave = true;

            for (int i = 0; i < wave.count; i++)
            {
                SpawnEnemy(wave.enemyPrefab);
                yield return new WaitForSeconds(wave.spawnInterval);
            }

            spawningWave = false;
            currentWaveIndex++;

            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    void SpawnEnemy(GameObject prefab)
    {
        if (prefab == null || spawnPoint == null) return;

        GameObject clone = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation, parentObject);
        GameController.Instance.enemyUnits.Add(clone);

        Health health = clone.GetComponent<Health>();
        if (health != null)
        {
            health.Died += () => GameController.Instance.enemyUnits.Remove(clone);
        }
    }
}
