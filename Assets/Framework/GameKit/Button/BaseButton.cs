using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using PureMVC.Core;

public class BaseButton : Base
{
    public enum EffectType
    {
        NONE, SCALE, JELLY,
    }
    public EffectType _effectType = EffectType.JELLY;
    public float _clickInterval = 1f;
    protected float _timeWaitNextClick = 0f;
    private Sequence _anim = null;
    private float _scaleChange = 0.13f;
    private float _scaleTime = 1f;
    private Vector3 _markScale = Vector3.one;

    void Update()
    {
        if (_timeWaitNextClick > 0f)
        {
            _timeWaitNextClick -= Time.deltaTime;
        }
    }

    public virtual void Init()
    {
        _markScale = transform.localScale;
    }

    protected bool CheckCanClick ()
    {
        if (_timeWaitNextClick <= 0)
        {
            return true;
        }
        return false;
    }

    public void PlayTouchSound()
    {
        UIManager.PlaySoundBtn();
    }

    public void PlayTouchAnim(Callback.CallbackV callback)
    {
        StopTouchAnim();
        if (_effectType == EffectType.SCALE)
        {
            PlayScale(callback);
        }
        else if (_effectType == EffectType.JELLY)
        {
            PlayJelly(callback);
        }
        else
        {
            callback();
        }
    }

    public void StopTouchAnim()
    {
        if (null != _anim)
        {
            _anim.Kill();
            _anim = null;
        }
        transform.DOKill();
        transform.localScale = _markScale;
    }

    public void PlayScale(Callback.CallbackV callback)
    {
        float time = _scaleTime / 2;
        float offset = _scaleChange;
        Vector3 toScale = _markScale + new Vector3(offset, offset, 0);
        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DOScale(toScale, time));
        sequence.Append(transform.DOScale(_markScale, time));
        sequence.AppendCallback(() =>
        {
            _anim = null;
            callback();
        });
        _anim = sequence;
    }

    public void PlayJelly(Callback.CallbackV callback)
    {
        float time = _scaleTime / 4;
        float offset = _scaleChange;
        Vector3 fromScale = _markScale;
        Vector3 toScale1 = fromScale + new Vector3(offset, -offset, 0);
        Vector3 toScale2 = fromScale + new Vector3(-offset, offset, 0);
        Sequence sequence = DOTween.Sequence();
        if (fromScale.x * fromScale.y > 0)
        {
            sequence.Append(transform.DOScale(toScale1, time));
            sequence.Append(transform.DOScale(toScale2, time));
        }
        else
        {
            sequence.Append(transform.DOScale(toScale2, time));
            sequence.Append(transform.DOScale(toScale1, time));
        }
        sequence.Append(transform.DOScale(_markScale, time));
        sequence.AppendCallback(() =>
        {
            _anim = null;
            callback();
        });
        _anim = sequence;
    }

}
