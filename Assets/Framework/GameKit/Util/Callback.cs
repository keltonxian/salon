using UnityEngine;
using UnityEngine.EventSystems;

public class Callback
{
    public delegate void CallbackV();
    public delegate void CallbackI(int iArgu);
    public delegate void CallbackF(float fArgu);
    public delegate void CallbackB(bool bArgu);
    public delegate void CallbackMaterial(Material material);

    [System.Serializable]
    public class UnityEventV : UnityEngine.Events.UnityEvent { }
    [System.Serializable]
    public class UnityEventI : UnityEngine.Events.UnityEvent<int> { }
    [System.Serializable]
    public class UnityEventF : UnityEngine.Events.UnityEvent<float> { }
    [System.Serializable]
    public class UnityEventS : UnityEngine.Events.UnityEvent<string> { }
    [System.Serializable]
    public class UnityEventB : UnityEngine.Events.UnityEvent<bool> { }
    [System.Serializable]
    public class UnityEventPE : UnityEngine.Events.UnityEvent<PointerEventData> { }
}
