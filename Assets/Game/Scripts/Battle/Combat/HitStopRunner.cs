using System.Collections;
using UnityEngine;

public sealed class HitStopRunner : MonoBehaviour
{
    private Coroutine routine;

    public void Play(float duration)
    {
        if (Time.timeScale <= 0f)
            return;

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(Run(duration));
    }

    private IEnumerator Run(float duration)
    {
        Time.timeScale = 0.05f;

        yield return new WaitForSecondsRealtime(duration);

        if (Time.timeScale > 0f)
            Time.timeScale = 1f;

        routine = null;
    }
}
