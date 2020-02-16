using PureMVC.Patterns.Mediator;
using PureMVC.Interfaces;

public class LoadingViewMediator : Mediator
{
    public new static string NAME = "LoadingViewMediator";

    public const string NOTI_ENTER = "View_Enter";

    private LoadingProxy _loadingProxy;
    private LoadingView _roleMaleView;

    public LoadingViewMediator(object viewComponent = null) : base(NAME, viewComponent)
    {
        _roleMaleView = viewComponent as LoadingView;
    }

    public override string[] ListNotificationInterests()
    {
        return new string[1] { NOTI_ENTER };
    }

    public override void HandleNotification(INotification notification)
    {
        switch (notification.Name)
        {
            case NOTI_ENTER:
                ViewEnter();
                break;
        }
    }

    public override void OnRegister()
    {
        base.OnRegister();
        _loadingProxy = Facade.RetrieveProxy(LoadingProxy.NAME) as LoadingProxy;
    }

    public override void OnRemove()
    {
        base.OnRemove();
    }

    public void ViewEnter()
    {
        _roleMaleView.Enter();
    }
}
