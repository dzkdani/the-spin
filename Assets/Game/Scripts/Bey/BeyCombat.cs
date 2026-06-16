using UnityEngine;

public class BeyCombat : MonoBehaviour
{
    BeyData data;
    BeyPhysics physics;
    [SerializeField]
    float impactMultiplier = 1f;

    private void Awake()
    {
        
    }
    
    public void Initialize(BeyData data, BeyPhysics physics)
    {
        this.data = data;
        this.physics = physics;

        this.data.OnRPMDepleted += LoseHandler;
        this.physics.OnImpact += ReceiveImpact;
    }

    public void ReceiveImpact(float impactForce)
    {
        float rpmLoss = impactForce * impactMultiplier;

        data.ReduceRPM(rpmLoss);

        Debug.Log($"{gameObject.name} received impact of {impactForce}, losing {rpmLoss} RPM.");
    }

    private void LoseHandler()
    {
        Debug.Log($"{gameObject.name} is defeated!");
    }
}
