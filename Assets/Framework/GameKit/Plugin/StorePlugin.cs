using UnityEngine;
using System.Runtime.InteropServices;

public class StorePlugin
{
/**
#if UNITY_IOS
    [DllImport("__Internal")]
    private static extern void _SetGameObjectNameForIAP(string gameObjectName);

    [DllImport("__Internal")]
    private static extern void _PurchaseById(string productId);

    [DllImport("__Internal")]
    private static extern void _RestorePurchase();

    [DllImport("__Internal")]
    private static extern void _requestProductInfo(string[] productIds, int length);
#endif

    public void SetGameObjectNameForIAP(string gameObjectName)
    {
        // Call plugin only when running on real device
        if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
        {
#if UNITY_ANDROID
			SSCIapAdapter.getInstance().setGameObjectName(gameObjectName);
#elif UNITY_IOS
            _SetGameObjectNameForIAP(gameObjectName);
#endif
        }
    }

    public void PurchaseById(string productId)
    {
        // Call plugin only when running on real device
        if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
        {
#if UNITY_ANDROID
			string tmpType = skuType == SkuType.Managed ? "Managed":"Unmanaged";
			SSCIapAdapter.getInstance().purchase(productId,tmpType);
#elif UNITY_IOS
            _PurchaseById(productId);
#endif
        }
    }

    public void RestoreAllPurchases()
    {
        // Call plugin only when running on real device
        if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
        {
#if UNITY_ANDROID
			SSCIapAdapter.getInstance().restore();
#elif UNITY_IOS
            _RestorePurchase();
#endif
        }

    }

    public void requestProductInfo(string[] productIds)
    {
        // Call plugin only when running on real device
        if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
        {
#if UNITY_ANDROID
			SSCIapAdapter.getInstance().queryProductsInfo(productIds);
#elif UNITY_IOS
            _requestProductInfo(productIds, productIds.Length);
#endif
        }
    }
*/
}
