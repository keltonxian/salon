using UnityEngine;
using PureMVC.Interfaces;
using PureMVC.Patterns.Facade;
using PureMVC.Command;
using PureMVC.Const;
using PureMVC.Manager;

public class BeardCommandEnd : Command
{
    public override void Execute(INotification Notification)
    {
        Debug.Log("Beard End");
        Facade.Instance.RemoveCommand(NotiConst.BEARD_END);
        Facade.Instance.RemoveProxy(BeardProxy.NAME);
        Facade.Instance.RemoveMediator(BeardViewMediator.NAME);
        Facade.Instance.RemoveMediator(BeardUIMediator.NAME);
        Facade.Instance.GetManager<GameManager>(ManagerName.Game).LoadNextScene(Notification.Body as string);
    }
}
