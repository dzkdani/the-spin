using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonSFX : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    [SerializeField] private AudioClip hoverSFX;
    [SerializeField] private AudioClip clickSFX;

    [Header("Hover")]
    [SerializeField] private RectTransform hoverTarget;
    [SerializeField] private float hoverScale = 1.08f;
    [SerializeField] private float hoverDuration = 0.12f;

    private void Awake()
    {
        if (hoverTarget == null)
            hoverTarget = transform as RectTransform;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ServiceLocator.Get<TitleAudioManager>()?.PlayHover();

        if (hoverTarget == null)
            return;

        hoverTarget.DOKill();
        hoverTarget.DOScale(hoverScale, hoverDuration)
            .SetEase(Ease.OutQuad);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoverTarget == null)
            return;

        hoverTarget.DOKill();
        hoverTarget.DOScale(Vector3.one, hoverDuration)
            .SetEase(Ease.OutQuad);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ServiceLocator.Get<TitleAudioManager>()?.PlayClick();
    }
}
