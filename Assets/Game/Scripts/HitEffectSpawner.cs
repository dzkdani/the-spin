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
        Spawn(pos, Color.white, 1f);
    }

    public void Spawn(Vector3 pos, Color color, float scale)
    {
        if (hitPrefab == null)
        {
            Debug.LogError("HitEffectSpawner requires a hit particle prefab.", this);
            return;
        }

        EnsurePool();

        ParticleSystem effect = Take();

        effect.transform.SetPositionAndRotation(pos, Quaternion.identity);
        effect.transform.localScale = Vector3.one * scale;
        effect.gameObject.SetActive(true);

        ParticleSystem.MainModule main = effect.main;
        main.startColor = color;

        effect.Clear(true);
        effect.Play(true);

        active.Add(effect);
    }

    public void SpawnFx(
        Vector3 pos,
        Color color,
        float scale,
        AbilityVisualKind kind)
    {
        if (hitPrefab == null)
            return;

        ParticleSystem effect = Instantiate(
            hitPrefab,
            pos,
            Quaternion.identity);

        effect.gameObject.SetActive(true);
        effect.transform.localScale = Vector3.one * scale;

        ParticleSystem.MainModule main = effect.main;
        main.startColor = color;

        ParticleSystem.ShapeModule shape = effect.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 25f;
        shape.radius = 0.3f;
        shape.radiusThickness = 1f;

        float lifetime = 0.5f;
        float speed = 6f;
        float size = 0.2f;
        int count = 16;

        switch (kind)
        {
            case AbilityVisualKind.Guard:
                shape.shapeType = ParticleSystemShapeType.Circle;
                shape.radius = 0.6f;
                shape.radiusThickness = 0.12f;
                lifetime = 0.55f;
                speed = 2.5f;
                size = 0.28f;
                count = 24;
                break;

            case AbilityVisualKind.Aura:
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = 0.9f;
                lifetime = 0.8f;
                speed = 0.8f;
                size = 0.22f;
                count = 30;
                break;

            case AbilityVisualKind.Dash:
                shape.shapeType = ParticleSystemShapeType.Cone;
                shape.angle = 8f;
                shape.radius = 0.2f;
                lifetime = 0.25f;
                speed = 12f;
                size = 0.12f;
                count = 12;
                break;

            case AbilityVisualKind.Counter:
                shape.shapeType = ParticleSystemShapeType.Circle;
                shape.radius = 0.4f;
                lifetime = 0.2f;
                speed = 4f;
                size = 0.15f;
                count = 10;
                break;

            case AbilityVisualKind.Sustain:
                shape.shapeType = ParticleSystemShapeType.Circle;
                shape.radius = 0.3f;
                lifetime = 0.6f;
                speed = 1.5f;
                size = 0.25f;
                count = 14;
                break;
        }

        main.startLifetime = new ParticleSystem.MinMaxCurve(lifetime);
        main.startSpeed = new ParticleSystem.MinMaxCurve(speed);
        main.startSize = new ParticleSystem.MinMaxCurve(size);
        main.duration = Mathf.Max(0.2f, lifetime);

        ParticleSystem.EmissionModule emission = effect.emission;
        emission.SetBursts(new[]
        {
            new ParticleSystem.Burst(0f, (short)count)
        });

        effect.Clear(true);
        effect.Play(true);

        Destroy(effect.gameObject, lifetime + 0.5f);
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
