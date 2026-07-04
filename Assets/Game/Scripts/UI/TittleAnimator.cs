using DG.Tweening;
using UnityEngine;

public class TitleAnimator : MonoBehaviour
{
    private Tween scaleTween;

    void Start()
    {
        scaleTween = transform
            .DORotate(new Vector3(0,0,3),1.2f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1,LoopType.Yoyo)
            .SetLink(gameObject);;
        
        scaleTween = transform
            .DOScale(0.08f,0.8f)
            .SetLoops(-1,LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetLink(gameObject);;

        Sequence seq = DOTween.Sequence();

        seq.AppendInterval(3f);

        seq.Append(transform.DOShakeRotation(
            0.25f,
            4f,
            20));

        seq.SetLoops(-1).SetLink(gameObject);
    
    }

    private void OnDestroy()
    {
        scaleTween?.Kill();
    }
}