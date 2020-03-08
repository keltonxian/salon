using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PureMVC.Manager
{
    public class AdsManager : Manager
    {

        private bool _noAdsLock = false;
        public bool NoAdsLock
        {
            get
            {
                return _noAdsLock;
            }
        }

        private const string KEY_APP_ID_IOS = "com.salon.full";
        private const string KEY_APP_ID_ANDROID = "com.salon.full";

        private const string KEY_BANNER_ID_IOS = "com.salon.full";
        private const string KEY_BANNER_ID_ANDROID = "com.salon.full";

        private const string KEY_INTERSTITIAL_ID_IOS = "com.salon.full";
        private const string KEY_INTERSTITIAL_ID_ANDROID = "com.salon.full";

        private const string KEY_REWARD_ID_IOS = "com.salon.full";
        private const string KEY_REWARD_ID_ANDROID = "com.salon.full";

        public string APP_ID
        {
            get { return (Application.platform == RuntimePlatform.IPhonePlayer ? KEY_APP_ID_IOS : KEY_APP_ID_ANDROID); }
        }

        public string BANNER_ID
        {
            get { return (Application.platform == RuntimePlatform.IPhonePlayer ? KEY_BANNER_ID_IOS : KEY_BANNER_ID_ANDROID); }
        }

        public string INTERSTITIAL_ID
        {
            get { return (Application.platform == RuntimePlatform.IPhonePlayer ? KEY_INTERSTITIAL_ID_IOS : KEY_INTERSTITIAL_ID_ANDROID); }
        }

        public string REWARD_ID
        {
            get { return (Application.platform == RuntimePlatform.IPhonePlayer ? KEY_REWARD_ID_IOS : KEY_REWARD_ID_ANDROID); }
        }

        Callback.CallbackB _onAdsRewardClose;

        public void ShowInterstitial()
        {
            //if (true == IsNoAdsUnlocked())
            //{
            //    return;
            //}
            //if (null == this.interstitial)
            //{
            //    return;
            //}
            //if (this.interstitial.IsLoaded())
            //{
            //    this.interstitial.Show();
            //}
            //else
            //{
            //    MonoBehaviour.print("Interstitial is not ready yet");
            //    DoOnRewardClose();
            //}
        }

        public void ShowRewardedAd(Callback.CallbackB onAdsRewardClose)
        {
            _onAdsRewardClose = onAdsRewardClose;
            DoOnRewardClose();
            //if (this.rewardedAd.IsLoaded())
            //{
            //    this.rewardedAd.Show();
            //}
            //else
            //{
            //    MonoBehaviour.print("Rewarded ad is not ready yet");
            //    ReloadRewardedAd();
            //    ShowInterstitial();
            //}
        }

        private void DoOnRewardClose()
        {
            if (null != _onAdsRewardClose)
            {
                _onAdsRewardClose(true);
                _onAdsRewardClose = null;
            }
        }

    }
}
