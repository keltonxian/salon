using UnityEngine;
using PureMVC.Interfaces;
using PureMVC.Patterns.Facade;
using PureMVC.Command;
using PureMVC.Const;

public class HomeCommandViewStart : Command
{
    public override void Execute(INotification Notification)
    {
        Debug.Log("Home View Start");
        Facade.Instance.RemoveCommand(NotiConst.HOME_VIEW_START);
        Facade.Instance.RegisterProxy(new HomeProxy());
        Facade.Instance.RegisterMediator(new HomeViewMediator(Notification.Body as HomeView));
    }
}
