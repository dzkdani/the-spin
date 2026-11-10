using DG.Tweening;
using UnityEngine;

public class TitleAnimator : MonoBehaviour
{
    private Tween rotateTween;
    private Tween scaleTween;
    private Sequence shakeSequence;

    void Start()
    {
        rotateTween = transform
            .DORotate(new Vector3(0,0,3),1.2f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1,LoopType.Yoyo)
            .SetLink(gameObject);
        
        scaleTween = transform
            .DOScale(0.08f,0.8f)
            .SetLoops(-1,LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetLink(gameObject);

        shakeSequence = DOTween.Sequence();

        shakeSequence.AppendInterval(3f);

        shakeSequence.Append(transform.DOShakeRotation(
            0.25f,
            4f,
            20));

        shakeSequence.SetLoops(-1).SetLink(gameObject);
    
    }

    private void OnDestroy()
    {
        rotateTween?.Kill();
        scaleTween?.Kill();
        shakeSequence?.Kill();
    }
}
