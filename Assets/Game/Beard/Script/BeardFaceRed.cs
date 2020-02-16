using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using PureMVC.Core;

public class BeardFaceRed : Base
{
    public ColliderTool _toolScraper = null;
    public ColliderTool _toolRazor = null;
    public ColliderTool _toolUnguent = null;
    public ColliderTool _toolGrower = null;
    public GameObject _prefabCream = null;
    public AudioClip _soundPutIn;
    private Transform _fixArea;
    private BeardModelHandler _model;

    public void Init(BeardModelHandler model)
    {
        _fixArea = model._fixArea;
        _model = model;
    }

    public void TriggerRed(Collider2D collider, bool isCut, ColliderTool tool = null)
    {
        if (!collider)
        {
            return;
        }
        RectTransform red = collider.GetComponent<RectTransform>();
        if (true == isCut && false == _toolScraper.GetColliderTriggerState(collider))
        {
            _toolUnguent.SetColliderTriggerState(collider, false);
            _toolScraper.SetColliderTriggerState(collider, true);
            _toolRazor.SetColliderTriggerState(collider, true);
            _toolGrower.SetColliderTriggerState(collider, false);
            collider.enabled = false;
            _model.InsertSad();
            Image image = red.GetComponent<Image>();
            image.color = new Color(1, 1, 1, 0);
            image.enabled = true;
            image.DOFade(1, 0.1f).SetEase(Ease.Linear);
        }
        else if (false == isCut && false == _toolUnguent.GetColliderTriggerState(collider))
        {
            _toolUnguent.SetColliderTriggerState(collider, true);
            _toolScraper.SetColliderTriggerState(collider, false);
            _toolRazor.SetColliderTriggerState(collider, false);
            _toolGrower.SetColliderTriggerState(collider, true);
            red.GetComponent<Image>().enabled = false;
            if (null != tool)
            {
                GameObject cream = Instantiate(_prefabCream);
                cream.transform.SetParent(_fixArea);
                cream.transform.position = red.transform.position;
                cream.transform.localScale = red.transform.localScale;
                AudioManager.PlaySound(_soundPutIn);
                Image image = cream.GetComponent<Image>();
                Sequence sequence = DOTween.Sequence();
                sequence.AppendInterval(2f);
                sequence.Append(cream.GetComponent<Image>().DOFade(0f, 0.8f).SetEase(Ease.Linear));
                sequence.OnComplete(() =>
                {
                    DestroyImmediate(cream, true);
                });
            }
        }
    }
}
