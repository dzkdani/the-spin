using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class HitEffectSpawnerTests
{
    [UnityTest]
    public IEnumerator SpawnerReusesPooledParticles()
    {
        GameObject template = new GameObject("HitTemplate");
        template.SetActive(false);

        ParticleSystem prefab = template.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = prefab.main;

        main.playOnAwake = false;
        main.loop = false;
        main.duration = 0.05f;
        main.startLifetime = 0.05f;

        GameObject host = new GameObject("HitSpawner");
        float previousTimeScale = Time.timeScale;
        Time.timeScale = 1f;

        try
        {
            Component spawner = host.AddComponent(
                ProductionReflection.TypeOf("HitEffectSpawner"));

            ProductionReflection.SetField(spawner, "hitPrefab", prefab);
            ProductionReflection.Call(spawner, "Spawn", Vector3.zero);

            Assert.AreEqual(
                1,
                (int)ProductionReflection.Property(spawner, "ActiveCount"));

            Assert.AreEqual(
                7,
                (int)ProductionReflection.Property(spawner, "PooledCount"));

            yield return new WaitForSeconds(0.3f);

            Assert.AreEqual(
                0,
                (int)ProductionReflection.Property(spawner, "ActiveCount"));

            Assert.AreEqual(
                8,
                (int)ProductionReflection.Property(spawner, "PooledCount"));

            ProductionReflection.Call(spawner, "Spawn", Vector3.zero);

            Assert.AreEqual(
                1,
                (int)ProductionReflection.Property(spawner, "ActiveCount"));

            Assert.AreEqual(
                7,
                (int)ProductionReflection.Property(spawner, "PooledCount"));
        }
        finally
        {
            Time.timeScale = previousTimeScale;
            Object.DestroyImmediate(host);
            Object.DestroyImmediate(template);
        }
    }
}
