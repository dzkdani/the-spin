using DG.Tweening;
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

    private void OnEnable()
    {
        BattleEvents.DashWindUpStarted += OnDashWindUpStarted;
        BattleEvents.DashStarted += OnDashStarted;
        BattleEvents.LaunchResolved += OnLaunchResolved;
    }

    private void OnDisable()
    {
        BattleEvents.DashWindUpStarted -= OnDashWindUpStarted;
        BattleEvents.DashStarted -= OnDashStarted;
        BattleEvents.LaunchResolved -= OnLaunchResolved;
    }

    private void OnDashWindUpStarted(BeyData bey)
    {
        if (bey == data)
            Punch(0.12f, 0.15f);
    }

    private void OnDashStarted(BeyData bey)
    {
        if (bey == data)
            Punch(0.08f, 0.12f);
    }

    private void OnLaunchResolved(BeyData bey, float power)
    {
        if (bey == data)
            Punch(0.1f + 0.15f * power, 0.25f);
    }

    private void Punch(float amount, float duration)
    {
        if (visualPivot == null)
            return;

        visualPivot.DOKill();
        visualPivot.DOPunchScale(
            Vector3.one * amount,
            duration,
            8,
            0.5f);
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
