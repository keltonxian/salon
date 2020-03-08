using UnityEngine;
using System.Runtime.InteropServices;

public class SharePlugin
{
    /* Interface to native implementation */
#if UNITY_IOS
    [DllImport("__Internal")]
    private static extern void _SetGameObjectNameForShare(string gameObjectName);

    [DllImport("__Internal")]
    private static extern void _ShareToAll(string message, string imagePath);

    [DllImport("__Internal")]
    private static extern void _ShareToEmail(string subject, string message, string imagePath);

    [DllImport("__Internal")]
    private static extern void _ShareToFacebook(string message, string imagePath);

    [DllImport("__Internal")]
    private static extern void _ShareToTwitter(string message, string imagePath);

    [DllImport("__Internal")]
    private static extern void _SaveImage(string path, byte[] imageBytes, int length);
#endif

    private string _gameObjectName = null;

    public void SetGameObjectNameForShare(string gameObjectName)
    {
        //add by tangjl 
        _gameObjectName = gameObjectName;
        // Call plugin only when running on real device
        if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
        {
#if UNITY_ANDROID
			SSCInternalAdapter.getInstance().setGameObjName(gameObjectName);
#elif UNITY_IOS
            _SetGameObjectNameForShare(gameObjectName);
#endif
        }
    }

    public void ShareToAll(string message, string imagePath)
    {
        // Call plugin only when running on real device
        if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
        {
#if UNITY_ANDROID
			SSCInternalAdapter.getInstance().sendEmailAndFilePic("",message,imagePath);
#elif UNITY_IOS
            _ShareToAll(message, imagePath);
#endif
        }
    }

    public void ShareToEmail(string subject, string message, string imagePath)
    {
        // Call plugin only when running on real device
        if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
        {
#if UNITY_ANDROID
			SSCInternalAdapter.getInstance().sendEmailAndFilePic(subject,message,imagePath);
#elif UNITY_IOS
            _ShareToEmail(subject, message, imagePath);
#endif
        }
    }

    public void ShareToFacebook(string message, string imagePath)
    {
        // Call plugin only when running on real device
        if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
        {
#if UNITY_ANDROID
			SSCInternalAdapter.getInstance().sendEmailAndFilePic("",message,imagePath);
#elif UNITY_IOS
            _ShareToFacebook(message, imagePath);
#endif
        }
    }

    public void ShareToTwitter(string message, string imagePath)
    {
        // Call plugin only when running on real device
        if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
        {
#if UNITY_ANDROID
			SSCInternalAdapter.getInstance().sendEmailAndFilePic("",message,imagePath);
#elif UNITY_IOS
            _ShareToTwitter(message, imagePath);
#endif
        }
    }
    /// <summary>
    /// Saves the image.
    /// </summary>
    /// <returns>The image full path .图片保存的全路径，仅在android平台返回有效值，如果返回null，图片保存失败。</returns>
    /// <param name="path">Path.图片保存的子目录，该参数仅对android 有效</param>
    /// <param name="imageBytes">Image bytes.</param>
    /// <param name="length">Length.</param>
    public string SaveImage(string path, byte[] imageBytes, int length)
    {
        // Call plugin only when running on real device
        if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
        {
#if UNITY_ANDROID
			string fullpath = SSCInternalAdapter.getInstance().saveImageToAlbum(path,imageBytes);
			if(!string.IsNullOrEmpty(_gameObjectName) && GameObject.Find(_gameObjectName)!=null){
				if(fullpath != null){
					//直接回调通知
					GameObject.Find(_gameObjectName).SendMessage("OnSaveImageSuccessed","saved successfully");
				}else{
					GameObject.Find(_gameObjectName).SendMessage("OnSaveImageFailed","saved failed");
				}
			}
			return fullpath;
#elif UNITY_IOS
            _SaveImage(path, imageBytes, length);
            return "";
#endif
        }
        return null;
    }
}
