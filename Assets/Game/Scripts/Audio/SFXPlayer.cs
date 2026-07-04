using UnityEngine;

public class SFXPlayer : MonoBehaviour
{
    public static SFXPlayer Instance { get; private set; }

    [SerializeField] private AudioSource audioSource;

    [Header("Hit")]
    [SerializeField] private AudioClip[] hitClips;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
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

        audioSource.pitch = Random.Range(minPitch, maxPitch);

        audioSource.PlayOneShot(
            clips[Random.Range(0, clips.Length)],
            volume);

        audioSource.pitch = 1f;
    }
}