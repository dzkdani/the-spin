using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private BeyController enemyPrefab;
    [SerializeField] private Transform[] spawnPoints;

    public BeyController SpawnEnemy()
    {
        Transform point =
            spawnPoints[Random.Range(0, spawnPoints.Length)];

        return Instantiate(
            enemyPrefab,
            point.position,
            Quaternion.identity);
    }
    
}