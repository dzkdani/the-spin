using DG.Tweening;
using UnityEngine;

public sealed class PlayerIndicator : MonoBehaviour
{
    [SerializeField] private Sprite sprite;
    [SerializeField] private float height = 1.2f;
    [SerializeField] private float holdSeconds = 2f;
    [SerializeField] private float fadeSeconds = 0.5f;

    private BeyData data;
    private Transform markerTransform;
    private SpriteRenderer marker;

    private void Awake()
    {
        data = GetComponent<BeyData>();

        GameObject child = new GameObject("PlayerIndicator");

        child.transform.SetParent(transform, false);
        markerTransform = child.transform;
        markerTransform.localPosition = new Vector3(0f, height, 0f);
        markerTransform.localScale = Vector3.one * 0.55f;

        marker = child.AddComponent<SpriteRenderer>();
        marker.sprite = sprite;
        marker.sortingOrder = 30;
    }

    private void OnEnable()
    {
        if (marker != null)
            Show();
    }

    private void OnDisable()
    {
        if (markerTransform != null)
            markerTransform.DOKill();

        if (marker != null)
            marker.DOKill();
    }

    private void Show()
    {
        markerTransform.DOKill();
        marker.DOKill();

        Color color = data != null && data.Elements.Current != null
            ? data.Elements.Current.color
            : Color.white;

        color.a = 1f;
        marker.color = color;
        marker.enabled = true;

        markerTransform.localPosition = new Vector3(0f, height, 0f);

        markerTransform.DOLocalMoveY(height + 0.15f, 0.35f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        marker.DOFade(0f, fadeSeconds)
            .SetDelay(holdSeconds)
            .OnComplete(() => marker.enabled = false);
    }
}
