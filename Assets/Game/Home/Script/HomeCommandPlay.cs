using UnityEngine;
using PureMVC.Patterns.Facade;
using PureMVC.Interfaces;
using PureMVC.Command;
using PureMVC.Const;

public class HomeCommandPlay : Command
{
    public override void Execute(INotification Notification)
    {
        Debug.Log("Home Play");
        Facade.Instance.RemoveCommand(NotiConst.HOME_PLAY);
        SendNotification(HomeViewMediator.NOTI_ENTER);
    }
}
