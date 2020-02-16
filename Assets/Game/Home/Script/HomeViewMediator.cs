using PureMVC.Patterns.Mediator;
using PureMVC.Interfaces;

public class HomeViewMediator : Mediator
{
    public new static string NAME = "HomeViewMediator";

    public const string NOTI_ENTER = "View_Enter";

    private HomeProxy _homeProxy;
    private HomeView _homeView;

    public HomeViewMediator(object viewComponent = null) : base(NAME, viewComponent)
    {
        _homeView = viewComponent as HomeView;
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
        _homeProxy = Facade.RetrieveProxy(HomeProxy.NAME) as HomeProxy;
    }

    public override void OnRemove()
    {
        base.OnRemove();
    }

    public void ViewEnter()
    {
        _homeView.Enter();
    }

}
