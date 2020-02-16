using UnityEngine;
using PureMVC.Patterns.Facade;
using PureMVC.Interfaces;
using PureMVC.Command;
using PureMVC.Const;

public class LoadingCommandPlay : Command
{
    public override void Execute(INotification Notification)
    {
        Debug.Log("Loading Play");
        Facade.Instance.RemoveCommand(NotiConst.LOADING_PLAY);
        SendNotification(LoadingViewMediator.NOTI_ENTER);
    }
}
