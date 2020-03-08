using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public enum PERMISSION : long
{
    kReadCalendar = 0x000001,
    kWriteCalendar = 0x000001 << 1,
    kCamera = 0x000001 << 2,

    kReadContacts = 0x000001 << 3,
    kWriteContacts = 0x000001 << 4,
    kGetAccounts = 0x000001 << 5,

    kAccessFineLocation = 0x000001 << 6,
    kAccessCoraseLocation = 0x000001 << 7,

    kRecordAudio = 0x000001 << 8,

    kReadPhoneState = 0x000001 << 9,
    kCallPhone = 0x000001 << 10,
    kReadCallLog = 0x000001 << 11,
    kWriteCallLog = 0x000001 << 12,
    kAddVoicemail = 0x000001 << 13,
    kUseSIP = 0x000001 << 14,
    kProcessOutgoingCalls = 0x000001 << 15,

    kBodySensors = 0x000001 << 16,

    kSendSMS = 0x000001 << 17,
    kReadSMS = 0x000001 << 18,
    kReceiveSMS = 0x000001 << 19,
    kReceiveWapPush = 0x000001 << 20,
    kReceiveMMS = 0x000001 << 21,

    kReadExternalStorage = 0x000001 << 22,
    kWriteExternalStorage = 0x000001 << 23
};

public class PermissionPlugin
{
    private static PermissionPlugin instance;
    private string _gameObjectName = null;

    public static PermissionPlugin getInstance()
    {
        if (instance == null)
            instance = new PermissionPlugin();
        return instance;
    }

    public void setGameObjectName(string name)
    {
        _gameObjectName = name;
        if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
        {
#if UNITY_ANDROID
			SSCPermissionAdapter.getInstance().setGameObjectName(name);
#elif UNITY_IOS
#endif
        }
    }

    public void requestRuntimePermissions(int requestCode, long permission)
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
        {
#if UNITY_ANDROID
			SSCPermissionAdapter.getInstance().requestRuntimePermissions(requestCode,permission);
#elif UNITY_IOS
            if (_gameObjectName != null)
            {

                GameObject.Find(_gameObjectName).SendMessage("onPermissionGranted", requestCode.ToString());

            }
#endif
        }
    }
}
