using UnityEngine;
using PureMVC.Interfaces;
using PureMVC.Patterns.Facade;
using PureMVC.Command;
using PureMVC.Const;

public class LoadingCommandViewStart : Command
{
    public override void Execute(INotification Notification)
    {
        Debug.Log("Loading View Start");
        Facade.Instance.RemoveCommand(NotiConst.LOADING_VIEW_START);
        Facade.Instance.RegisterProxy(new LoadingProxy());
        Facade.Instance.RegisterMediator(new LoadingViewMediator(Notification.Body as LoadingView));
    }
}
