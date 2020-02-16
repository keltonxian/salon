using UnityEngine;
using PureMVC.Interfaces;
using PureMVC.Patterns.Facade;
using PureMVC.Command;
using PureMVC.Const;

public class BeardCommandUIStart : Command
{
    public override void Execute(INotification Notification)
    {
        Debug.Log("Beard UI Start");
        Facade.Instance.RemoveCommand(NotiConst.BEARD_UI_START);
        Facade.Instance.RegisterMediator(new BeardUIMediator(Notification.Body as BeardUI));
    }
}
