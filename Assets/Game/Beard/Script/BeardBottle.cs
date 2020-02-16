using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BeardBottle : MonoBehaviour
{
    public RectTransform _cap = null;
    public RectTransform _bottle = null;
    public float _capAnimTime = 0.3f;
    private Vector2 _markCapPos;
    private Vector3 _markCapRotate;
    private Sequence _animCap = null;

    void Start()
    {
        _markCapPos = _cap.anchoredPosition;
        _markCapRotate = _cap.localEulerAngles;
    }

    public void SetBottleUse(bool isUse)
    {
        if (null != _animCap)
        {
            _animCap.Kill();
        }
        Sequence seq = DOTween.Sequence();
        if (true == isUse)
        {
            seq.Append(_cap.DOAnchorPos(_markCapPos + new Vector2(_cap.sizeDelta.x / 2, -(_bottle.sizeDelta.y - _cap.sizeDelta.y / 2 - 20)), _capAnimTime));
            seq.Join(_cap.DOLocalRotate(_markCapRotate + new Vector3(0, 0, -180), _capAnimTime, RotateMode.Fast));
        }
        else
        {
            seq.Append(_cap.DOAnchorPos(_markCapPos, _capAnimTime));
            seq.Join(_cap.DOLocalRotate(_markCapRotate, _capAnimTime, RotateMode.Fast));
        }
        seq.AppendCallback(() =>
        {
            _animCap = null;
        });
        _animCap = seq;
    }
}
