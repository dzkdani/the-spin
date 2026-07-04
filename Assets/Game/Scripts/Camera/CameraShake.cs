using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [SerializeField] private float damping = 25f;

    private Vector3 originalLocalPos;
    private float intensity;

    private void Awake()
    {
        Instance = this;
        originalLocalPos = transform.localPosition;
    }

    private void LateUpdate()
    {
        if (intensity > 0f)
        {
            transform.localPosition =
                originalLocalPos +
                (Vector3)Random.insideUnitCircle * intensity;

            intensity = Mathf.MoveTowards(
                intensity,
                0f,
                damping * Time.deltaTime);
        }
        else
        {
            transform.localPosition = originalLocalPos;
        }
    }

    public void Shake(float amount)
    {
        intensity = Mathf.Max(intensity, amount);
    }
}