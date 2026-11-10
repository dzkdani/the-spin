using UnityEngine;

public class SFXPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    [Header("Hit")]
    [SerializeField] private AudioClip[] hitClips;

    private void Awake()
    {
        if (!ServiceLocator.TryRegister(this))
        {
            Destroy(gameObject);
            return;
        }
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

    private void OnImpactResolved(ImpactEvent impact)
    {
        PlayHit();
    }

    public void PlayHit()
    {
        Play(hitClips, 1f, 0.9f, 1.1f);
    }

    public void Play(
        AudioClip[] clips,
        float volume,
        float minPitch,
        float maxPitch)
    {
        if (clips == null || clips.Length == 0)
            return;

        if (audioSource == null)
        {
            Debug.LogError("SFXPlayer requires an AudioSource reference.", this);
            return;
        }

        audioSource.pitch = Random.Range(minPitch, maxPitch);

        audioSource.PlayOneShot(
            clips[Random.Range(0, clips.Length)],
            volume);

        audioSource.pitch = 1f;
    }
}
