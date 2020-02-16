using PureMVC.Patterns.Facade;
using PureMVC.Const;

public class BeardManager : BaseScene
{
    public BeardView _beardView;
    public BeardUI _beardUI;

    public override void Init()
    {
        base.Init();
        Facade.Instance.RegisterCommand(NotiConst.BEARD_VIEW_START, () => new BeardCommandViewStart());
        Facade.Instance.RegisterCommand(NotiConst.BEARD_UI_START, () => new BeardCommandUIStart());
        Facade.Instance.RegisterCommand(NotiConst.BEARD_END, () => new BeardCommandEnd());
        Facade.Instance.RegisterCommand(NotiConst.BEARD_PLAY, () => new BeardCommandPlay());
        Facade.Instance.SendMessageCommand(NotiConst.BEARD_VIEW_START, _beardView);
        Facade.Instance.SendMessageCommand(NotiConst.BEARD_UI_START, _beardUI);
        Facade.Instance.SendMessageCommand(NotiConst.BEARD_PLAY);
    }
}
