using UnityEngine;
using UnityEngine.UI;
using PureMVC.Core;
using DG.Tweening;

public class BeardToolCream : Base
{
    private ColliderTool _toolCollider = null;
    public ColliderTool ToolCollider
    {
        get
        {
            return _toolCollider;
        }
    }

    private BeardModelHandler _model;
    private Transform _fixArea;
    public GameObject _prefabCream = null;
    public AudioClip _soundPutIn;

    public void Init(BeardModelHandler model)
    {
        _toolCollider = GetComponent<ColliderTool>();
        _toolCollider.Init(new GameObject[] { model._beardRed, model._beardScar });
        _model = model;
        _fixArea = model._fixArea;
        ToolCollider.OnToolStart.AddListener(OnToolStart);
        ToolCollider.OnToolEnd.AddListener(OnToolEnd);
    }

    private void OnToolStart()
    {
        _model.SetShowHappy(true);
    }

    private void OnToolEnd()
    {
        _model.SetShowHappy(false);
    }

    public void OnCollisionHover()
    {
        DoTrigger(ToolCollider.CurrentCollider);
    }

    private void DoTrigger(Collider2D collider2d)
    {
        if (!collider2d)
        {
            return;
        }
        if (false == ToolCollider.GetColliderTriggerState(collider2d))
        {
            RectTransform target = collider2d.GetComponent<RectTransform>();
            ToolCollider.SetColliderTriggerState(collider2d, true);
            GameObject cream = Instantiate(_prefabCream);
            cream.transform.SetParent(_fixArea);
            cream.transform.position = target.transform.position;
            cream.transform.localScale = target.transform.localScale;
            AudioManager.PlaySound(_soundPutIn);
            Sequence sequence = DOTween.Sequence();
            sequence.Append(cream.GetComponent<Image>().DOFade(0f, 2f).SetEase(Ease.Linear));
            sequence.OnComplete(() =>
            {
                ToolCollider.SetColliderTriggerState(collider2d, false);
                DestroyImmediate(cream, true);
            });
        }
    }
}
