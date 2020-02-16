using UnityEngine;
using DG.Tweening;

public class SimpleAnim
{
    public static Sequence DoJelly(Transform target, Vector3 defaultScale, float scaleTime = 0.15f, float scaleOffset = 0.1f)
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(target.DOScale(defaultScale + new Vector3(scaleOffset, 0f, 0f), scaleTime).SetEase(Ease.Linear));
        sequence.Append(target.DOScale(defaultScale + new Vector3(0f, scaleOffset, 0f), scaleTime).SetEase(Ease.Linear));
        sequence.Append(target.DOScale(defaultScale, scaleTime).SetEase(Ease.Linear));
        sequence.SetLoops(-1);
        return sequence;
    }
}
