using UnityEngine;
using System;

public class BeyData : MonoBehaviour
{
    [field: SerializeField]
    public BeyStats Base { get; private set; }
    public event Action<float> OnRPMChanged;
    public event Action OnRPMDepleted;

    private float MaxRPM => Base != null ? Base.maxRPM : 0f;
    public float RPMNormalized => MaxRPM > 0f ? CurrentRPM / MaxRPM : 0f;
    public float CurrentRPM { get; private set; }


    private void Awake()
    {
        if (Base == null)
        {
            Debug.LogError("BeyData requires a BeyStats reference.", this);
            return;
        }

        if (Base.maxRPM <= 0f)
        {
            Debug.LogError("BeyData requires BeyStats.maxRPM to be greater than zero.", this);
            return;
        }

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
        if (Base == null || MaxRPM <= 0f)
            return;

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
