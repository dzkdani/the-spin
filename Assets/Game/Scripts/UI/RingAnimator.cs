using DG.Tweening;
using UnityEngine;

public class RingAnimator : MonoBehaviour
{
    private Tween scaleTween;

    void Start()
    {
        scaleTween = transform
            .DOScale(1.5f, 2f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetLink(gameObject);
    }

    private void OnDestroy()
    {
        scaleTween?.Kill();
    }
}