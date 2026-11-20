using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class HitStopTests
{
    [UnityTest]
    public IEnumerator HitStopRestoresTimeScale()
    {
        Time.timeScale = 1f;

        ProductionReflection.CallStatic("HitStop", "Play", 0.05f);

        Assert.Less(Time.timeScale, 1f);

        yield return new WaitForSecondsRealtime(0.2f);

        Assert.AreEqual(1f, Time.timeScale, 0.001f);
    }
}
