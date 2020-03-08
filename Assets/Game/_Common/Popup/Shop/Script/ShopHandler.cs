using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;
using PureMVC.Manager;

public class ShopHandler : PopupView
{
	private string _videoLockKey;
	public string VideoLockKey
    {
		get {
			return _videoLockKey;
		}
		set {
			_videoLockKey = value;
		}
	}

	private Callback.CallbackB _videoLockCallback;
	public Callback.CallbackB VideoLockCallback
    {
		get {
			return _videoLockCallback;
		}
		set {
			_videoLockCallback = value;
		}
	}

	public delegate void IapCallbackDelegate (LockManager.IAP_TYPE iapType);
	private IapCallbackDelegate _iapCallback;
	public IapCallbackDelegate IapCallback
    {
		get {
			return _iapCallback;
		}
		set {
			_iapCallback = value;
		}
	}

	private Callback.CallbackV _shopCloseCallback;
	public Callback.CallbackV ShopCloseCallback
    {
		get {
			return _shopCloseCallback;
		}
		set {
			_shopCloseCallback = value;
		}
	}

	public override void Init ()
    {
		base.Init();
	}

    public virtual void ShowNetLoading()
    {
        UIManager.ShowNetLoading();
    }

    public virtual void RemoveNetLoading()
    {
        UIManager.HideNetLoading();
    }

    void OnUnlocked()
    {
        LockManager.UnlockItemByVideo(VideoLockKey);
        Close(() => {
            if (null != _videoLockCallback)
            {
                _videoLockCallback(true);
            }
        });
    }

    private bool UnlockIapItem(string iapKey)
    {
        LockManager.IAP_TYPE iapType = LockManager.IAP_TYPE.NONE;
        bool isUnlock = true;
        if (iapKey == GameManager.Instance.KEY_FULL)
        {
            LockManager.UnlockFull();
            iapType = LockManager.IAP_TYPE.FULL;
        }
        else
        {
            isUnlock = false;
        }
        if (true == isUnlock)
        {
            UIManager.ShowMsgBox("Thank you for your purchase.");
        }
        else
        {
            UIManager.ShowMsgBox(string.Format("Purchase Product '{0}' Fail.", iapKey));
        }
        if (null != _iapCallback && iapType != LockManager.IAP_TYPE.NONE)
        {
            _iapCallback(iapType);
        }
        Close();
        return isUnlock;
    }

    private void SimulateNetWaiting(Callback.CallbackV callback)
    {
        Sequence seq = DOTween.Sequence();
        seq.AppendCallback(() => {
            ShowNetLoading();
        });
        seq.AppendInterval(0.5f);
        seq.AppendCallback(() => {
            RemoveNetLoading();
        });
        seq.AppendInterval(1f);
        seq.AppendCallback(() => {
            callback();
        });
    }

    protected void UnlockFull()
    {
        if (Application.isEditor)
        {
            SimulateNetWaiting(() => {
                LockManager.UnlockFull();
                UnlockIapItem(GameManager.KEY_FULL);
            });
            return;
        }
        //		if (!CheckNetwork ()) {
        //			return;
        //		}
        if (LockManager.IsFullUnlocked())
        {
            UIManager.ShowMsgBox("You've Already Purchased Full.");
            UnlockIapItem(GameManager.KEY_FULL);
            return;
        }
        //BuyProductID(GameManager.KEY_FULL);
        PluginManager.PurchaseById(GameManager.Instance.KEY_FULL);
    }

    protected void Restore()
    {
        if (Application.isEditor)
        {
            SimulateNetWaiting(() => {
                // string[] keys = { GameManager.Instance.KEY_FULL };
                // OnRestoreQuerySuccess (keys);
                //RestorePurchases();
            });
            return;
        }
        //		if (!CheckNetwork ()) {
        //			return;
        //		}
        //RestorePurchases();
        PluginManager.RestorePurchase();
    }

    protected void GetItForFree()
    {
        if (string.IsNullOrEmpty(VideoLockKey))
        {
            return;
        }
        if (Application.isEditor)
        {
            SimulateNetWaiting(() => {
                OnUnlocked();
            });
            return;
        }
        //		if (!CheckNetwork ()) {
        //			return;
        //		}
        AdsManager.ShowRewardedAd((bool isDone) => {
            if (true == isDone)
            {
                OnUnlocked();
            }
        });
        //		if (PluginManager.Instance.ShowRewardAdOrShowCrossIfRewardNotReady ()) {
        //		}
        // PluginManager.Instance.ShowRewardAdOrShowCrossIfRewardNotReady (delegate(string name, int count, bool isSkip) {
        // 	if (!isSkip) {
        // 		OnUnlocked ();
        // 	}
        // });
    }

    public override void OnClickClose()
    {
        Close(() =>
        {
            //if (null != _shopCloseCallback)
            //{
            //    _shopCloseCallback();
            //}
        });
    }

    //protected override void Start ()
    //   {
    //	Debug.Log ("Shop Start");
    //	InitializePurchasing();
    //	base.Start ();
    //}

    /**

	public void InitializePurchasing() 
	{
		// If we have already connected to Purchasing ...
		if (IsInitialized())
		{
			// ... we are done here.
			return;
		}
		
		// Create a builder, first passing in a suite of Unity provided stores.
		var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
		
		// Add a product to sell / restore by way of its identifier, associating the general identifier
		// with its store-specific identifiers.
		// builder.AddProduct(kProductIDConsumable, ProductType.Consumable);
		// Continue adding the non-consumable product.
		// builder.AddProduct(kProductIDNonConsumable, ProductType.NonConsumable);
		// And finish adding the subscription product. Notice this uses store-specific IDs, illustrating
		// if the Product ID was configured differently between Apple and Google stores. Also note that
		// one uses the general kProductIDSubscription handle inside the game - the store-specific IDs 
		// must only be referenced here. 
		// builder.AddProduct(kProductIDSubscription, ProductType.Subscription, new IDs(){
		// 	{ kProductNameAppleSubscription, AppleAppStore.Name },
		// 	{ kProductNameGooglePlaySubscription, GooglePlay.Name },
		// });
		builder.AddProduct(GameManager.Instance.KEY_FULL, ProductType.NonConsumable);
		builder.AddProduct(GameManager.Instance.KEY_ACCESSORIES, ProductType.NonConsumable);
		builder.AddProduct(GameManager.Instance.KEY_DECORATIONS, ProductType.NonConsumable);
		builder.AddProduct(GameManager.Instance.KEY_NOADS, ProductType.NonConsumable);
		
		// Kick off the remainder of the set-up with an asynchrounous call, passing the configuration 
		// and this class' instance. Expect a response either in OnInitialized or OnInitializeFailed.
		UnityPurchasing.Initialize(this, builder);
	}

	private bool IsInitialized()
	{
		// Only say we are initialized if both the Purchasing references are set.
		return m_StoreController != null && m_StoreExtensionProvider != null;
	}
	
	
	// public void BuyConsumable()
	// {
	// 	// Buy the consumable product using its general identifier. Expect a response either 
	// 	// through ProcessPurchase or OnPurchaseFailed asynchronously.
	// 	BuyProductID(kProductIDConsumable);
	// }
	
	
	// public void BuyNonConsumable()
	// {
	// 	// Buy the non-consumable product using its general identifier. Expect a response either 
	// 	// through ProcessPurchase or OnPurchaseFailed asynchronously.
	// 	BuyProductID(kProductIDNonConsumable);
	// }
	
	
	// public void BuySubscription()
	// {
	// 	// Buy the subscription product using its the general identifier. Expect a response either 
	// 	// through ProcessPurchase or OnPurchaseFailed asynchronously.
	// 	// Notice how we use the general product identifier in spite of this ID being mapped to
	// 	// custom store-specific identifiers above.
	// 	BuyProductID(kProductIDSubscription);
	// }
	
	void BuyProductID(string productId)
	{
		// If Purchasing has been initialized ...
		if (IsInitialized())
		{
			// ... look up the Product reference with the general product identifier and the Purchasing 
			// system's products collection.
			Product product = m_StoreController.products.WithID(productId);
			
			// If the look up found a product for this device's store and that product is ready to be sold ... 
			if (product != null && product.availableToPurchase)
			{
				Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
				// ... buy the product. Expect a response either through ProcessPurchase or OnPurchaseFailed 
				// asynchronously.
				ShowNetLoading ();
				m_StoreController.InitiatePurchase(product);
			}
			// Otherwise ...
			else
			{
				// ... report the product look-up failure situation  
				ShowMsg ("Product Not Found.");
				Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
			}
		}
		// Otherwise ...
		else
		{
			// ... report the fact Purchasing has not succeeded initializing yet. Consider waiting longer or 
			// retrying initiailization.
			ShowMsg ("Shop Not Ready.");
			Debug.Log("BuyProductID FAIL. Not initialized.");
		}
	}
	
	
	// Restore purchases previously made by this customer. Some platforms automatically restore purchases, like Google. 
	// Apple currently requires explicit purchase restoration for IAP, conditionally displaying a password prompt.
	public void RestorePurchases()
	{
		// If Purchasing has not yet been set up ...
		if (!IsInitialized())
		{
			// ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
			Debug.Log("RestorePurchases FAIL. Not initialized.");
			return;
		}
		
		// If we are running on an Apple device ... 
		if (Application.platform == RuntimePlatform.IPhonePlayer || 
			Application.platform == RuntimePlatform.OSXPlayer)
		{
			// ... begin restoring purchases
			Debug.Log("RestorePurchases started ...");
			
			// Fetch the Apple store-specific subsystem.
			var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
			// Begin the asynchronous process of restoring purchases. Expect a confirmation response in 
			// the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
			ShowNetLoading ();
			apple.RestoreTransactions((result) => {
				// The first phase of restoration. If no more responses are received on ProcessPurchase then 
				// no purchases are available to be restored.
				Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
			});
		}
		// Otherwise ...
		else
		{
			// We are not running on an Apple device. No work is necessary to restore purchases.
			ShowMsg ("Restore Fail.");
			Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
		}
	}
	
	
	//  
	// --- IStoreListener
	//
	
	public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
	{
		// Purchasing has succeeded initializing. Collect our Purchasing references.
		Debug.Log("OnInitialized: PASS");
		
		// Overall Purchasing system, configured with products for this application.
		m_StoreController = controller;
		// Store specific subsystem, for accessing device-specific store features.
		m_StoreExtensionProvider = extensions;
	}
	
	
	public void OnInitializeFailed(InitializationFailureReason error)
	{
		// Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
		Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
	}
	
	
	public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args) 
	{
		RemoveNetLoading ();
		_shopHandlerInstance.UnlockIapItem (args.purchasedProduct.definition.id);
		// // A consumable product has been purchased by this user.
		// if (String.Equals(args.purchasedProduct.definition.id, kProductIDConsumable, StringComparison.Ordinal))
		// {
		// 	Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
		// 	// The consumable item has been successfully purchased, add 100 coins to the player's in-game score.
		// }
		// // Or ... a non-consumable product has been purchased by this user.
		// else if (String.Equals(args.purchasedProduct.definition.id, kProductIDNonConsumable, StringComparison.Ordinal))
		// {
		// 	Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
		// 	// TODO: The non-consumable item has been successfully purchased, grant this item to the player.
		// 	UnlockIapItem (args.purchasedProduct.definition.id);
		// }
		// // Or ... a subscription product has been purchased by this user.
		// else if (String.Equals(args.purchasedProduct.definition.id, kProductIDSubscription, StringComparison.Ordinal))
		// {
		// 	Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
		// 	// TODO: The subscription item has been successfully purchased, grant this to the player.
		// }
		// // Or ... an unknown product has been purchased by this user. Fill in additional products here....
		// else 
		// {
		// 	Debug.Log(string.Format("ProcessPurchase: FAIL. Unrecognized product: '{0}'", args.purchasedProduct.definition.id));
		// }

		// Return a flag indicating whether this product has completely been received, or if the application needs 
		// to be reminded of this purchase at next app launch. Use PurchaseProcessingResult.Pending when still 
		// saving purchased products to the cloud, and when that save is delayed. 
		return PurchaseProcessingResult.Complete;
	}

	public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
	{
		RemoveNetLoading ();
		// A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing 
		// this reason with the user to guide their troubleshooting actions.
		ShowMsg ("Purchase Fail.");
		Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
	}

	protected override void Init () {
	}

	protected override void ActionPreClose () {
		base.ActionPreClose ();
		RemoveNetLoading ();
	}

	protected override void ActionInClose () {
		base.ActionInClose ();
	}

	protected override void ActionInOnClickClose () {
		base.ActionInOnClickClose ();
		if (null != _shopCloseCallback) {
			_shopCloseCallback ();
		}
	}

	public virtual void ShowNetLoading () {
		NetLoading.Show ();
	}

	public virtual void RemoveNetLoading () {
		NetLoading.Remove (2f);
	}

	// void OnProductRequestBegin () {
	// 	ShowNetLoading ();
	// }

	// void OnProductRequestEnd () {
	// 	RemoveNetLoading ();
	// }

// 	void OnPurchaseSuccess (string iapKey) {
// 		// UnlockIapItem (iapKey);
// //		bool isUnlock = UnlockIapItem (iapKey);
// //		if (isUnlock) {
// //			PluginManager.Instance.internalSDk.popAlertDialog ("Thank you for your purchase.");
// //		}
// 	}

// 	void OnPurchaseFailed (string iapKey) {
// //		PluginManager.Instance.internalSDk.popAlertDialog("Purchase failed.");
// 		// Close ();
// 	}

// 	void OnPurchaseCancelled (string iapKey) {
// //		PluginManager.Instance.internalSDk.popAlertDialog("Your purchase was canceled. No purchase was made and your account was not charged.");
// 		// Close ();
// 	}

// 	void OnRestoreFailed (string iapKey) {
// //		PluginManager.Instance.internalSDk.popAlertDialog ("We have detected a problem restoring your purchases. No purchases were restored. Please check your device settings and storage and try again later..");
// 		// Close ();
// 	}

// 	void OnRestoreCancelled (string iapKey) {
// 		// Close ();
// 	}

// 	void OnNoRestore () {
// //		PluginManager.Instance.internalSDk.popAlertDialog ("We could not find any previous purchases. No purchases were restored.Please check your device settings and storage and try again later..");
// 		// Close ();
// 	}

// 	void OnProductsNotReady () {
// //		PluginManager.Instance.internalSDk.popAlertDialog ("Purchase not ready.");
// 		// Close ();
// 	}

// 	void OnRestoreQuerySuccess (string[] iapKeys) {
// 		RemoveNetLoading ();
// 		if (null == iapKeys || iapKeys.Length == 0) {
// //			PluginManager.Instance.internalSDk.popAlertDialog ("No purchase record has been found.");
// 			return;
// 		}
// 		bool hasFull = !string.IsNullOrEmpty (GameManager.Instance.KEY_FULL);
// 		bool hasNoAds = !string.IsNullOrEmpty (GameManager.Instance.KEY_NOADS);
// 		bool hasAccessories = !string.IsNullOrEmpty (GameManager.Instance.KEY_ACCESSORIES);
// 		bool hasDecorations = !string.IsNullOrEmpty (GameManager.Instance.KEY_DECORATIONS);
// 		for (int i = 0; i < iapKeys.Length; i++) {
// 			LockManager.IAP_TYPE iapType = LockManager.IAP_TYPE.NONE;
// 			string iapKey = iapKeys [i];
// 			if (iapKey.Equals (GameManager.Instance.KEY_FULL)) {
// 				LockManager.Instance.UnlockDecorations ();
// 				LockManager.Instance.UnlockAccessories ();
// 				LockManager.Instance.UnlockNoAds ();
// 				LockManager.Instance.UnlockFull ();
// 				hasFull = true;
// 				//				iapType = UnlockManager.IAP_TYPE.FULL_VERSION;
// 			} else if (iapKey.Equals (GameManager.Instance.KEY_DECORATIONS)) {
// 				LockManager.Instance.UnlockDecorations ();
// 				iapType = LockManager.IAP_TYPE.DECORATIONS;
// 				hasDecorations = true;
// 			} else if (iapKey.Equals (GameManager.Instance.KEY_ACCESSORIES)) {
// 				LockManager.Instance.UnlockAccessories ();
// 				iapType = LockManager.IAP_TYPE.ACCESSORIES;
// 				hasAccessories = true;
// 			} else if (iapKey.Equals (GameManager.Instance.KEY_NOADS)) {
// 				LockManager.Instance.UnlockNoAds ();
// 				iapType = LockManager.IAP_TYPE.NOADS;
// 				hasNoAds = true;
// 			} else {
// 				continue;
// 			}
// 			if (null != _iapCallback && iapType != LockManager.IAP_TYPE.NONE) {
// 				_iapCallback (iapType);
// 			}
// 		}
// 		if (hasFull || (hasAccessories && hasDecorations && hasNoAds)) {
// 			if (null != _iapCallback) {
// 				_iapCallback (LockManager.IAP_TYPE.FULL);
// 			}
// 		}
// //		PluginManager.Instance.internalSDk.popAlertDialog ("Restore successfully.");
// 		Close ();
// 	}

//	void OnAdsRewardHandler (string itemName,int amount,bool isSkipped) {
//		OnUnlocked ();
//	}

	

//	private bool CheckNetwork () {
//		if (PluginManager.netAvailable) {
//			return true;
//		}
//		PluginManager.Instance.internalSDk.popAlertDialog ("Please connect to internet and try again!");
//		return false;
//	}

	

	protected void UnlockAccessories () {
		if (Application.isEditor) {
			SimulateNetWaiting (() => {
				LockManager.Instance.UnlockAccessories ();
				UnlockIapItem (GameManager.Instance.KEY_ACCESSORIES);
			});
			return;
		}
//		if (!CheckNetwork ()) {
//			return;
//		}
		if (LockManager.Instance.IsAccessoriesUnlocked ()) {
			ShowMsg ("You've Already Purchased Accessories.");
			UnlockIapItem (GameManager.Instance.KEY_ACCESSORIES);
			return;
		}
		BuyProductID(GameManager.Instance.KEY_ACCESSORIES);
		// PluginManager.Instance.PurchaseById (GameManager.Instance.KEY_LEVELS, SkuType.Managed);
	}

	protected void UnlockDecorations () {
		if (Application.isEditor) {
			SimulateNetWaiting (() => {
				LockManager.Instance.UnlockDecorations ();
				UnlockIapItem (GameManager.Instance.KEY_DECORATIONS);
			});
			return;
		}
//		if (!CheckNetwork ()) {
//			return;
//		}
		if (LockManager.Instance.IsDecorationsUnlocked ()) {
			ShowMsg ("You've Already Purchased Decorations.");
			UnlockIapItem (GameManager.Instance.KEY_DECORATIONS);
			return;
		}
		BuyProductID(GameManager.Instance.KEY_DECORATIONS);
		// PluginManager.Instance.PurchaseById (GameManager.Instance.KEY_DECORATIONS, SkuType.Managed);
	}

	protected void UnlockNoAds () {
		if (Application.isEditor) {
			SimulateNetWaiting (() => {
				LockManager.Instance.UnlockNoAds ();
				UnlockIapItem (GameManager.Instance.KEY_NOADS);
			});
			return;
		}
//		if (!CheckNetwork ()) {
//			return;
//		}
		if (LockManager.Instance.IsNoAdsUnlocked ()) {
			ShowMsg ("You've Already Purchased No Ads.");
			UnlockIapItem (GameManager.Instance.KEY_NOADS);
			return;
		}
		BuyProductID(GameManager.Instance.KEY_NOADS);
		// PluginManager.Instance.PurchaseById (GameManager.Instance.KEY_NOADS, SkuType.Managed);
	}

	

    */

}
