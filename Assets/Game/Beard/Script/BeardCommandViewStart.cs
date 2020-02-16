using UnityEngine;
using PureMVC.Interfaces;
using PureMVC.Patterns.Facade;
using PureMVC.Command;
using PureMVC.Const;

public class BeardCommandViewStart : Command
{
    public override void Execute(INotification Notification)
    {
        Debug.Log("Beard View Start");
        Facade.Instance.RemoveCommand(NotiConst.BEARD_VIEW_START);
        Facade.Instance.RegisterProxy(new BeardProxy());
        Facade.Instance.RegisterMediator(new BeardViewMediator(Notification.Body as BeardView));
    }
}
