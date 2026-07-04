using UnityEngine;

public class BeyVisual : MonoBehaviour
{
    [SerializeField]
    Transform visualPivot;

    [SerializeField] 
    float maxRPM = 3000f;
    
    BeyData data;

    public void Initialize(BeyData data)
    {
        this.data = data;
    }

    private void Update()
    {
        float spinSpeed =
            data.RPMNormalized * maxRPM;

        visualPivot.Rotate(
            Vector3.forward,
            spinSpeed * Time.deltaTime
        );
    }
}
