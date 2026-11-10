using UnityEngine;

public class HitEffectSpawner : MonoBehaviour
{
    [SerializeField]
    ParticleSystem hitPrefab;

    private void Awake()
    {
        ServiceLocator.Register(this);
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister(this);
    }

    public void Spawn(Vector3 pos)
    {
        if (hitPrefab == null)
        {
            Debug.LogError("HitEffectSpawner requires a hit particle prefab.", this);
            return;
        }

        Instantiate(hitPrefab, pos, Quaternion.identity);
    }
}
