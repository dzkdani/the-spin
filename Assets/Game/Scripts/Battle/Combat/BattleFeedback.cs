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
        BattleEvents.AbilityProcced += OnAbilityProcced;
    }

    private void OnDisable()
    {
        BattleEvents.Burst -= OnBurst;
        BattleEvents.AbilityActivated -= OnAbilityActivated;
        BattleEvents.AbilityProcced -= OnAbilityProcced;

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
        SpawnEffect(loser, null, 2f);

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
        SpawnEffect(bey, abilityId, 1f);
    }

    private void OnAbilityProcced(BeyData bey, string abilityId)
    {
        SpawnEffect(bey, abilityId, 0.6f);
    }

    private static void SpawnEffect(
        BeyData bey,
        string abilityId,
        float procScale)
    {
        if (bey == null || bey.transform == null)
            return;

        AbilityDefinition definition =
            ContentRegistry.Get<AbilityDefinition>(abilityId);

        Color color = definition != null && definition.accent.a > 0f
            ? definition.accent
            : (bey.Elements.Current != null
                ? bey.Elements.Current.color
                : Color.white);

        float scale = definition != null
            ? definition.vfxScale * procScale
            : procScale;

        AbilityVisualKind kind = definition != null
            ? definition.visualKind
            : AbilityVisualKind.Strike;

        ServiceLocator.Get<HitEffectSpawner>()?.SpawnFx(
            bey.transform.position,
            color,
            scale,
            kind);
    }
}
