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
        TitleAudioManager.Instance.PlayHover();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        TitleAudioManager.Instance.PlayClick();
    }
}