using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PureMVC.Manager
{
    public class LockManager : Manager
    {
        public enum IAP_TYPE
        {
            NONE,
            FULL,
        }

        #region video lock
        public bool IsItemUnlockedByVideo(string itemKey)
        {
            //当前的分钟tick
            int nowMin = (int)(System.DateTime.Now.Ticks * 0.0000001f / 60f);
            //unlock时保存的时间
            int savedMin = DataManager.GetInt(itemKey, nowMin - 48 * 60);
            if (nowMin - savedMin < 24 * 60)
            {
                return true;
            }
            return false;
        }

        public void UnlockItemByVideo(string itemKey)
        {
            //当前的分钟tick
            int nowMin = (int)(System.DateTime.Now.Ticks * 0.0000001f / 60f);
            DataManager.SaveInt(itemKey, nowMin);
        }
        #endregion

        #region iap lock
        public bool IsFullUnlocked()
        {
            if (AdsManager.NoAdsLock)
            {
                return true;
            }
            if (string.IsNullOrEmpty(GameManager.KEY_FULL))
            {
                return true;
            }
            if (true == DataManager.GetBool(GameManager.KEY_FULL, false))
            {
                return true;
            }
            return false;
        }

        public void UnlockFull()
        {
            DataManager.SaveBool(GameManager.KEY_FULL, true);
        }
        #endregion

        #region decoration item lock
        public bool IsItemUnlocked(string itemKey)
        {
            if (IsFullUnlocked())
            {
                return true;
            }
            return IsItemUnlockedByVideo(itemKey);
        }
        #endregion

    }
}
