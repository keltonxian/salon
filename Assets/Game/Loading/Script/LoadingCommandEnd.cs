using UnityEngine;
using PureMVC.Interfaces;
using PureMVC.Patterns.Facade;
using PureMVC.Command;
using PureMVC.Const;
using PureMVC.Manager;

public class LoadingCommandEnd : Command
{
    public override void Execute(INotification Notification)
    {
        Debug.Log("Loading End");
        Facade.Instance.RemoveCommand(NotiConst.LOADING_END);
        Facade.Instance.RemoveProxy(LoadingProxy.NAME);
        Facade.Instance.RemoveMediator(LoadingViewMediator.NAME);
        Facade.Instance.GetManager<GameManager>(ManagerName.Game).LoadNextSceneInLoading();
    }
}
