using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private BeyController enemyPrefab;
    [SerializeField] private Transform[] spawnPoints;

    public BeyController SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("EnemySpawner requires an enemy prefab.", this);
            return null;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("EnemySpawner requires at least one spawn point.", this);
            return null;
        }

        Transform point =
            spawnPoints[Random.Range(0, spawnPoints.Length)];

        if (point == null)
        {
            Debug.LogError("EnemySpawner contains a missing spawn point.", this);
            return null;
        }

        return Instantiate(
            enemyPrefab,
            point.position,
            Quaternion.identity);
    }
    
}
