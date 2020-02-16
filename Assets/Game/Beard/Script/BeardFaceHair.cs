using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BeardFaceHair : MonoBehaviour
{
    public ColliderTool _toolScissors = null;
    public ColliderTool _toolGrower = null;

    public void TriggerMessy(Collider2D collider, bool isCut)
    {
        if (!collider)
        {
            return;
        }
        RectTransform messy = collider.GetComponent<RectTransform>();
        if (true == isCut && false == _toolScissors.GetColliderTriggerState(collider))
        {
            _toolScissors.SetColliderTriggerState(collider, true);
            _toolGrower.SetColliderTriggerState(collider, false);
            collider.enabled = false;
            Vector2 pos = messy.anchoredPosition;
            Quaternion rotate = messy.localRotation;
            Sequence sequence = DOTween.Sequence();
            sequence.Append(messy.DOAnchorPosY(pos.y - 500, 1.2f).SetEase(Ease.Linear));
            sequence.Join(messy.DOLocalRotate(new Vector3(0, 0, 100), 1.2f, RotateMode.Fast));
            sequence.OnComplete(() =>
            {
                messy.GetComponent<Image>().enabled = false;
                messy.localRotation = rotate;
                messy.anchoredPosition = pos;
            });
        }
        else if (false == isCut && false == _toolGrower.GetColliderTriggerState(collider))
        {
            _toolScissors.SetColliderTriggerState(collider, false);
            _toolGrower.SetColliderTriggerState(collider, true);
            collider.enabled = false;
            Image image = messy.GetComponent<Image>();
            image.enabled = true;
            Color color = image.color;
            image.color = new Color(color.r, color.g, color.b, 0f);
            image.DOFade(1f, 0.3f).SetEase(Ease.Linear);
        }
    }
}
