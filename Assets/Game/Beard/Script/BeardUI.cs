using UnityEngine;
using PureMVC.Patterns.Facade;
using PureMVC.Const;

public class BeardUI : MonoBehaviour
{
    public void Enter()
    {

    }

    public void OnClickHome()
    {
        Facade.Instance.SendMessageCommand(NotiConst.BEARD_END, "SceneHome");
    }

    public void OnClickNext()
    {
    }

}
