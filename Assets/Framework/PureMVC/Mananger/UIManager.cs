using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using PureMVC.Const;

namespace PureMVC.Manager
{
    public class UIManager : Manager
    {
        private static string _bundleUrlUI = string.Format("{0}/{1}_ui.unity3d", AppConst.AppName, AppConst.AppName);

        private void GetAssetBundleUI(Action<AssetBundle> callback)
        {
            AssetBundleManager.LoadAsset(_bundleUrlUI, (AssetBundleManager.Asset asset) =>
            {
                callback.Invoke(asset._assetBundle);
            }, (AssetBundleManager.Asset asset) =>
            {
                Debug.LogWarning("GetAssetBundleUI LoadAsset Fail");
            });
        }

        private void DisposeUI()
        {
            AssetBundleManager.DisposeAsset(_bundleUrlUI);
        }

        private Dictionary<GameObject, Sequence> _dicTipAnim = new Dictionary<GameObject, Sequence>();

        public void ShowTipFinger(Transform tipParent, Action<GameObject> callback, Transform fromObj, Transform toObj = null, float speed = 3f, LoopType loopType = LoopType.Restart)
        {
            if (null == tipParent)
            {
                return;
            }
            GetAssetBundleUI((AssetBundle bundle) =>
            {
                GameObject obj = bundle.LoadAsset<GameObject>("TipFinger");
                if (null == obj)
                {
                    return;
                }
                GameObject tipObj = Instantiate(obj);
                RectTransform tip = tipObj.GetComponent<RectTransform>();
                tip.SetParent(tipParent);
                tip.localPosition = Vector3.zero;
                tip.localEulerAngles = Vector3.zero;
                tip.localScale = Vector3.one;

                tip.gameObject.SetActive(true);
                Image image1 = tip.Find("1").GetComponent<Image>();
                image1.enabled = false;
                Image image2 = tip.Find("2").GetComponent<Image>();
                image2.enabled = false;
                Vector3 tipOffset = new Vector3(0.22f, -0.33f, 0f);
                Vector3 fromPos = fromObj.position + tipOffset;
                tip.position = fromPos;
                Sequence sequence = DOTween.Sequence();
                sequence.AppendCallback(() =>
                {
                    image1.enabled = true;
                    image2.enabled = false;
                });
                sequence.AppendInterval(0.5f);
                sequence.AppendCallback(() =>
                {
                    image1.enabled = false;
                    image2.enabled = true;
                });
                sequence.AppendInterval(0.5f);
                if (null != toObj)
                {
                    Vector3 toPos = toObj.position + tipOffset;
                    float distance = Mathf.Sqrt(Mathf.Pow((toPos.x - fromPos.x), 2) + Mathf.Pow((toPos.y - fromPos.y), 2));
                    float time = distance / speed;
                    sequence.Append(tip.DOMove(toPos, time).SetEase(Ease.Linear));
                }
                sequence.SetLoops(-1, loopType);
                _dicTipAnim.Add(tipObj, sequence);
                callback.Invoke(tipObj);
            });
        }

        public void HideTipFinger(GameObject tipObj)
        {
            if (null == tipObj)
            {
                return;
            }
            Sequence anim = _dicTipAnim[tipObj];
            if (null != anim)
            {
                anim.Kill();
            }
            _dicTipAnim[tipObj] = null;
            Destroy(tipObj);
        }

        public void ShowSelectFrameAni(Transform tipParent, Action<GameObject> callback, Transform targetObj)
        {
            if (null == tipParent)
            {
                return;
            }
            GetAssetBundleUI((AssetBundle bundle) =>
            {
                GameObject obj = bundle.LoadAsset<GameObject>("SelectFrameAni");
                if (null == obj)
                {
                    return;
                }
                GameObject tipObj = Instantiate(obj);
                RectTransform tip = tipObj.GetComponent<RectTransform>();
                tip.SetParent(tipParent);
                tip.localPosition = Vector3.zero;
                tip.localEulerAngles = Vector3.zero;
                tip.localScale = Vector3.one;

                tip.position = targetObj.position;

                tipObj.SetActive(true);
                callback.Invoke(tipObj);
            });
        }

        public void HideSelectFrameAni(GameObject tipObj)
        {
            if (null == tipObj)
            {
                return;
            }
            Destroy(tipObj);
        }

        public void ShowStaticObj(Transform tipParent, string objName, Action<GameObject> callback, Transform targetObj)
        {
            if (null == tipParent)
            {
                return;
            }
            GetAssetBundleUI((AssetBundle bundle) =>
            {
                GameObject obj = bundle.LoadAsset<GameObject>(objName);
                if (null == obj)
                {
                    return;
                }
                GameObject tipObj = Instantiate(obj);
                RectTransform tip = tipObj.GetComponent<RectTransform>();
                tip.SetParent(tipParent);
                tip.localPosition = Vector3.zero;
                tip.localEulerAngles = Vector3.zero;
                tip.localScale = Vector3.one;

                tip.position = targetObj.position;

                tipObj.SetActive(true);
                callback.Invoke(tipObj);
            });
        }

        public void HideStaticObj(GameObject tipObj)
        {
            if (null == tipObj)
            {
                return;
            }
            Destroy(tipObj);
        }

        public void ShowSelectFrame(Transform tipParent, Action<GameObject> callback, Transform targetObj)
        {
            ShowStaticObj(tipParent, "SelectFrame", callback, targetObj);
        }

        public void HideSelectFrame(GameObject tipObj)
        {
            HideStaticObj(tipObj);
        }

        public void ShowSelectTab(Transform tipParent, Action<GameObject> callback, Transform targetObj)
        {
            ShowStaticObj(tipParent, "SelectTab", callback, targetObj);
        }

        public void HideSelectTab(GameObject tipObj)
        {
            HideStaticObj(tipObj);
        }

        public void PlayUISound(string name)
        {
            GetAssetBundleUI((AssetBundle bundle) =>
            {
                AudioClip audioClip = bundle.LoadAsset<AudioClip>(name);
                AudioManager.PlaySound(audioClip);
            });
        }

        public void PlaySoundBtn()
        {
            PlayUISound("btn");
        }

        public void PlaySoundPopupShow()
        {
            PlayUISound("popup_show");
        }

        public void PlaySoundPopupHide()
        {
            PlayUISound("popup_hide");
        }
    }
}
