using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public sealed class BattleFeedback : MonoBehaviour
{
    private Image flash;
    private bool built;

    public static void EnsureInScene()
    {
        if (FindFirstObjectByType<BattleFeedback>(
                FindObjectsInactive.Include) != null)
            return;

        GameObject host = new GameObject("BattleFeedback", typeof(RectTransform));
        Canvas canvas = FindFirstObjectByType<Canvas>();

        if (canvas != null)
            host.transform.SetParent(canvas.transform, false);

        RectTransform rect = host.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        host.AddComponent<BattleFeedback>();
    }

    private void Awake()
    {
        BuildUI();
    }

    private void OnEnable()
    {
        BattleEvents.Burst += OnBurst;
        BattleEvents.AbilityActivated += OnAbilityActivated;
    }

    private void OnDisable()
    {
        BattleEvents.Burst -= OnBurst;
        BattleEvents.AbilityActivated -= OnAbilityActivated;

        if (flash != null)
            flash.DOKill();
    }

    private void BuildUI()
    {
        if (built)
            return;

        GameObject overlay = new GameObject("Flash", typeof(RectTransform));
        overlay.transform.SetParent(transform, false);

        RectTransform rect = overlay.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        flash = overlay.AddComponent<Image>();
        flash.color = new Color(1f, 1f, 1f, 0f);
        flash.raycastTarget = false;

        built = true;
    }

    private void OnBurst(BeyData loser)
    {
        ServiceLocator.Get<SFXPlayer>()?.PlayBurst();
        SpawnEffect(loser);

        if (flash == null)
            return;

        flash.DOKill();
        flash.color = new Color(1f, 0.34f, 0.16f, 0f);

        Sequence sequence = DOTween.Sequence();
        sequence.Append(flash.DOFade(0.55f, 0.06f));
        sequence.Append(flash.DOFade(0f, 0.45f));
        sequence.SetUpdate(true);
    }

    private void OnAbilityActivated(BeyData bey, string abilityId)
    {
        ServiceLocator.Get<SFXPlayer>()?.PlayAbility();
        SpawnEffect(bey);
    }

    private static void SpawnEffect(BeyData bey)
    {
        if (bey == null || bey.transform == null)
            return;

        ServiceLocator.Get<HitEffectSpawner>()?.Spawn(bey.transform.position);
    }
}
