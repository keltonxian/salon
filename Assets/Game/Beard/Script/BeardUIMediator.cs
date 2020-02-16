using PureMVC.Patterns.Mediator;
using PureMVC.Interfaces;

public class BeardUIMediator : Mediator
{
    public new static string NAME = "BeardUIMediator";

    public const string NOTI_ENTER = "UI_ENTER";

    private BeardProxy _beardProxy;
    private BeardUI _beardUI;

    public BeardUIMediator(object viewComponent = null) : base(NAME, viewComponent)
    {
        _beardUI = viewComponent as BeardUI;
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
        _beardProxy = Facade.RetrieveProxy(BeardProxy.NAME) as BeardProxy;
    }

    public override void OnRemove()
    {
        base.OnRemove();
    }

    public void ViewEnter()
    {
        _beardUI.Enter();
    }
}
