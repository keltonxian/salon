using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PureMVC.Core;
using PureMVC.Manager;
using System;

public class SpriteHandler : Base
{
    public enum LoadType
    {
        None, Resources, StreamingAsset, AssetBundle,
    }

    public enum TargetType
    {
        None, Image, SpriteRenderer, DragonBones, ScribbleTool, UIMask, 
    }

    public enum ObjType
    {
        None, Sprite, Texture2D,
    }

    [SerializeField]
    private Callback.UnityEventV _onUpdate = new Callback.UnityEventV();
    public Callback.UnityEventV OnUpdate
    {
        get
        {
            return _onUpdate;
        }
    }

    [System.Serializable]
    public class SpriteData
    {
        public string _url;
        public LoadType _loadType = LoadType.None;
        public Image _targetImage;
        public SpriteRenderer _targetSpriteRenderer;
        public ScribbleTool _targetScribbleTool;
        public bool _isKeepLastAsset = false;
        public string _fileName;
        public bool _isReadable = false;
        public bool _isAutoShowAfterUpdate = true;
        public ResourcesManager.AssetType _assetType = ResourcesManager.AssetType.Sprite;
        public ResourcesManager.AssetPath _assetPath = ResourcesManager.AssetPath.StreamingAssets;

        public ObjType _objType = ObjType.None;
        public TargetType _targetType = TargetType.None;
        public object _obj;
        public object _lastAsset;

        public SpriteData()
        {
        }

        private void Init(string url, string fileName, LoadType loadType, bool isReadable = false, bool isKeepLastAsset = false, bool isAutoShowAfterUpdate = true)
        {
            _url = url;
            _fileName = fileName;
            _loadType = loadType;
            _isReadable = isReadable;
            _isKeepLastAsset = isKeepLastAsset;
            _isAutoShowAfterUpdate = isAutoShowAfterUpdate;
            if (_targetType == TargetType.ScribbleTool || _targetType == TargetType.UIMask || _targetType == TargetType.DragonBones)
            {
                _objType = ObjType.Texture2D;
            }
            else
            {
                _objType = ObjType.Sprite;
            }
        }

        public SpriteData(Image target, string formatUrl, string fileName, LoadType loadType, bool isReadable = false, bool isKeepLastAsset = false, bool isAutoShowAfterUpdate = true)
        {
            _targetImage = target;
            _targetType = TargetType.Image;
            Init(formatUrl, fileName, loadType, isReadable, isKeepLastAsset, isAutoShowAfterUpdate);
        }

        public SpriteData(SpriteRenderer target, string formatUrl, string fileName, LoadType loadType, bool isReadable = false, bool isKeepLastAsset = false, bool isAutoShowAfterUpdate = true)
        {
            _targetSpriteRenderer = target;
            _targetType = TargetType.SpriteRenderer;
            Init(formatUrl, fileName, loadType, isReadable, isKeepLastAsset, isAutoShowAfterUpdate);
        }

        public SpriteData(ScribbleTool target, string formatUrl, string fileName, LoadType loadType, bool isReadable = false, bool isKeepLastAsset = false, bool isAutoShowAfterUpdate = true)
        {
            _targetScribbleTool = target;
            _targetType = TargetType.ScribbleTool;
            Init(formatUrl, fileName, loadType, isReadable, isKeepLastAsset, isAutoShowAfterUpdate);
        }
    }

    private List<SpriteData> _listTarget = new List<SpriteData>();

    public void Init(List<SpriteData> listTarget)
    {
        for (int i = 0; i < listTarget.Count; i++)
        {
            _listTarget.Add(listTarget[i]);
        }
    }

    public void SelectItem(int index)
    {
        DoLoad(index);
    }

    private void DoLoad(int index)
    {
        for (int i = 0; i < _listTarget.Count; i++)
        {
            SpriteData data = _listTarget[i];
            if (data._loadType == LoadType.AssetBundle)
            {
                LoadByAssetBundle(index, data);
            }
            else
            {
                LoadByResources(index, data);
            }
        }
    }

    private void HandleUpdate(int index, bool isSuccess, SpriteData data)
    {
        if (data._targetType == TargetType.DragonBones)
        {

        }
        else if (data._targetType == TargetType.UIMask)
        {

        }
        else if (data._targetType == TargetType.ScribbleTool)
        {
            ScribbleTool scribbleTool = data._targetScribbleTool;
            if (scribbleTool._operation == ScribbleTool.Operation.Replace)
            {
                scribbleTool.UpdateTexture(data._obj as Texture2D);
            }
        }
    }

    private void LoadByResources(int index, SpriteData data)
    {
        //if (data._obj.GetType() == typeof(DataManager)) { };
        string url = string.Format(data._url, index);
        ResourcesManager.Asset tAsset = new ResourcesManager.Asset(url, data._assetType, data._assetPath);
        tAsset._isTexture2dReadable = data._isReadable;
        ResourcesManager.LoadAsset(tAsset, (ResourcesManager.Asset asset) => {
            if (false == data._isKeepLastAsset)
            {
                if (null != data._lastAsset)
                {
                    ResourcesManager.Asset temp = data._lastAsset as ResourcesManager.Asset;
                    if (asset != temp)
                    {
                        temp._isCached = false;
                        temp.Dispose();
                        data._lastAsset = null;
                    }
                }
            }
            asset._isCached = true;
            data._lastAsset = asset;
            if (data._objType == ObjType.Texture2D)
            {
                data._obj = asset._texture2d;
            }
            else
            {
                data._obj = asset.Sprite;
            }
            HandleUpdate(index, true, data);
        }, (ResourcesManager.Asset asset) => {
            HandleUpdate(index, false, data);
        });
    }

    private void LoadByAssetBundle(int index, SpriteData data)
    {
        string url = data._url;
        AssetBundleManager.LoadAsset(url, (AssetBundleManager.Asset asset) => {
            if (false == data._isKeepLastAsset)
            {
                if (null != data._lastAsset)
                {
                    AssetBundleManager.Asset temp = data._lastAsset as AssetBundleManager.Asset;
                    if (asset != temp)
                    {
                        temp._isCached = false;
                        temp.Dispose();
                        data._lastAsset = null;
                    }
                }
            }
            asset._isCached = true;
            data._lastAsset = asset;
            string fileName = string.Format(data._fileName, index);
            if (data._objType == ObjType.Texture2D)
            {
                data._obj = asset._assetBundle.LoadAsset<Texture2D>(fileName);
            }
            else
            {
                data._obj = asset._assetBundle.LoadAsset<Sprite>(data._fileName);
            }
            HandleUpdate(index, true, data);
        }, (AssetBundleManager.Asset asset) => {
            HandleUpdate(index, false, data);
        });
    }

    private void DisposeAssetBundle(AssetBundleManager.Asset asset)
    {
        AssetBundleManager.DisposeAsset(asset);
    }

}
