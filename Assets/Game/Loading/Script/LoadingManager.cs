using PureMVC.Patterns.Facade;
using PureMVC.Const;

public class LoadingManager : BaseScene
{
    public LoadingView _loadingView;

    public override void Init()
    {
        base.Init();
        Facade.Instance.RegisterCommand(NotiConst.LOADING_VIEW_START, () => new LoadingCommandViewStart());
        Facade.Instance.RegisterCommand(NotiConst.LOADING_END, () => new LoadingCommandEnd());
        Facade.Instance.RegisterCommand(NotiConst.LOADING_PLAY, () => new LoadingCommandPlay());
        Facade.Instance.SendMessageCommand(NotiConst.LOADING_VIEW_START, _loadingView);
        Facade.Instance.SendMessageCommand(NotiConst.LOADING_PLAY);
    }
}
