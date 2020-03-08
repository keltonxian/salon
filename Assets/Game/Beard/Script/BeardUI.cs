using UnityEngine;
using PureMVC.Patterns.Facade;
using PureMVC.Const;
using PureMVC.Core;
using PureMVC.Manager;

public class BeardUI : Base
{
    LockItemHandler _lockItemHandler = null;

    public void Enter()
    {
        
    }

    public void OnClickHome()
    {
        Facade.Instance.SendMessageCommand(NotiConst.BEARD_END, "SceneHome");
    }

    public void OnClickShop()
    {
        UIManager.ShowShop((LockManager.IAP_TYPE iapType) =>
        {
            _lockItemHandler.IapUnlock(iapType, (GameObject[] arrayLockItem) =>
            {
                //CheckHideShop(arrayLockItem);
                //ShowIapUnlockAction(arrayLockItem);
            });
        });
    }

    public void OnClickNext()
    {
    }

}
