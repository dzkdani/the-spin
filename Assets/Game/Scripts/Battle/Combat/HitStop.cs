using UnityEngine;

public static class HitStop
{
    private static HitStopRunner runner;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        runner = null;
    }

    public static void Play(float duration)
    {
        if (duration <= 0f || Time.timeScale <= 0f)
            return;

        if (runner == null)
        {
            GameObject host = new GameObject("HitStop");
            Object.DontDestroyOnLoad(host);
            runner = host.AddComponent<HitStopRunner>();
        }

        runner.Play(duration);
    }
}
