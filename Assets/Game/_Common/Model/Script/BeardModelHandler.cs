using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PureMVC.Core;
using DG.Tweening;

public class BeardModelHandler : Base
{
    public DragonBonesModel _emotion;
    public RectTransform _eye;
    public RectTransform _pupil;
    private Vector2 _markPupilPos = Vector2.zero;
    private Vector2 _limitPupilOffset = new Vector2(10f, 10f);
    public GameObject _beardMessy;
    public RenderTexturePainter _beardBackPainter;
    public RenderTexturePainter _beardMiddlePainter;
    public RenderTexturePainter _beardFrontPainter;
    public PainterChecker _beardBubbleChecker;
    public GameObject _beardFur;
    public GameObject _beardRed;
    public GameObject _beardScar;
    public Transform _fixArea;
    public RenderTexturePainter _hairMainPainter;
    public RenderTexturePainter _hairTempPainter;

    private string[] _arrayEmotionNameDefault = { "zhayan", "tp", "zhayan", "xiangxiakan", };
    private string[] _arrayEmotionNameHappy = { "1", "1", "1", "1", "1", "1", "daxiao", };
    private string[] _arrayEmotionNameEyeMove = { "1", "1", "1", "1", "1", "1", "zhayan", "zhayan", "xiangxiakan", };
    private List<string> _listEmotionPlayName = new List<string>();
    private List<string> _listEmotionName = new List<string>();

    private string _nextEmotion = null;
    private bool _emotionDone = false;
    private bool _isEyeMoving = false;
    private bool _isHasEyeMove = false;

    private const string NAME_SAD = "tongku";
    private bool _isPlayingSad = false;
    private bool _isShowingSad = false;

    public AudioClip[] _arrayVO;

    public void Init()
    {
        _isEyeMoving = false;
        _emotionDone = false;
        _isHasEyeMove = false;
        _isPlayingSad = false;
        _isShowingSad = false;
        _markPupilPos = _pupil.anchoredPosition;
        SetEmotionPlayNames(_arrayEmotionNameDefault);
        _eye.gameObject.SetActive(false);
    }

    public void StartEmotion()
    {
        _nextEmotion = "daxiao";
    }

    private void SetEmotionPlayNames(string[] names)
    {
        _listEmotionPlayName.Clear();
        for (int i = 0; i < names.Length; i++)
        {
            _listEmotionPlayName.Add(names[i]);
        }
        RefreshListIndex();
    }

    private void RefreshListIndex()
    {
        _listEmotionName.Clear();
        foreach (string eName in _listEmotionPlayName)
        {
            _listEmotionName.Add(eName);
        }
    }

    private void NextAnim()
    {
        if (null != _nextEmotion)
        {
            return;
        }
        if (0 == _listEmotionName.Count)
        {
            RefreshListIndex();
        }
        int ranIndex = 0;
        string eName = _listEmotionName[ranIndex];
        _listEmotionName.RemoveAt(ranIndex);
        _nextEmotion = eName;
    }

    private bool PlayNextAnim()
    {
        if (string.IsNullOrEmpty(_nextEmotion))
        {
            return false;
        }
        if ("1" == _nextEmotion)
        {
            _eye.gameObject.SetActive(true);
            _isEyeMoving = true;
        }
        else
        {
            _eye.gameObject.SetActive(false);
            _isEyeMoving = false;
        }
        if (_nextEmotion == NAME_SAD)
        {
            _isPlayingSad = true;
        }
        _emotion.PlayAnimation(_nextEmotion, () =>
        {
            float delay = 1.3f;
            if (true == _isHasEyeMove)
            {
                delay = 0.1f;
            }
            Sequence sequence = DOTween.Sequence();
            sequence.PrependInterval(delay);
            sequence.AppendCallback(() => {
                _emotionDone = true;
                if (_isPlayingSad)
                {
                    _isPlayingSad = false;
                    _isShowingSad = false;
                }
            });
        });
        PlayVo(_nextEmotion);
        _nextEmotion = null;
        return true;
    }

    private void PlayVo(string emotionName)
    {
        AudioClip vo = null;
        if (emotionName == "tp")
        {
            vo = _arrayVO[0];
        }
        else if (emotionName == "tongku")
        {
            vo = _arrayVO[1];
        }
        else if (emotionName == "xiangxiakan")
        {
            vo = _arrayVO[2];
        }
        else if (emotionName == "daxiao")
        {
            vo = _arrayVO[3];
        }

        if (null == vo)
        {
            return;
        }
        AudioManager.PlaySound(vo);
    }

    void Update()
    {
        if (true == _emotionDone)
        {
            _emotionDone = false;
            NextAnim();
            return;
        }
        if (true == PlayNextAnim())
        {
            if (false == _isEyeMoving)
            {
                return;
            }
        }
        if (true == _isEyeMoving)
        {
            if (Input.GetMouseButton(0))
            {
                //Vector3 inputPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                //float toX = inputPos.x / 50f;
                //float toY = inputPos.y / 50f;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_eye.GetComponent<RectTransform>(), Input.mousePosition, Camera.main, out Vector2 inputPos);
                float toX = inputPos.x / 50f;
                float toY = inputPos.y / 50f;
                if (toX < -_limitPupilOffset.x)
                {
                    toX = -_limitPupilOffset.x;
                }
                if (toX > _limitPupilOffset.x)
                {
                    toX = _limitPupilOffset.x;
                }
                if (toY < -_limitPupilOffset.y)
                {
                    toY = -_limitPupilOffset.y;
                }
                if (toY > _limitPupilOffset.y)
                {
                    toY = _limitPupilOffset.y;
                }
                _pupil.transform.localPosition = _markPupilPos + new Vector2(toX, toY);
            }
            return;
        }
    }

    public void SetShowDefault(bool isShow)
    {
        _isHasEyeMove = !isShow;
        SetEmotionPlayNames(_arrayEmotionNameDefault);
    }

    public void SetShowHappy(bool isShow)
    {
        _isHasEyeMove = isShow;
        if (false == isShow)
        {
            SetShowDefault(true);
            return;
        }
        SetEmotionPlayNames(_arrayEmotionNameHappy);
    }

    public void SetShowEyeMove(bool isShow)
    {
        _isHasEyeMove = isShow;
        if (false == isShow)
        {
            SetShowDefault(true);
            return;
        }
        SetEmotionPlayNames(_arrayEmotionNameEyeMove);
    }

    public void InsertSad()
    {
        if (_isShowingSad)
        {
            return;
        }
        _isShowingSad = true;
        _emotionDone = true;
        _listEmotionName.Insert(0, NAME_SAD);
    }
}
