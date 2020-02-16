using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using PureMVC.Core;

public class BeardFaceScar : Base
{
    public ColliderTool _toolScraper = null;
    public ColliderTool _toolRazor = null;
    public ColliderTool _toolBandage = null;
    public ColliderTool _toolGrower = null;
    public GameObject _prefabBandage = null;
    public AudioClip _soundPutIn;
    public AudioClip _soundTakeOff;
    private Transform _fixArea;
    private BeardModelHandler _model;

    public void Init(BeardModelHandler model)
    {
        _fixArea = model._fixArea;
        _model = model;
    }

    public void TriggerScar(Collider2D collider, bool isCut, ColliderTool tool = null)
    {
        if (!collider)
        {
            return;
        }
        RectTransform scar = collider.GetComponent<RectTransform>();
        if (true == isCut && false == _toolScraper.GetColliderTriggerState(collider))
        {
            _toolBandage.SetColliderTriggerState(collider, false);
            _toolScraper.SetColliderTriggerState(collider, true);
            _toolRazor.SetColliderTriggerState(collider, true);
            _toolGrower.SetColliderTriggerState(collider, false);
            collider.enabled = false;
            _model.InsertSad();
            Image image = scar.GetComponent<Image>();
            image.color = new Color(1, 1, 1, 0);
            image.enabled = true;
            image.DOFade(1, 0.1f).SetEase(Ease.Linear);
        }
        else if (false == isCut && false == _toolBandage.GetColliderTriggerState(collider))
        {
            _toolBandage.SetColliderTriggerState(collider, true);
            _toolScraper.SetColliderTriggerState(collider, false);
            _toolRazor.SetColliderTriggerState(collider, false);
            _toolGrower.SetColliderTriggerState(collider, true);
            scar.GetComponent<Image>().enabled = false;
            if (null != tool)
            {
                tool._drag.SetEnabled(false);
                tool._drag.BackPosition(UGUIDrag.DragBackEffect.TweenScale);
                GameObject bandage = Instantiate(_prefabBandage);
                bandage.transform.SetParent(_fixArea);
                bandage.transform.position = scar.transform.position;
                bandage.transform.localScale = scar.transform.localScale;
                AudioManager.PlaySound(_soundPutIn);
                Sequence sequence = DOTween.Sequence();
                sequence.AppendInterval(0.5f);
                sequence.AppendCallback(() =>
                {
                    tool._drag.SetEnabled(true);
                });
                sequence.AppendInterval(1.5f);
                sequence.AppendCallback(() => {
                    AudioManager.PlaySound(_soundTakeOff);
                });
                sequence.Append(bandage.GetComponent<Image>().DOFade(0f, 0.8f).SetEase(Ease.Linear));
                sequence.OnComplete(() =>
                {
                    DestroyImmediate(bandage, true);
                });
            }
        }
    }
}
