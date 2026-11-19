using System.Collections.Generic;
using UnityEngine;

public class HitEffectSpawner : MonoBehaviour
{
    [SerializeField] private ParticleSystem hitPrefab;
    [SerializeField] private int prewarm = 8;
    [SerializeField] private int maxSize = 32;

    private readonly List<ParticleSystem> idle =
        new List<ParticleSystem>();

    private readonly List<ParticleSystem> active =
        new List<ParticleSystem>();

    private Transform poolRoot;
    private bool initialized;

    public int ActiveCount => active.Count;
    public int PooledCount => idle.Count;

    private void Awake()
    {
        ServiceLocator.Register(this);
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister(this);
    }

    private void OnEnable()
    {
        BattleEvents.ImpactResolved += OnImpactResolved;
    }

    private void OnDisable()
    {
        BattleEvents.ImpactResolved -= OnImpactResolved;
    }

    private void Update()
    {
        for (int i = active.Count - 1; i >= 0; i--)
        {
            ParticleSystem effect = active[i];

            if (effect != null && effect.IsAlive(true))
                continue;

            active.RemoveAt(i);

            if (effect == null)
                continue;

            effect.gameObject.SetActive(false);
            idle.Add(effect);
        }
    }

    private void OnImpactResolved(ImpactEvent impact)
    {
        Spawn(impact.Point);
    }

    public void Spawn(Vector3 pos)
    {
        if (hitPrefab == null)
        {
            Debug.LogError("HitEffectSpawner requires a hit particle prefab.", this);
            return;
        }

        EnsurePool();

        ParticleSystem effect = Take();

        effect.transform.SetPositionAndRotation(pos, Quaternion.identity);
        effect.gameObject.SetActive(true);
        effect.Clear(true);
        effect.Play(true);

        active.Add(effect);
    }

    private ParticleSystem Take()
    {
        while (idle.Count > 0)
        {
            int last = idle.Count - 1;
            ParticleSystem effect = idle[last];
            idle.RemoveAt(last);

            if (effect != null)
                return effect;
        }

        if (active.Count >= Mathf.Max(1, maxSize) && active.Count > 0)
        {
            ParticleSystem oldest = active[0];
            active.RemoveAt(0);
            return oldest;
        }

        return Create();
    }

    private void EnsurePool()
    {
        if (initialized)
            return;

        initialized = true;

        GameObject root = new GameObject(name + " Pool");
        root.transform.SetParent(transform, false);
        poolRoot = root.transform;

        int count = Mathf.Max(1, prewarm);

        for (int i = 0; i < count; i++)
            idle.Add(Create());
    }

    private ParticleSystem Create()
    {
        ParticleSystem effect = Instantiate(hitPrefab, poolRoot, false);
        effect.gameObject.SetActive(false);

        return effect;
    }
}
