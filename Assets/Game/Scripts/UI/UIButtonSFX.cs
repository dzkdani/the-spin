using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonSFX : MonoBehaviour,
    IPointerEnterHandler,
    IPointerClickHandler
{
    [SerializeField] private AudioClip hoverSFX;
    [SerializeField] private AudioClip clickSFX;

    public void OnPointerEnter(PointerEventData eventData)
    {
        ServiceLocator.Get<TitleAudioManager>()?.PlayHover();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ServiceLocator.Get<TitleAudioManager>()?.PlayClick();
    }
}