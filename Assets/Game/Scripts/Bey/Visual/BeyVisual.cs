using UnityEngine;

public class BeyVisual : MonoBehaviour
{
    [SerializeField]
    Transform visualPivot;

    BeyData data;

    public void Initialize(BeyData data)
    {
        this.data = data;
    }

    private void Update()
    {
        if (data == null || data.Base == null || visualPivot == null)
            return;

        float spinSpeed =
            data.RPMNormalized * data.Base.maxRPM;

        visualPivot.Rotate(
            Vector3.forward,
            spinSpeed * Time.deltaTime
        );
    }
}
