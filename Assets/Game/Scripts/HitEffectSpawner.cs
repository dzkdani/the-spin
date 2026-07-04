using UnityEngine;

public class HitEffectSpawner : MonoBehaviour
{
    public static HitEffectSpawner Instance;

    [SerializeField]
    ParticleSystem hitPrefab;

    private void Awake()
    {
        Instance = this;
    }

    public void Spawn(Vector3 pos)
    {
        Instantiate(hitPrefab, pos, Quaternion.identity);
    }
}