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
        BattleEvents.DashWindUpStarted += OnDashWindUp;
    }

    private void OnDisable()
    {
        BattleEvents.DashWindUpStarted -= OnDashWindUp;
    }

    private void OnDashWindUp(BeyData bey)
    {
        if (bey != data || visualPivot == null)
            return;

        visualPivot.DOKill();
        visualPivot.DOPunchScale(
            Vector3.one * 0.15f,
            0.2f,
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
