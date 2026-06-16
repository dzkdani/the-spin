using UnityEngine;
using System;

public class BeyData : MonoBehaviour
{
    [field: SerializeField]
    public BeyBaseSO Base { get; private set; }
    public event Action<float> OnRPMChanged;
    public event Action OnRPMDepleted;

    private float MaxRPM => Base.maxRPM;
    public float RPMNormalized => CurrentRPM / MaxRPM;
    public float CurrentRPM { get; private set; }


    private void Awake()
    {
        CurrentRPM = Base.maxRPM;

        OnRPMChanged?.Invoke(CurrentRPM);
    }

    public void ReduceRPM(float amount)
    {
        SetRPM(CurrentRPM - amount);
    }

    public void AddRPM(float amount)
    {
        SetRPM(CurrentRPM + amount);
    }

    private void SetRPM(float value)
    {
        float newRPM = Mathf.Clamp(value, 0, MaxRPM);

        if (Mathf.Approximately(newRPM, CurrentRPM))
            return;

        bool wasAlive = CurrentRPM > 0;

        CurrentRPM = newRPM;

        OnRPMChanged?.Invoke(CurrentRPM);

        if (wasAlive && CurrentRPM <= 0)
        {
            OnRPMDepleted?.Invoke();
        }
    }
}
