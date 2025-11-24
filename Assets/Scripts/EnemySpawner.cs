using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject prefabToSpawn;
    public Transform spawnPoint;
    public float spawnInterval = 2f;

    private float timer;

    void Update()
    {

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            Spawn();
            timer = 0f;
        }
    }

    void Spawn()
    {
        if (prefabToSpawn == null || spawnPoint == null) return;

        Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);
        Debug.Log($"Spawned {prefabToSpawn.name} at {spawnPoint.position}");
    }
}
