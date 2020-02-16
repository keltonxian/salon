using PureMVC.Patterns.Facade;
using PureMVC.Const;

public class HomeManager : BaseScene
{
    public HomeView _homeView;

    public override void Init ()
    {
        base.Init();
        Facade.Instance.RegisterCommand(NotiConst.HOME_VIEW_START, () => new HomeCommandViewStart());
        Facade.Instance.RegisterCommand(NotiConst.HOME_END, () => new HomeCommandEnd());
        Facade.Instance.RegisterCommand(NotiConst.HOME_PLAY, () => new HomeCommandPlay());
        Facade.Instance.SendMessageCommand(NotiConst.HOME_VIEW_START, _homeView);
        Facade.Instance.SendMessageCommand(NotiConst.HOME_PLAY);
    }
}
