using UnityEngine;
using PureMVC.Interfaces;
using PureMVC.Patterns.Facade;
using PureMVC.Command;
using PureMVC.Const;

public class BeardCommandPlay : Command
{
    public override void Execute(INotification Notification)
    {
        Debug.Log("Beard Play");
        Facade.Instance.RemoveCommand(NotiConst.BEARD_PLAY);
        SendNotification(BeardViewMediator.NOTI_ENTER);
        SendNotification(BeardUIMediator.NOTI_ENTER);
    }
}
