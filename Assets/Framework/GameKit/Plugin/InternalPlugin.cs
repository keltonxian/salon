using UnityEngine;
using System.Runtime.InteropServices;

public class InternalPlugin
{
    /* Interface to native implementation */
#if UNITY_IOS
    [DllImport("__Internal")]
    private static extern void _setGameObjectNameForInternal(string gameObjectName);

    [DllImport("__Internal")]
    private static extern void _popAlertDialog(string message);

    [DllImport("__Internal")]
    private static extern void _showLoadingView();

    [DllImport("__Internal")]
    private static extern void _removeLoadingView();
#endif

    private string _gameObjectName;
    public void setGameObjectNameForInternal(string gameObjectName)
    {
        _gameObjectName = gameObjectName;
        // Call plugin only when running on real device
        if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
        {
#if UNITY_ANDROID
			SSCInternalAdapter.getInstance().setGameObjName(gameObjectName);
#elif UNITY_IOS
            _setGameObjectNameForInternal(gameObjectName);
#endif
        }
    }

    public void popAlertDialog(string message)
    {
        // Call plugin only when running on real device
        if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
        {
#if UNITY_ANDROID
			SSCInternalAdapter.getInstance().popAlertDialog(message);
#elif UNITY_IOS
            _popAlertDialog(message);
#endif
        }
    }

    public void showLoadingView()
    {
        // Call plugin only when running on real device
        if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
        {
#if UNITY_ANDROID
			SSCInternalAdapter.getInstance().showLoadingView();
#elif UNITY_IOS
            _showLoadingView();
#endif
        }
    }

    public void removeLoadingView()
    {
        // Call plugin only when running on real device
        if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
        {
#if UNITY_ANDROID
			SSCInternalAdapter.getInstance().removeLoadingView();
#elif UNITY_IOS
            _removeLoadingView();
#endif
        }
    }
}
