using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private BeyController enemyPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private BeyBlueprint[] blueprints;

    public BeyController SpawnEnemy()
    {
        return SpawnEnemy(0);
    }

    public BeyController SpawnEnemy(int waveIndex)
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

        BeyController enemy = Instantiate(
            enemyPrefab,
            point.position,
            Quaternion.identity);

        BeyBlueprint blueprint = null;

        if (blueprints != null && blueprints.Length > 0)
        {
            int index = Mathf.Clamp(waveIndex, 0, blueprints.Length - 1);
            blueprint = blueprints[index];
        }

        if (blueprint != null && enemy.Data != null)
            enemy.Data.Configure(blueprint);

        if (blueprint != null &&
            blueprint.aiProfile != null &&
            enemy.Input is EnemyInput enemyInput)
        {
            enemyInput.SetProfile(blueprint.aiProfile);
        }

        return enemy;
    }
}
