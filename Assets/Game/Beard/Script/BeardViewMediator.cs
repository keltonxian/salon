using PureMVC.Patterns.Mediator;
using PureMVC.Interfaces;

public class BeardViewMediator : Mediator
{
    public new static string NAME = "BeardViewMediator";

    public const string NOTI_ENTER = "View_Enter";
    public const string NOTI_EXIT = "View_Exit";

    private BeardProxy _beardProxy;
    private BeardView _beardView;

    public BeardViewMediator(object viewComponent = null) : base(NAME, viewComponent)
    {
        _beardView = viewComponent as BeardView;
    }

    public override string[] ListNotificationInterests()
    {
        return new string[2] { NOTI_ENTER, NOTI_EXIT };
    }

    public override void HandleNotification(INotification notification)
    {
        switch (notification.Name)
        {
            case NOTI_ENTER:
                ViewEnter();
                break;
            case NOTI_EXIT:
                ViewExit();
                break;
        }
    }

    public override void OnRegister()
    {
        base.OnRegister();
        _beardProxy = Facade.RetrieveProxy(BeardProxy.NAME) as BeardProxy;
    }

    public override void OnRemove()
    {
        base.OnRemove();
    }

    public void ViewEnter()
    {
        _beardView.Enter();
    }

    public void ViewExit()
    {
        _beardView.Exit();
    }
}
