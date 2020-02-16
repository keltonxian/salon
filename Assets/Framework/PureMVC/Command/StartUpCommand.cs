using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PureMVC.Interfaces;
using PureMVC.Const;
using PureMVC.Patterns.Facade;
using PureMVC.Manager;

namespace PureMVC.Command
{
    public class StartUpCommand : Command
    {
        public override void Execute(INotification Notification)
        {
            Debug.Log("Game Start Up");
            Facade.Instance.AddManager<GameManager>(ManagerName.Game);
            Facade.Instance.AddManager<ResourcesManager>(ManagerName.Resources);
            Facade.Instance.AddManager<AssetBundleManager>(ManagerName.AssetBundle);
            Facade.Instance.AddManager<AudioManager>(ManagerName.Audio);
            Facade.Instance.AddManager<DataManager>(ManagerName.Data);
            Facade.Instance.AddManager<NetworkManager>(ManagerName.Network);
            Facade.Instance.AddManager<UIManager>(ManagerName.UI);
            Facade.Instance.AddManager<EnhancedTextManager>(ManagerName.EnhancedText);
        }

        public override void SendNotification(string notificationName, object body = null, string type = null)
        {

        }
    }
}
