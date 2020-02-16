using UnityEngine;
using DG.Tweening;
using PureMVC.Patterns.Facade;
using PureMVC.Const;
using PureMVC.Core;
using PureMVC.Manager;

public class HomeView : Base
{
    public AudioClip _bgm;
    public RectTransform _modelLeftRT;
    private Vector2 _markModelLeftPos;
    public RectTransform _modelRightRT;
    private Vector2 _markModelRightPos;
    public RectTransform _modelBottomRT;
    private Vector2 _markModelBottomPos;
    public BeardModelHandler _modelLeft;
    public BeardModelHandler _modelRight;
    public BeardModelHandler _modelBottom;
    public RectTransform _nameLeft;
    private float _markNameLeftScale;
    public RectTransform _nameRight;
    private float _markNameRightScale;
    public RectTransform _nameBottom;
    private float _markNameBottomScale;

    public RectTransform _titleRT;
    private Vector2 _markTitlePos;
    private float _markTitleScale;

    void Awake()
    {
        _markModelLeftPos = _modelLeftRT.anchoredPosition;
        _modelLeftRT.anchoredPosition = new Vector2(-800f, _markModelLeftPos.y);
        _markModelRightPos = _modelRightRT.anchoredPosition;
        _modelRightRT.anchoredPosition = new Vector2(800f, _markModelRightPos.y);
        _markModelBottomPos = _modelBottomRT.anchoredPosition;
        _modelBottomRT.anchoredPosition = new Vector2(_markModelBottomPos.x, -1000f);
        _modelLeft.Init();
        _modelRight.Init();
        _modelBottom.Init();

        _markTitlePos = _titleRT.anchoredPosition;
        _titleRT.anchoredPosition = new Vector2(_markTitlePos.x, 1000f);
        _markTitleScale = _titleRT.localScale.x;

        _markNameLeftScale = _nameLeft.localScale.x;
        _nameLeft.localScale = Vector3.zero;
        _markNameRightScale = _nameRight.localScale.x;
        _nameRight.localScale = Vector3.zero;
        _markNameBottomScale = _nameBottom.localScale.x;
        _nameBottom.localScale = Vector3.zero;
    }

    public void Enter ()
    {
        AudioManager.PlayBGM(_bgm);
        Sequence seq = DOTween.Sequence();
        seq.Append(_modelLeftRT.DOAnchorPos(_markModelLeftPos, 0.6f).SetEase(Ease.OutBack));
        seq.AppendCallback(() =>
        {
            _modelLeft.StartEmotion();
        });
        seq.Append(_modelRightRT.DOAnchorPos(_markModelRightPos, 0.6f).SetEase(Ease.OutBack));
        seq.AppendCallback(() =>
        {
            _modelRight.StartEmotion();
        });
        seq.Append(_modelBottomRT.DOAnchorPos(_markModelBottomPos, 0.6f).SetEase(Ease.OutBack));
        seq.AppendCallback(() =>
        {
            _modelBottom.StartEmotion();
        });
        seq.Append(_titleRT.DOAnchorPos(_markTitlePos, 0.45f).SetEase(Ease.OutBack));
        seq.Join(_titleRT.DOScaleY(_markTitleScale * 1.2f, 0.45f).SetEase(Ease.OutBack));
        seq.Append(_titleRT.DOScale(_markTitleScale, 0.2f).SetEase(Ease.Linear));
        seq.Append(_nameLeft.DOScale(_markNameLeftScale, 0.3f).SetEase(Ease.OutBack));
        seq.Join(_nameRight.DOScale(_markNameRightScale, 0.3f).SetEase(Ease.OutBack));
        seq.Join(_nameBottom.DOScale(_markNameBottomScale, 0.3f).SetEase(Ease.OutBack));
    }

    public void OnClickRoleLeft()
    {
        GameManager.Role = GameManager.RoleType.Kobe;
        GoNext();
    }

    public void OnClickRoleRight()
    {
        GameManager.Role = GameManager.RoleType.Tony;
        GoNext();
    }

    public void OnClickRoleBottom()
    {
        GameManager.Role = GameManager.RoleType.Kelton;
        GoNext();
    }

    private void GoNext()
    {
        Facade.Instance.SendMessageCommand(NotiConst.HOME_END, "SceneBeard");
    }

}
