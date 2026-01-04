using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject prefabToSpawn;
    public Transform parentObject;
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

        GameObject clone = Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation, parent: parentObject);
        GameController.Instance.enemyUnits.Add(clone);
        Health health = clone.GetComponent<Health>();
        if (health != null)
        {
            health.Died += () => GameController.Instance.enemyUnits.Remove(clone);
        }
    }
}
