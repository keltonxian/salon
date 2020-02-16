using System.Collections;
using UnityEngine;
using PureMVC.Patterns.Facade;

public class BaseScene : MonoBehaviour
{
    void Start()
    {
        Init();
    }

    public virtual void Init()
    {
        Facade.Instance.StartUp();
    }
}
