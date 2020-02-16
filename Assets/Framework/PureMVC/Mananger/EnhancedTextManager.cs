using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PureMVC.Manager
{
    public class EnhancedTextManager : Manager
    {
        private static EnhancedTextManager _instance;
        public static EnhancedTextManager Instance
        {
            get
            {
                return _instance;
            }
        }

        public const string UI_LAYER_NAME = "UI";
        public Color C_TITLE = new Color(243 / 255, 209 / 255, 80 / 255); // 243, 209, 80
        public Color C_TITLE_BORDER = new Color(0 / 255, 8 / 255, 9 / 255); // 0, 8, 9

        public static Callback.CallbackV OnLocalize;

        private List<string> _listLocalizeType = new List<string>();
        private int _localizeTypeIndex = 0;
        private Dictionary<string, string> _dicLocalizeString = new Dictionary<string, string>();

        public static void SetupRichText(EnhancedText txt)
        {
            txt.color = new Color(255f / 255f, 255f / 255f, 255f / 255f);
            txt.raycastTarget = false;
            RectTransform rt = txt.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(160f, 30f);
            rt.gameObject.layer = LayerMask.NameToLayer(UI_LAYER_NAME);
        }

        private void Awake()
        {
            _instance = this;
            LoadLocalizeType();
        }

        private void LoadLocalizeType()
        {
            _listLocalizeType.Clear();
            ResourcesManager.Asset tAsset = new ResourcesManager.Asset("TextScript/LocalizationType", ResourcesManager.AssetType.Bytes, ResourcesManager.AssetPath.Resources);
            ResourcesManager.LoadAsset(tAsset, (ResourcesManager.Asset asset) => {
                byte[] bytes = asset._bytes;
                FileHandler handler = new FileHandler(bytes);
                string str;
                while (!string.IsNullOrEmpty(str = handler.ReadLine()))
                {
                    _listLocalizeType.Add(str.Trim());
                }
                LoadLocalizeString();
            }, (ResourcesManager.Asset asset) => {
            });
        }

        private void LoadLocalizeString()
        {
            _dicLocalizeString.Clear();
            string url = string.Format("TextScript/{0}/Localization", _listLocalizeType[_localizeTypeIndex]);
            ResourcesManager.Asset tAsset = new ResourcesManager.Asset(url, ResourcesManager.AssetType.Bytes, ResourcesManager.AssetPath.Resources);
            ResourcesManager.LoadAsset(tAsset, (ResourcesManager.Asset asset) => {
                byte[] bytes = asset._bytes;
                FileHandler handler = new FileHandler(bytes);
                string str;
                while (!string.IsNullOrEmpty(str = handler.ReadLine()))
                {
                    string[] strs = str.Split('=');
                    string key = strs[0].Trim();
                    if (key.Substring(0, 2).Equals("//"))
                    {
                        continue;
                    }
                    key = key.Substring(1, key.Length - 2);

                    string value = strs[1].Trim();
                    value = value.Substring(1, value.Length - 3);
                    if (_dicLocalizeString.ContainsKey(key))
                    {
                        _dicLocalizeString[key] = value;
                    }
                    else
                    {
                        _dicLocalizeString.Add(key, value);
                    }
                    if (null != OnLocalize)
                    {
                        OnLocalize.Invoke();
                    }
                }
            }, (ResourcesManager.Asset asset) => {
            });
        }

        public string GetLocalizeString(string key)
        {
            if (!_dicLocalizeString.ContainsKey(key))
            {
                return key;
            }
            return _dicLocalizeString[key];
        }
    }
}
