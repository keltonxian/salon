using UnityEngine;
using PureMVC.Interfaces;
using PureMVC.Patterns.Facade;
using PureMVC.Command;
using PureMVC.Const;
using PureMVC.Manager;

public class HomeCommandEnd : Command
{
    public override void Execute(INotification Notification)
    {
        Debug.Log("Home End");
        Facade.Instance.RemoveCommand(NotiConst.HOME_END);
        Facade.Instance.RemoveProxy(HomeProxy.NAME);
        Facade.Instance.RemoveMediator(HomeViewMediator.NAME);
        Facade.Instance.GetManager<GameManager>(ManagerName.Game).LoadNextScene(Notification.Body as string);
    }
}
