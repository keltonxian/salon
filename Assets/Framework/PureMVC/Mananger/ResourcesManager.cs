using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System;
using System.IO;
using UnityEngine.SceneManagement;

namespace PureMVC.Manager
{
    public class ResourcesManager : Manager
    {
        private static ResourcesManager _instance;

        public enum AssetType
        {
            Sprite,
            Texture2D,
            Sprites,
            AudioClip,
            Text,
            Bytes
        }

        public enum AssetPath
        {
            StreamingAssets,
            Resources,
            PersistentData,
            TemporaryCache
        }

        [System.Serializable]
        public class Asset
        {
            public string _url;
            public AssetType _type;
            public AssetPath _path;
            [HideInInspector]
            public Dictionary<string, Sprite> _dicSprite;

            private Sprite _sprite;
            public Sprite Sprite
            {
                get
                {
                    if (_sprite == null && _texture2d != null)
                    {
                        _sprite = CreateSprite(Vector2.one * 0.5f);
                    }
                    return _sprite;
                }
                set { _sprite = value; }
            }

            [HideInInspector]
            public Texture2D _texture2d;
            [HideInInspector]
            public AudioClip _audioClip;
            [HideInInspector]
            public string _text;
            [HideInInspector]
            public byte[] _bytes;

            //for sprite
            public SpriteMeshType _meshType = SpriteMeshType.FullRect;
            //for texture2d
            public bool _isTexture2dReadable = false;
            //for texture2d
            public TextureWrapMode _wrapMode = TextureWrapMode.Clamp;
            //use search path
            public bool _isUseSearchPath = true;

            public string PathName
            {
                get
                {
                    return _instance.GetPathName(_path);
                }
            }

            public bool _isCached = false;

            public Asset()
            {
            }

            public Asset(string url, AssetType type, AssetPath path = AssetPath.StreamingAssets, bool isCached = false)
            {
                _url = url;
                _type = type;
                _path = path;
                _isCached = isCached;
            }

            public Sprite CreateSprite(Vector2 pivot)
            {
                if (_texture2d != null)
                {
                    return _instance.CreateSprite(_texture2d, pivot);
                }
                return null;
            }

            public void Dispose()
            {
                _instance.DisposeAsset(this);
            }
        }

        public string _searchPath = "";
        public bool _isCheckEncryptFirst = false;
        public bool _isDisposeAssetsOnSceneLoad = true;
        [Range(1, 20)]
        public int _maxLoadingCount = 5;
        private Dictionary<string, Asset> _dicLoadSuccessed = new Dictionary<string, Asset>();
        private Dictionary<string, Asset> _dicLoadFailed = new Dictionary<string, Asset>();
        private Dictionary<string, Asset> _dicLoadingAsset = new Dictionary<string, Asset>();
        private int _currentCount;

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

        private string GetPathName(AssetPath path)
        {
            if (path == AssetPath.StreamingAssets) return DataManager.FolderStreamingAssets;
            if (path == AssetPath.PersistentData) return DataManager.FolderPersistentData;
            if (path == AssetPath.TemporaryCache) return DataManager.FolderTemporaryCache;
            return DataManager.FolderResources;
        }

        private string GetPath(Asset asset)
        {
            string search = asset._isUseSearchPath ? this._searchPath : "";
            if (File.Exists(Application.persistentDataPath + "/" + search + asset._url)
                || File.Exists(Application.persistentDataPath + "/" + search + asset._url + ".txt"))
            {
                return DataManager.PathPersistentData + search;
            }

            if (asset._path == AssetPath.StreamingAssets) return DataManager.PathStreamingAssets + search;
            if (asset._path == AssetPath.PersistentData) return DataManager.PathPersistentData + search;
            if (asset._path == AssetPath.TemporaryCache) return DataManager.PathTemporaryCache;
            return this._searchPath;
        }

        private Sprite CreateSprite(Texture2D texture2d, Vector2 pivot)
        {
            return Sprite.Create(texture2d, new Rect(0, 0, texture2d.width, texture2d.height), pivot, 100, 0, SpriteMeshType.FullRect);
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
            _dicLoadingAsset.Clear();
            _dicLoadFailed.Clear();
        }

        public void DisposeAsset(Asset asset)
        {
            if (asset._path == AssetPath.StreamingAssets || asset._path == AssetPath.PersistentData)
            {

                if (asset.Sprite)
                    DestroyImmediate(asset.Sprite, false);
                if (asset._dicSprite != null)
                {
                    foreach (Sprite s in asset._dicSprite.Values)
                    {
                        if (s) DestroyImmediate(s, false);
                    }
                    asset._dicSprite = null;
                }
                if (asset._texture2d)
                {
                    DestroyImmediate(asset._texture2d, false);
                }
                if (asset._audioClip)
                {
                    DestroyImmediate(asset._audioClip, false);
                }
                asset._bytes = null;
                asset._text = null;

            }
            else if (asset._path == AssetPath.Resources)
            {

                if (asset.Sprite)
                    Resources.UnloadAsset(asset.Sprite);
                if (asset._texture2d)
                    Resources.UnloadAsset(asset._texture2d);
                if (asset._dicSprite != null)
                {
                    foreach (Sprite s in asset._dicSprite.Values)
                    {
                        if (s) DestroyImmediate(s, false);
                    }
                    asset._dicSprite = null;
                }
                if (asset._audioClip)
                {
                    Resources.UnloadAsset(asset._audioClip);
                }
                asset._bytes = null;
                asset._text = null;
            }
            if (_dicLoadSuccessed.ContainsKey(asset.PathName + asset._url))
            {
                _dicLoadSuccessed.Remove(asset.PathName + asset._url);
            }
            GameManager.Log("url:" + asset._url);
        }

        public void DisposeAsset(string url, AssetPath path = AssetPath.StreamingAssets)
        {
            string pathName = GetPathName(path);

            if (_dicLoadSuccessed.ContainsKey(pathName + url))
            {
                DisposeAsset(_dicLoadSuccessed[pathName + url]);
            }
        }

        public void DisposeAsset(Texture texture)
        {
            if (texture != null)
            {
                foreach (Asset asset in _dicLoadSuccessed.Values)
                {
                    if (asset != null && asset._texture2d == texture)
                    {
                        DisposeAsset(asset);
                        break;
                    }
                }
            }
        }

        public void DisposeAsset(Sprite sprite)
        {
            if (sprite != null && sprite.texture != null)
            {
                DisposeAsset(sprite.texture);
            }
        }

        IEnumerator LoadingAsset(Asset asset, Action<Asset> onLoaded, Action<Asset> onLoadFailed = null)
        {
            string assetKey = asset.PathName + asset._url;
            if (_dicLoadFailed.ContainsKey(assetKey))
            {
                onLoadFailed?.Invoke(asset);
            }
            else
            {
                while (_currentCount >= _maxLoadingCount)
                {
                    yield return null;
                }

                if (_dicLoadSuccessed.ContainsKey(assetKey))
                {
                    Asset loaded = _dicLoadSuccessed[assetKey];
                    if (loaded._isTexture2dReadable != asset._isTexture2dReadable)
                    {
                        GameManager.Log(string.Format("Url[{0}] loaded, but readable is not the same", asset._url), GameManager.LogLevel.Error);
                    }
                    onLoaded?.Invoke(loaded);
                }
                else
                {
                    ++_currentCount;
                    GameManager.Log(string.Format("Load Current Check 1 [{0}]", _currentCount));
                    if (asset._path != AssetPath.Resources)
                    {
                        GameManager.Log(string.Format("Load Start[{0}]", assetKey));
                        string url = GetPath(asset) + asset._url;

                        _dicLoadingAsset[assetKey] = asset;
                        bool isReversed = _isCheckEncryptFirst ? true : false;

                        string realUrl = url + (_isCheckEncryptFirst ? ".txt" : "");
                        if (asset._type == AssetType.Sprites)
                        {
                            realUrl = url;
                            isReversed = false;
                        }

                        byte[] data = ReadFileBytes(realUrl);
                        if (data == null)
                        {
                            isReversed = _isCheckEncryptFirst ? false : true;
                            data = ReadFileBytes(url + (_isCheckEncryptFirst ? "" : ".txt"));
                        }

                        if (data != null && asset._type != AssetType.AudioClip)
                        {
                            GameManager.Log(string.Format("Load Successed[{0}]", assetKey));
                            yield return null;
                            if (_dicLoadingAsset.ContainsKey(assetKey))
                            {
                                _dicLoadingAsset.Remove(assetKey);
                            }
                            if (asset._type == AssetType.Sprite || asset._type == AssetType.Texture2D)
                            {
                                Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                                if (isReversed)
                                {
                                    data = ReverseBytes(data);
                                }
                                texture.LoadImage(data, !asset._isTexture2dReadable);
                                asset._texture2d = texture;
                                asset._texture2d.name = asset._url.Substring(asset._url.LastIndexOf("/", StringComparison.Ordinal) + 1);
                            }

                            if (asset._type == AssetType.Sprite)
                            {
                                asset._texture2d.wrapMode = TextureWrapMode.Clamp;
                                asset.Sprite = Sprite.Create(asset._texture2d, new Rect(0f, 0f, asset._texture2d.width, asset._texture2d.height), Vector2.one * 0.5f, 100, 1, asset._meshType);
                                asset.Sprite.name = asset._texture2d.name;
                                HandleLoadAsset(asset, true, onLoaded);

                            }
                            else if (asset._type == AssetType.Texture2D)
                            {
                                asset._texture2d.wrapMode = asset._wrapMode;
                                HandleLoadAsset(asset, true, onLoaded);
                            }
                            else if (asset._type == AssetType.Sprites && asset._url.LastIndexOf(".xml", StringComparison.Ordinal) == asset._url.Length - 4)
                            {
                                LoadingSpritesByXML(System.Text.Encoding.UTF8.GetString(data), asset, onLoaded, onLoadFailed);
                            }
                            else if (asset._type == AssetType.Text)
                            {
                                asset._text = System.Text.Encoding.UTF8.GetString(data);
                                HandleLoadAsset(asset, true, onLoaded);
                            }
                            else if (asset._type == AssetType.Bytes)
                            {
                                asset._bytes = data;
                                HandleLoadAsset(asset, true, onLoaded);
                            }
                        }
                        else
                        {
                            url = GetPath(asset) + asset._url;
                            isReversed = _isCheckEncryptFirst ? true : false;
                            realUrl = url + (_isCheckEncryptFirst ? ".txt" : "");
                            if (asset._type == AssetType.Sprites)
                            {
                                realUrl = url;
                                isReversed = false;
                            }

                            if (asset._type == AssetType.Sprite || asset._type == AssetType.Texture2D)
                            {

                                NetworkManager.GetTextureEncryptedPriority(realUrl, (Texture2D texture2d) => {
                                    asset._texture2d = texture2d;
                                    if (_dicLoadingAsset.ContainsKey(assetKey))
                                    {
                                        _dicLoadingAsset.Remove(assetKey);
                                    }
                                    asset._texture2d.name = asset._url.Substring(asset._url.LastIndexOf("/", StringComparison.Ordinal) + 1);
                                    if (asset._type == AssetType.Sprite)
                                    {
                                        asset._texture2d.wrapMode = TextureWrapMode.Clamp;
                                        asset.Sprite = Sprite.Create(asset._texture2d, new Rect(0f, 0f, asset._texture2d.width, asset._texture2d.height), Vector2.one * 0.5f, 100, 1, asset._meshType);
                                        asset.Sprite.name = asset._texture2d.name;
                                        HandleLoadAsset(asset, true, onLoaded);
                                    }
                                    else if (asset._type == AssetType.Texture2D)
                                    {
                                        asset._texture2d.wrapMode = asset._wrapMode;
                                        HandleLoadAsset(asset, true, onLoaded);
                                    }
                                }, (string error) => {
                                    GameManager.Log(string.Format("Load Texture2D Fail Url[{0}] Error[{1}]", realUrl, error), GameManager.LogLevel.Warning);
                                    HandleLoadAsset(asset, false, onLoadFailed);
                                }, asset._isTexture2dReadable, _isCheckEncryptFirst);
                            }
                            else if (asset._type == AssetType.Sprites && asset._url.LastIndexOf(".xml", StringComparison.Ordinal) == asset._url.Length - 4)
                            {
                                NetworkManager.GetText(realUrl, (string text) => {
                                    if (_dicLoadingAsset.ContainsKey(assetKey))
                                    {
                                        _dicLoadingAsset.Remove(assetKey);
                                    }
                                    LoadingSpritesByXML(text, asset, onLoaded, onLoadFailed);
                                }, (string error) => {
                                    GameManager.Log(string.Format("Load Texture2D Fail Url[{0}] Error[{1}]", asset._url, error), GameManager.LogLevel.Warning);
                                    HandleLoadAsset(asset, false, onLoadFailed);
                                });
                            }
                            else if (asset._type == AssetType.AudioClip)
                            {
                                NetworkManager.GetAudioClip(realUrl, (AudioClip audioClip) =>
                                {
                                    if (null != audioClip)
                                    {
                                        if (_dicLoadingAsset.ContainsKey(assetKey))
                                        {
                                            _dicLoadingAsset.Remove(assetKey);
                                        }
                                        asset._audioClip = audioClip;
                                        asset._audioClip.name = asset._url;
                                        HandleLoadAsset(asset, true, onLoaded);
                                    }
                                    else
                                    {
                                        GameManager.Log(string.Format("Load AudioClip Fail Url[{0}] isEmpty", asset._url), GameManager.LogLevel.Warning);
                                        HandleLoadAsset(asset, false, onLoadFailed);
                                    }
                                }, (string error) =>
                                {
                                    GameManager.Log(string.Format("Load AudioClip Fail Url[{0}] Error[{1}]", asset._url, error), GameManager.LogLevel.Warning);
                                    HandleLoadAsset(asset, false, onLoadFailed);
                                }, AudioType.MPEG);
                            }
                            else if (asset._type == AssetType.Text)
                            {
                                NetworkManager.GetText(realUrl, (string text) => {
                                    if (_dicLoadingAsset.ContainsKey(assetKey))
                                    {
                                        _dicLoadingAsset.Remove(assetKey);
                                    }
                                    asset._text = text;
                                    HandleLoadAsset(asset, true, onLoaded);
                                }, (string error) => {
                                    GameManager.Log(string.Format("Load Text Fail Url[{0}] Error[{1}]", asset._url, error), GameManager.LogLevel.Warning);
                                    HandleLoadAsset(asset, false, onLoadFailed);
                                });
                            }
                            else if (asset._type == AssetType.Bytes)
                            {
                                NetworkManager.GetBytes(realUrl, (byte[] bytes) => {
                                    if (_dicLoadingAsset.ContainsKey(assetKey))
                                    {
                                        _dicLoadingAsset.Remove(assetKey);
                                    }
                                    asset._bytes = bytes;
                                    HandleLoadAsset(asset, true, onLoaded);
                                }, (string error) => {
                                    GameManager.Log(string.Format("Load bytes Fail Url[{0}] Error[{1}]", asset._url, error), GameManager.LogLevel.Warning);
                                    HandleLoadAsset(asset, false, onLoadFailed);
                                });
                            }
                        }

                    }
                    else
                    {
                        _dicLoadingAsset[assetKey] = asset;
                        if (asset._type == AssetType.Sprite)
                        {
                            ResourceRequest rr = Resources.LoadAsync<Sprite>(asset._url);
                            yield return rr;
                            if (_dicLoadingAsset.ContainsKey(assetKey))
                            {
                                _dicLoadingAsset.Remove(assetKey);
                            }
                            if (rr.isDone && rr.asset != null)
                            {
                                asset.Sprite = rr.asset as Sprite;
                                if (asset.Sprite)
                                {
                                    asset._texture2d = asset.Sprite.texture;
                                    HandleLoadAsset(asset, true, onLoaded);
                                }
                            }
                            else
                            {
                                HandleLoadAsset(asset, false, onLoadFailed);
                            }

                        }
                        else if (asset._type == AssetType.Texture2D)
                        {
                            ResourceRequest rr = Resources.LoadAsync<Texture2D>(asset._url);
                            yield return rr;
                            if (_dicLoadingAsset.ContainsKey(assetKey))
                            {
                                _dicLoadingAsset.Remove(assetKey);
                            }
                            if (rr.isDone && rr.asset != null)
                            {
                                asset._texture2d = rr.asset as Texture2D;
                                HandleLoadAsset(asset, true, onLoaded);
                            }
                            else
                            {
                                HandleLoadAsset(asset, false, onLoadFailed);
                            }
                        }
                        else if (asset._type == AssetType.Sprites)
                        {

                            if (_dicLoadingAsset.ContainsKey(assetKey))
                            {
                                _dicLoadingAsset.Remove(assetKey);
                            }
                            Sprite[] sprites = Resources.LoadAll<Sprite>(asset._url);
                            if (sprites != null && sprites.Length > 0)
                            {
                                asset._dicSprite = new Dictionary<string, Sprite>();
                                foreach (Sprite s in sprites)
                                {
                                    asset._dicSprite[s.name] = s;
                                }
                                HandleLoadAsset(asset, true, onLoaded);
                            }
                            else
                            {
                                HandleLoadAsset(asset, false, onLoadFailed);
                            }
                        }
                        else if (asset._type == AssetType.AudioClip)
                        {
                            ResourceRequest rr = Resources.LoadAsync<AudioClip>(asset._url);
                            yield return rr;
                            asset._audioClip = rr.asset as AudioClip;
                            if (asset._audioClip == null) HandleLoadAsset(asset, false, onLoadFailed);
                            else HandleLoadAsset(asset, true, onLoaded);

                        }
                        else if (asset._type == AssetType.Text)
                        {
                            ResourceRequest rr = Resources.LoadAsync<TextAsset>(asset._url);
                            yield return rr;
                            TextAsset ta = (rr.asset as TextAsset);
                            if (ta) asset._text = ta.text;
                            if (ta == null) HandleLoadAsset(asset, false, onLoadFailed);
                            else HandleLoadAsset(asset, true, onLoaded);

                        }
                        else if (asset._type == AssetType.Bytes)
                        {
                            ResourceRequest rr = Resources.LoadAsync<TextAsset>(asset._url);
                            yield return rr;
                            TextAsset ta = (rr.asset as TextAsset);
                            if (ta) asset._bytes = ta.bytes;
                            if (ta == null) HandleLoadAsset(asset, false, onLoadFailed);
                            else HandleLoadAsset(asset, true, onLoaded);
                        }
                    }
                }
            }
        }

        private byte[] ReadFileBytes(string url)
        {
            try
            {
                url = url.Replace("file://", "").Replace("file:///", "");
                if (File.Exists(url))
                {
                    return File.ReadAllBytes(url);
                }
            }
            catch
            {
                throw new Exception("ReadFileBytes Fail");
            }
            return null;
        }

        public byte[] ReverseBytes(byte[] bytes)
        {
            Array.Reverse(bytes);
            return bytes;
        }

        private void HandleLoadAsset(Asset asset, bool isLoadSuccess, Action<Asset> onDone)
        {
            if (true == isLoadSuccess)
            {
                _dicLoadSuccessed[asset.PathName + asset._url] = asset;
                if (_dicLoadFailed.ContainsKey(asset.PathName + asset._url))
                {
                    _dicLoadFailed.Remove(asset.PathName + asset._url);
                }
            }
            else
            {
                GameManager.Log(string.Format("Load Fail [{0}][{1}]", asset.PathName, asset._url), GameManager.LogLevel.Warning);
                _dicLoadFailed[asset.PathName + asset._url] = asset;
            }
            onDone?.Invoke(asset);
            --_currentCount;
            GameManager.Log(string.Format("Load Current Check 2 [{0}]", _currentCount));
        }

        private void ReadSpritesByXML(string config, Asset asset)
        {

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(config);
            XmlNode root = xmlDoc.SelectSingleNode("TextureAtlas");
            XmlNodeList nodeList = root.ChildNodes;

            asset._dicSprite = new Dictionary<string, Sprite>();
            foreach (XmlNode xn in nodeList)
            {
                if (!(xn is XmlElement))
                    continue;
                XmlElement xe = (XmlElement)xn;
                string frameName = xe.GetAttribute("name").Replace('/', '_');
                float x = float.Parse(xe.GetAttribute("x"));
                float y = float.Parse(xe.GetAttribute("y"));
                float w = float.Parse(xe.GetAttribute("width"));
                float h = float.Parse(xe.GetAttribute("height"));
                Sprite s = Sprite.Create(asset._texture2d, new Rect(x, asset._texture2d.height - h - y, w, h), Vector2.one * 0.5f, 100, 1, asset._meshType);
                s.name = frameName;
                asset._dicSprite[frameName] = s;
            }
        }

        private void LoadingSpritesByXML(string config, Asset asset, Action<Asset> onLoaded, Action<Asset> onLoadFailed = null)
        {
            string atlasPath = asset._url.Substring(0, asset._url.LastIndexOf(".xml", StringComparison.Ordinal)) + ".png";
            bool isReversed = _isCheckEncryptFirst ? true : false;
            string url = GetPath(asset) + atlasPath;

            byte[] data = ReadFileBytes(url + (_isCheckEncryptFirst ? ".txt" : ""));
            if (data == null)
            {
                isReversed = _isCheckEncryptFirst ? false : true;
                data = ReadFileBytes(url + (_isCheckEncryptFirst ? "" : ".txt"));
            }

            if (data != null)
            {
                Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                texture.wrapMode = TextureWrapMode.Clamp;
                if (isReversed)
                {
                    data = ReverseBytes(data);
                }
                texture.LoadImage(data, !asset._isTexture2dReadable);
                asset._texture2d = texture;
                ReadSpritesByXML(config, asset);
                HandleLoadAsset(asset, true, onLoaded);
            }
            else
            {
                url = GetPath(asset) + atlasPath;

                NetworkManager.GetTextureEncryptedPriority(url, (Texture2D texture2d) => {
                    asset._texture2d = texture2d;
                    ReadSpritesByXML(config, asset);
                    HandleLoadAsset(asset, true, onLoaded);
                }, (string error) => {
                    GameManager.Log(string.Format("Load Fail [{0}]", url), GameManager.LogLevel.Warning);
                    HandleLoadAsset(asset, false, onLoadFailed);
                }, asset._isTexture2dReadable, _isCheckEncryptFirst);
            }
        }

        public void LoadGroup(Asset[] assetGroup, Action<Asset[]> onLoaded, Action<Asset[], float> onProgress = null)
        {
            int count = assetGroup.Length;
            for (int i = 0; i < assetGroup.Length; ++i)
            {
                LoadAsset(assetGroup[i], delegate (Asset asset) {
                    count--;
                    assetGroup[assetGroup.Length - count - 1] = asset;
                    onProgress?.Invoke(assetGroup, 1f - (float)count / assetGroup.Length);
                    if (count == 0)
                    {
                        onLoaded?.Invoke(assetGroup);
                    }
                }, delegate (Asset asset) {
                    count--;
                    onProgress?.Invoke(assetGroup, 1f - (float)count / assetGroup.Length);
                    if (count == 0)
                    {
                        onLoaded?.Invoke(assetGroup);
                    }
                });
            }
        }

        public void LoadAsset(Asset asset, Action<Asset> onLoaded, Action<Asset> onLoadFailed = null)
        {
            string path = asset.PathName + asset._url;
            if (_dicLoadFailed.ContainsKey(path))
            {
                onLoadFailed?.Invoke(asset);
                return;
            }
            if (_dicLoadSuccessed.ContainsKey(path))
            {
                Asset loaded = _dicLoadSuccessed[path];
                if (loaded._isTexture2dReadable != asset._isTexture2dReadable)
                {
                    GameManager.Log(string.Format("Url[{0}] loaded, but readable is not the same", asset._url), GameManager.LogLevel.Error);
                }
                onLoaded?.Invoke(loaded);
            }
            else if (_dicLoadingAsset.ContainsKey(path))
            {

                StartCoroutine(WaitLoad(asset, onLoaded, onLoadFailed));

            }
            else
            {
                StartCoroutine(LoadingAsset(asset, onLoaded, onLoadFailed));
            }
        }

        IEnumerator WaitLoad(Asset asset, Action<Asset> onLoaded, Action<Asset> onLoadFailed = null)
        {
            bool isSuccess = true;
            string path = asset.PathName + asset._url;
            while (true)
            {
                if (_dicLoadFailed.ContainsKey(path))
                {
                    isSuccess = false;
                    break;
                }

                if (!_dicLoadSuccessed.ContainsKey(path))
                {
                    yield return new WaitForFixedUpdate();
                }
                else break;
            }
            if (isSuccess)
            {
                onLoaded?.Invoke(_dicLoadSuccessed[path]);
            }
            else
            {
                onLoadFailed?.Invoke(asset);
            }
        }

        public Asset GetAsset(string url, AssetPath path = AssetPath.StreamingAssets)
        {
            string key = GetPathName(path) + url;

            if (_dicLoadSuccessed.ContainsKey(key))
                return _dicLoadSuccessed[key];
            return null;
        }

        public void RemoveAssetDontDispose(string url, AssetPath path = AssetPath.StreamingAssets)
        {
            string key = GetPathName(path) + url;

            if (_dicLoadSuccessed.ContainsKey(key))
            {
                _dicLoadSuccessed.Remove(key);
            }
            if (_dicLoadFailed.ContainsKey(key))
            {
                _dicLoadFailed.Remove(key);
            }
        }

        public void RemoveFailedAsset(string url, AssetPath path)
        {
            string key = GetPathName(path) + url;
            if (_dicLoadFailed.ContainsKey(key))
            {
                _dicLoadFailed.Remove(key);
            }
        }

        public void ClearFailedAssets()
        {
            _dicLoadFailed.Clear();
        }

        public void LoadSpriteFromStreamingAsset(string url, Action<Sprite> onLoaded, Vector2 pivot, bool isAsyncInIOS = true, Action<string> onLoadFailed = null, bool useSearchPath = true, SpriteMeshType meshType = SpriteMeshType.FullRect)
        {
            LoadTexture2DFromStreamingAsset(url, delegate (Texture2D t) {
                Sprite s = Sprite.Create(t, new Rect(0, 0, t.width, t.height), pivot, 100, 0, meshType);
                onLoaded?.Invoke(s);
            }, isAsyncInIOS, true, onLoadFailed, useSearchPath);
        }

        public void LoadTexture2DFromStreamingAsset(string url, Action<Texture2D> onLoaded, bool isAsyncInIOS = true, bool isReadable = false, Action<string> onLoadFailed = null, bool useSearchPath = true)
        {
            string searchPath = useSearchPath ? this._searchPath : "";

            string rootFolder = DataManager.PathPersistentData;
            string realPath = Application.persistentDataPath + "/" + searchPath + url;
            if (!File.Exists(realPath) && !File.Exists(realPath + ".txt"))
            {
                realPath = Application.streamingAssetsPath + "/" + searchPath + url;
                rootFolder = DataManager.PathStreamingAssets;
            }

#if UNITY_IOS

            if (isAsyncInIOS)
            {
                NetworkManager.GetTextureEncryptedPriority(rootFolder + searchPath + url, (Texture2D texture2d) => {
                    onLoaded?.Invoke(texture2d);
                }, (string error) => {
                    GameManager.Log(string.Format("Url[{0}] Async Loaded Fail", url));
                    onLoadFailed?.Invoke(error);
                }, isReadable, _isCheckEncryptFirst);
            }
            else
            {
                bool isHaveNoEncrypted = File.Exists(realPath);
                bool isHaveEncrypted = File.Exists(realPath + ".txt");
                if (isHaveNoEncrypted == false && isHaveEncrypted == false) {
                    GameManager.Log(string.Format("Url[{0}] Loaded Fail", url), GameManager.LogLevel.Warning);
				    onLoadFailed?.Invoke(url);
                    return;
                }
                byte[] bytes = File.ReadAllBytes(isHaveNoEncrypted ? realPath : realPath + ".txt");
                if (bytes != null) {
                    if (isHaveEncrypted) {
                        bytes = NetworkManager.ReverseBytes(bytes);
                    }
                    Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    texture.wrapMode = TextureWrapMode.Clamp;
                    texture.LoadImage(bytes, isReadable);
                    onLoaded?.Invoke(texture);
                }
            }
#else
            NetworkManager.GetTextureEncryptedPriority(rootFolder + searchPath + url, (Texture2D texture2d) => {
                onLoaded?.Invoke(texture2d);
            }, (string error) => {
                GameManager.Log(string.Format("Load Fail Url[{0}]", url), GameManager.LogLevel.Warning);
                onLoadFailed?.Invoke(error);
            }, isReadable, _isCheckEncryptFirst);
#endif
        }
    }
}
