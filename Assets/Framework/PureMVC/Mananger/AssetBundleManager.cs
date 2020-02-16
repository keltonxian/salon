using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System;
using System.IO;
using UnityEngine.SceneManagement;

namespace PureMVC.Manager
{
    public class AssetBundleManager : Manager
    {
        private static AssetBundleManager _instance;

        [System.Serializable]
        public class Asset
        {
            public string _url;
            public AssetBundle _assetBundle;
            public bool _isCached = false;
            public Action<Asset> _onSuccess;
            public Action<Asset> _onFail;

            public Asset()
            {
            }

            public Asset(string url, Action<Asset> onSuccess, Action<Asset> onFail)
            {
                _url = url;
                _onSuccess = onSuccess;
                _onFail = onFail;
            }

            public void Dispose()
            {
                _instance.DisposeAsset(this);
            }
        }

        public bool _isDisposeAssetsOnSceneLoad = true;
        [Range(1, 20)]
        public int _maxLoadingCount = 5;
        private Dictionary<string, Asset> _dicLoadSuccessed = new Dictionary<string, Asset>();
        private Dictionary<string, AssetBundleManifest> _dicManifest = new Dictionary<string, AssetBundleManifest>();
        private List<Asset> _listLoadingAsset = new List<Asset>();
        private int _currentCount;
        private bool _isLoadingMainifest = false;

        void Awake()
        {
            _instance = this;
            SceneManager.sceneLoaded += delegate (Scene scene, LoadSceneMode mode) {
                if (_isDisposeAssetsOnSceneLoad && mode == LoadSceneMode.Single)
                {
                    DisposeAll();
                }
            };
            _maxLoadingCount = Mathf.Clamp(_maxLoadingCount, 1, 20);
        }

        public void DisposeAsset(string bundleUrl)
        {
            if (_dicLoadSuccessed.ContainsKey(bundleUrl))
            {
                Asset asset = _dicLoadSuccessed[bundleUrl];
                AssetBundle bundle = asset._assetBundle;
                if (bundle)
                {
                    bundle.Unload(true);
                }
                _dicLoadSuccessed.Remove(bundleUrl);
                GameManager.Log("Dispose by url:" + bundleUrl);
            }
        }

        public void DisposeAsset(Asset asset)
        {
            DisposeAsset(asset._url);
        }

        public void DisposeAll(bool containCache = false)
        {
            StopAllCoroutines();
            _currentCount = 0;
            List<Asset> assets = new List<Asset>();
            foreach (Asset asset in _dicLoadSuccessed.Values)
            {
                if (containCache)
                {
                    assets.Add(asset);
                }
                else if (!asset._isCached)
                {
                    assets.Add(asset);
                }
            }
            for (int i = 0; i < assets.Count; ++i)
            {
                DisposeAsset(assets[i]);
            }
            _listLoadingAsset.Clear();
            _dicLoadSuccessed.Clear();
        }

        public void DisposeAll(string[] excludes)
        {
            StopAllCoroutines();
            _currentCount = 0;
            List<string> keys = new List<string>();
            foreach (string key in _dicLoadSuccessed.Keys)
            {
                bool canDispose = true;
                for (int i = 0; i < excludes.Length; ++i)
                {
                    if (key.IndexOf(excludes[i], StringComparison.Ordinal) > -1)
                    {
                        canDispose = false;
                        break;
                    }
                }
                if (canDispose)
                {
                    keys.Add(key);
                }
            }
            for (int i = 0; i < keys.Count; ++i)
            {
                DisposeAsset(keys[i]);
            }
            _listLoadingAsset.Clear();
        }

        string GetManifestUrlByBundleUrl(string bundleUrl)
        {
            string url = bundleUrl.Replace("//", "/");
            if (url.IndexOf('/') > 0)
            {
                string[] b = url.Split('/');
                string u = "";
                for (int i = 0; i < b.Length - 1; ++i)
                {
                    u += b[i] + "/";
                }
                u += b[b.Length - 2];
                return u;
            }
            return DataManager.FolderStreamingAssets;
        }

        string GetRealUrl(string url)
        {
            if (File.Exists(Application.persistentDataPath + "/" + url))
            {
                return DataManager.PathPersistentData + url;
            }
            return DataManager.PathStreamingAssets + url;
        }

        public string[] GetDependBundles(string bundleUrl)
        {
            string manifestUrl = GetManifestUrlByBundleUrl(bundleUrl);
            if (_dicManifest.ContainsKey(manifestUrl))
            {
                string bundleName = Path.GetFileName(bundleUrl);
                string[] dependencies = _dicManifest[manifestUrl].GetAllDependencies(bundleName);
                if (dependencies.Length > 0)
                {
                    string folder = Path.GetDirectoryName(bundleUrl);
                    for (int i = 0; i < dependencies.Length; ++i)
                    {
                        dependencies[i] = folder + "/" + dependencies[i];
                    }
                }
                return dependencies;
            }
            return new string[] { };
        }

        public void LoadAssetBundleManifest(string mainifestUrl, Action onLoaded)
        {
            if (!_dicManifest.ContainsKey(mainifestUrl))
            {
                StartCoroutine(LoadAssetAsyn(mainifestUrl, null, delegate (Asset asset) {
                    AssetBundle bundle = asset._assetBundle;
                    _dicManifest[mainifestUrl] = bundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                    bundle.Unload(false);
                    GameManager.Log(string.Format("Load Manifest[{0}] Success", mainifestUrl));
                    onLoaded?.Invoke();
                }, null));
            }
            else
            {
                onLoaded?.Invoke();
            }
        }

        IEnumerator LoadAssetBundleManifestAsyn(string mainifestUrl)
        {
            GameManager.Log(string.Format("Load Manifest[{0}]", mainifestUrl));
            while (_isLoadingMainifest)
            {
                yield return new WaitForFixedUpdate();
            }
            if (_dicManifest.ContainsKey(mainifestUrl))
            {
                yield break;
            }

            _isLoadingMainifest = true;
            yield return StartCoroutine(NetworkManager.GetAssetBundle_CO(GetRealUrl(mainifestUrl), (AssetBundle bundle) => {
                if (_dicManifest.ContainsKey(mainifestUrl))
                {
                    return;
                }
                if (bundle)
                {
                    _dicManifest[mainifestUrl] = bundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                    GameManager.Log(string.Format("Load Manifest[{0}] Success", mainifestUrl));
                }
                else
                {
                    GameManager.Log(string.Format("Load Manifest[{0}] Fail Empty", mainifestUrl));
                }
            }, (string error) => {
                GameManager.Log(string.Format("Load Manifest[{0}] Fail Error[{1}]", mainifestUrl, error), GameManager.LogLevel.Error);
            }));
            yield return new WaitForEndOfFrame();
            yield return new WaitForFixedUpdate();
            _isLoadingMainifest = false;
        }

        public void LoadAsset(string bundleUrl, Action<Asset> onLoaded, Action<Asset> onLoadFailed = null)
        {
            if (_dicLoadSuccessed.ContainsKey(bundleUrl))
            {
                onLoaded?.Invoke(_dicLoadSuccessed[bundleUrl]);
                return;
            }
            for (int i = 0; i < _listLoadingAsset.Count; ++i)
            {
                if (_listLoadingAsset[i]._url.Equals(bundleUrl))
                {
                    _listLoadingAsset.Add(new Asset(bundleUrl, onLoaded, onLoadFailed));
                    return;
                }
            }
            string manifestUrl = GetManifestUrlByBundleUrl(bundleUrl);
            StartCoroutine(LoadAssetAsyn(bundleUrl, manifestUrl, onLoaded, onLoadFailed));
        }

        IEnumerator LoadAssetAsyn(string bundleUrl, string manifestUrl, Action<Asset> onLoaded, Action<Asset> onLoadFailed)
        {
            while (_currentCount >= _maxLoadingCount)
            {
                yield return 0;
            }

            Asset asset = new Asset(bundleUrl, onLoaded, onLoadFailed);
            _listLoadingAsset.Add(asset);
            ++_currentCount;
            GameManager.Log(string.Format("Load Current Check 1 [{0}][{1}]", _currentCount, bundleUrl));

            if (!string.IsNullOrEmpty(manifestUrl))
            {
                if (!_dicManifest.ContainsKey(manifestUrl))
                {
                    yield return StartCoroutine(LoadAssetBundleManifestAsyn(manifestUrl));
                }

                if (_dicManifest.ContainsKey(manifestUrl))
                {
                    yield return StartCoroutine(LoadDependencies(asset, manifestUrl));
                }
            }

            if (!_dicLoadSuccessed.ContainsKey(bundleUrl))
            {
                yield return StartCoroutine(NetworkManager.GetAssetBundle_CO(GetRealUrl(bundleUrl), (AssetBundle bundle) => {
                    if (_dicLoadSuccessed.ContainsKey(bundleUrl))
                    {
                        HandleLoadAsset(bundleUrl, true, _dicLoadSuccessed[bundleUrl]);
                        return;
                    }
                    if (bundle)
                    {
                        if (!string.IsNullOrEmpty(manifestUrl))
                        {
                            asset._assetBundle = bundle;
                            _dicLoadSuccessed[bundleUrl] = asset;
                            GameManager.Log(string.Format("Load AssetBundle[{0}] Success", bundleUrl));
                            HandleLoadAsset(bundleUrl, true, asset);
                        }
                    }
                    else
                    {
                        GameManager.Log(string.Format("Load AssetBundle[{0}] Fail Empty", bundleUrl), GameManager.LogLevel.Warning);
                        HandleLoadAsset(bundleUrl, false);
                    }
                }, (string error) => {
                    GameManager.Log(string.Format("Load Manifest[{0}] Fail Error[{1}]", bundleUrl, error), GameManager.LogLevel.Error);
                    HandleLoadAsset(bundleUrl, false);
                }));
                yield return new WaitForEndOfFrame();
                yield return new WaitForFixedUpdate();
            }
            else
            {
                HandleLoadAsset(bundleUrl, true, _dicLoadSuccessed[bundleUrl]);
            }
            --_currentCount;
            GameManager.Log(string.Format("Load Current Check 2 [{0}]", _currentCount));
        }

        void HandleLoadAsset(string url, bool isLoadSuccess, Asset asset = null)
        {
            for (int i = _listLoadingAsset.Count - 1; i >= 0; i--)
            {
                Asset temp = _listLoadingAsset[i];
                if (temp._url.Equals(url))
                {
                    if (isLoadSuccess)
                    {
                        temp._onSuccess?.Invoke(asset);
                    }
                    else
                    {
                        temp._onFail?.Invoke(asset);
                    }
                    _listLoadingAsset.RemoveAt(i);
                }
            }
        }

        IEnumerator LoadDependencies(Asset asset, string manifestUrl)
        {
            string bundleUrl = asset._url;
            string bundleName = Path.GetFileName(bundleUrl);
            string folder = Path.GetDirectoryName(bundleUrl);
            string[] dependencies = _dicManifest[manifestUrl].GetAllDependencies(bundleName);
            foreach (string str in dependencies)
            {
                string depend = str;
                if (depend.Equals("comm.unity3d") && _dicLoadSuccessed.ContainsKey(Const.AppConst.AppName + "/comm.unity3d"))
                {
                    continue;
                }
                depend = folder + "/" + depend;
                GameManager.Log("Depend:" + depend);
                if (!_dicLoadSuccessed.ContainsKey(depend))
                {
                    yield return StartCoroutine(NetworkManager.GetAssetBundle_CO(GetRealUrl(depend), (AssetBundle bundle) => {
                        if (_dicLoadSuccessed.ContainsKey(depend))
                        {
                            return;
                        }
                        if (bundle)
                        {
                            if (!string.IsNullOrEmpty(manifestUrl))
                            {
                                Asset dependAsset = new Asset(depend, null, null);
                                dependAsset._assetBundle = bundle;
                                _dicLoadSuccessed[depend] = dependAsset;
                                GameManager.Log(string.Format("Load AssetBundle[{0}] Success", depend));
                            }
                        }
                    }, (string error) => {
                        GameManager.Log(string.Format("Load Manifest[{0}] Fail Error[{1}]", depend, error), GameManager.LogLevel.Error);
                    }));
                    yield return new WaitForEndOfFrame();
                    yield return new WaitForFixedUpdate();
                }
            }
        }

        public AssetBundle GetAssetBundle(string bundleUrl)
        {
            if (_dicLoadSuccessed.ContainsKey(bundleUrl))
            {
                return _dicLoadSuccessed[bundleUrl]._assetBundle;
            }
            return null;
        }
    }
}
