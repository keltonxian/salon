using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace PureMVC.Manager
{
    public class PluginManager : Manager
    {
        private const string AdUnlockNoNetwork = "Ad_Unlock_NoNetwork";
        private const string AdLoadProblem = "Ads_loading_problem";
        private const string AlreadyPurchased = "Already_Purchased";
        private const string PurchaseNoNetwork = "Purchase_No_Network";
        private const string PurchaseFail = "Purchase_Fail";
        private const string PurchaseSuccess = "Purchase_Success";
        private const string PurchaseCancel = "Purchase_Cancel";
        private const string RestoreFail = "Restore_Fail";
        private const string NoRestore = "No_Restore";
        private const string RestoreSuccess = "Restore_Success";
        private const string PurchaseNotReady = "Purchase_Not_Ready";

        public bool IsNetworkAvailable
        {
            get
            {
                return Application.internetReachability != NetworkReachability.NotReachable;
            }
        }

        public bool IsWifi
        {
            get
            {
                return Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
            }
        }

        private string GetLocalization(string key)
        {
            //if (_localization.Count == 0)
            //{
            //    if (supportLocalization)
            //    {
            //        SetLanguage(Application.systemLanguage.ToString());
            //    }
            //    else
            //    {
            //        SetLanguage("English");
            //    }
            //}
            //if (_localization.ContainsKey(key))
            //{
            //    return _localization[key];
            //}
            //return "";
            return key;
        }

        #region Permissions
        /// <summary>
        /// 保存图片权限申请结果回调.(Android用，IOS调用申请权限，SDK都是啥也没做，直接返回申请成功)
        /// </summary>
        public System.Action<bool, string> OnPermissionAction;
        /// <summary>
        /// 运行时申请权限实例(write external for android)
        /// </summary>
        private PermissionPlugin _permissiom;
        public PermissionPlugin permissiom
        {
            get
            {
                if (_permissiom == null)
                {
                    _permissiom = new PermissionPlugin();
                    _permissiom.setGameObjectName(gameObject.name);
                }
                return _permissiom;
            }
        }
        /// <summary>
        /// Android申请WriteExternalStorage权限(安卓6.0以后可动态申请权限，不仅仅是在安装时)
        /// 如果你要申请特殊的权限，可以用 permissiom.requestRuntimePermissions 方法
        /// </summary>
        public void RequestPermission()
        {
#if UNITY_ANDROID
		permissiom.requestRuntimePermissions(100,(long)PERMISSION.kWriteExternalStorage);
#else
            if (OnPermissionAction != null) OnPermissionAction.Invoke(true, "-1");
#endif
        }

        //回调，成功
        void onPermissionGranted(string requestCode)
        {
            if (OnPermissionAction != null) OnPermissionAction.Invoke(true, requestCode);
        }
        //回调，失败
        void onPermissionDenied(string requestCode)
        {
            if (OnPermissionAction != null) OnPermissionAction.Invoke(false, requestCode);
        }
        #endregion

        #region internal function
        private InternalPlugin _internalPlugin;
        public InternalPlugin internalPlugin
        {
            get
            {
                if (_internalPlugin == null)
                {
                    _internalPlugin = new InternalPlugin();
                    _internalPlugin.setGameObjectNameForInternal(gameObject.name);
                }
                return _internalPlugin;
            }
        }
        #endregion

        #region share
        [System.Serializable]
        class PickImageFinished
        {
            public string path = "";
        }
        /// <summary>
        /// 图片保存成功回调
        /// </summary>
        public event Action OnImageSaveSuccessed;
        /// <summary>
        /// 图片保存失败回调
        /// </summary>
        public event Action OnImageSaveFailed;
        /// <summary>
        /// 分享成功回调()
        /// </summary>
        public event Action OnShareDidSuccessed;
        /// <summary>
        /// 分享失败回调
        /// </summary>
        public event Action OnShareDidFailed;
        /// <summary>
        /// 当从相册中取出图片，参数为图片路径
        /// </summary>
        public Action<string> OnPickImageFromAlbum;
        /// <summary>
        /// 当从相册中取出图片，参数为图片路径
        /// </summary>
        public Action<Texture2D> OnPickTextureFromAlbum;

        private SharePlugin _sharePlugin;
        /// <summary>
        /// 分享插件实例(保存图片，发送邮件，分享twitter等)
        /// </summary>
        public SharePlugin sharePlugin
        {
            get
            {
                if (_sharePlugin == null)
                {
                    _sharePlugin = new SharePlugin();
                    _sharePlugin.SetGameObjectNameForShare(gameObject.name);
                }
                return _sharePlugin;
            }
        }

        void OnShareSuccessed(string message)
        {
            if (OnShareDidSuccessed != null) OnShareDidSuccessed.Invoke();
        }

        void OnShareFailed(string message)
        {
            if (OnShareDidFailed != null) OnShareDidFailed.Invoke();
        }

        void OnSaveImageSuccessed(string message)
        {
            if (OnImageSaveSuccessed != null) OnImageSaveSuccessed.Invoke();
        }

        void OnSaveImageFailed(string message)
        {
            if (OnImageSaveFailed != null) OnImageSaveFailed.Invoke();
        }

        void OnPickImageFinished(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                PickImageFinished info = JsonUtility.FromJson<PickImageFinished>(message);
                var path = info.path.Replace("//", "/");
                if (OnPickImageFromAlbum != null) OnPickImageFromAlbum(path);
                if (OnPickTextureFromAlbum != null)
                {
                    try
                    {
                        byte[] bytes = System.IO.File.ReadAllBytes(path);
                        Texture2D t = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                        t.LoadImage(bytes);
                        OnPickTextureFromAlbum(t);
                    }
                    catch
                    {
                        OnPickTextureFromAlbum(null);
                    }
                }
            }
            else
            {
                OnPickTextureFromAlbum(null);
            }
        }
        #endregion

        #region iap
        public bool iapAutoAlert = true;//自动显示提示
        public bool restoreSuccessAutoAlert = true;//自动显示提示
        private bool _isIap;
        /// <summary>
        /// 是否正在处理IAP购买(比如你希望从enter foreground的时候，如果正在处理iap，可不显示广告，可用此状态变量判断)
        /// </summary>
        public bool IsIap
        {
            get { return _isIap; }
        }

        private StorePlugin _storePlugin;
        /// <summary>
        /// IAP插件实例
        /// </summary>
        private StorePlugin storePlugin
        {
            get
            {
                if (_storePlugin == null)
                {
                    _storePlugin = new StorePlugin();
                    _storePlugin.SetGameObjectNameForIAP(gameObject.name);
                }
                return _storePlugin;
            }
        }

        /// <summary>
        /// IAP Restore 成功回调, 参数为字符串数组
        /// </summary>
        public event Action<string[]> OnRestoreQuerySuccessEvent;

        /// <summary>
        /// IAP 购买成功回调.
        /// </summary>
        public event Action<string> OnRestoreSuccessEvent;

        public event Action<string> OnNoRestoreEvent;

        /// <summary>
        /// IAP 购买成功回调.
        /// </summary>
        public event Action<string> OnRestoreFailEvent;
        /// <summary>
        /// IAP 购买成功回调.
        /// </summary>
        public event Action<string> OnPurchaseSuccessEvent;

        /// <summary>
        /// IAP 购买失败回调.
        /// </summary>
        public event Action<string> OnPurchaseFailEvent;

        /// <summary>
        /// IAP购买开始事件,如可用于播放IAP购买loading动画.
        /// </summary>
        public event Action OnStartIapEvent;

        /// <summary>
        /// IAP处理结束事件,如可用于结束IAP购买loading动画.
        /// </summary>
        public event Action OnEndIapEvent;

        /// <summary>
        /// 返回的iap信息列表
        /// </summary>
        public Action<string> OnIapInfosActions;


        [System.Serializable]
        class IapClass
        {
            public string product_id = null;
        }
        [System.Serializable]
        class RestoreClass
        {
            public List<string> product_ids = null;
        }
        [System.Serializable]
        class IapFailClass
        {
            public string product_id = null;
            public int response_index = 0;
        }

        /// <summary>
        /// 购买一个
        /// </summary>
        /// <param name="productId">Product identifier.</param>
        public void PurchaseById(string productId)
        {
            if (IsPurchased(productId))
            {
                internalPlugin.popAlertDialog(GetLocalization(AlreadyPurchased));
                if (OnPurchaseSuccessEvent != null) OnPurchaseSuccessEvent.Invoke(productId);
                return;
            }
            if (!PluginManager.IsNetworkAvailable)
            {
                internalPlugin.popAlertDialog(GetLocalization(PurchaseNoNetwork));
                return;
            }
            storePlugin.PurchaseById(productId);
        }
        /// <summary>
        /// Restore 所有
        /// </summary>
        public void RestorePurchase()
        {
            if (!PluginManager.IsNetworkAvailable)
            {
                internalPlugin.popAlertDialog(GetLocalization(PurchaseNoNetwork));
                return;
            }
            storePlugin.RestoreAllPurchases();
        }

        /// <summary>
        /// 是否购买了非消耗性iap
        /// </summary>
        /// <returns><c>true</c> if this instance is purchased the specified iapId; otherwise, <c>false</c>.</returns>
        /// <param name="iapId">Iap identifier.</param>
        public bool IsPurchased(string iapId)
        {
            return PlayerPrefs.GetInt("Iap_" + iapId, 0) == 1;
        }

        /// <summary>
        /// Gets the IAP infos.
        /// </summary>
        /// <returns><c>true</c>, if IAP infos was gotten, <c>false</c> otherwise.</returns>
        /// <param name="iapIds">Iap identifiers.</param>
        public void GetIAPInfos(string[] iapIds)
        {
#if UNITY_EDITOR
            OnProductsInfos(null);
#else
		storePlugin.requestProductInfo(iapIds);
#endif
        }

        //iap回调
        void OnPurchaseSuccess(string message)
        {
            string iapId = JsonUtility.FromJson<IapClass>(message).product_id;
            PlayerPrefs.SetInt("Iap_" + iapId, 1);
            PlayerPrefs.Save();
#if UNITY_IOS
            if (iapAutoAlert) internalPlugin.popAlertDialog(GetLocalization(PurchaseSuccess));
#endif
            if (OnPurchaseSuccessEvent != null) OnPurchaseSuccessEvent.Invoke(iapId);
        }
        void OnPurchaseFailed(string message)
        {
#if UNITY_IOS
            if (iapAutoAlert) internalPlugin.popAlertDialog(GetLocalization(PurchaseFail));
#elif UNITY_ANDROID

		if (Application.platform == RuntimePlatform.Android) {

			int platformCode = AndroidHelper.getIapManagerAndroidObject ().GetStatic<int>("platformCode");
			if(platformCode==32){

				IapFailClass fail = JsonUtility.FromJson<IapFailClass>(message);
				string iapId = fail.product_id;
				int code = fail.response_index;

				if(code==7){
					if(iapAutoAlert) internalSDk.popAlertDialog(GetLocalization(AlreadyPurchased));

					PlayerPrefs.SetInt("Iap_"+iapId,1);
					PlayerPrefs.Save();

					if(OnPurchaseSuccessEvent!=null) OnPurchaseSuccessEvent.Invoke(iapId);
					return;

				}else{
					if(iapAutoAlert) internalSDk.popAlertDialog(GetLocalization(PurchaseFail));
				}
			}
		}
#endif
            if (OnPurchaseFailEvent != null) OnPurchaseFailEvent.Invoke(message);
        }
        void OnPurchaseCancelled(string message)
        {
#if UNITY_IOS
            if (iapAutoAlert) internalPlugin.popAlertDialog(GetLocalization(PurchaseCancel));
#endif
        }
        void OnRestoreFailed(string message)
        {
#if UNITY_IOS
            if (iapAutoAlert) internalPlugin.popAlertDialog(GetLocalization(RestoreFail));
#endif
            if (OnRestoreFailEvent != null) OnRestoreFailEvent.Invoke(message);
        }
        void OnRestoreQuerySuccess(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                if (OnRestoreQuerySuccessEvent != null) OnRestoreQuerySuccessEvent.Invoke(null);
                if (iapAutoAlert) internalPlugin.popAlertDialog(GetLocalization(NoRestore));
            }
            else
            {
                if (restoreSuccessAutoAlert) internalPlugin.popAlertDialog(GetLocalization(RestoreSuccess));
                RestoreClass iapArray = JsonUtility.FromJson<RestoreClass>(message);
                if (iapArray != null && iapArray.product_ids != null)
                {
                    foreach (string iapId in iapArray.product_ids)
                    {
                        PlayerPrefs.SetInt("Iap_" + iapId, 1);
                        PlayerPrefs.Save();
                    }
                }
                if (OnRestoreQuerySuccessEvent != null)
                    OnRestoreQuerySuccessEvent.Invoke(iapArray.product_ids.ToArray());
            }
        }
        void OnRestoreSuccess(string message)
        {
            if (restoreSuccessAutoAlert) internalPlugin.popAlertDialog(GetLocalization(RestoreSuccess));
            string iapId = JsonUtility.FromJson<IapClass>(message).product_id;
            PlayerPrefs.SetInt("Iap_" + iapId, 1);
            PlayerPrefs.Save();
            if (OnRestoreSuccessEvent != null) OnRestoreSuccessEvent.Invoke(iapId);
        }
        void OnRestoreCancelled(string message)
        {

        }
        void OnNoRestore(string message)
        {
            if (iapAutoAlert) internalPlugin.popAlertDialog(GetLocalization(NoRestore));
            if (OnNoRestoreEvent != null) OnNoRestoreEvent.Invoke(message);
        }
        void OnProductRequestBegin(string message)
        {
            if (OnStartIapEvent != null) OnStartIapEvent();
            CancelInvoke("StopIap");
            _isIap = true;
        }
        void OnProductRequestEnd(string message)
        {
            if (OnEndIapEvent != null) OnEndIapEvent.Invoke();
            Invoke("StopIap", 0.75f);//延迟一段时间
        }
        void StopIap()
        {
            _isIap = false;
        }
        void OnProductsNotReady(string message)
        {
#if UNITY_IOS
            if (iapAutoAlert) internalPlugin.popAlertDialog(GetLocalization(PurchaseNotReady));
#endif
        }
        void OnProductsInfos(string message)
        {
            if (OnIapInfosActions != null) OnIapInfosActions(message);
        }
        #endregion

    }
}
