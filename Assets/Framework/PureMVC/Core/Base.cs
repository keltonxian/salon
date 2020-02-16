using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PureMVC.Patterns.Facade;
using PureMVC.Manager;
using PureMVC.Interfaces;
using PureMVC.Const;

namespace PureMVC.Core
{
    public class Base : MonoBehaviour
    {
        private Facade _facade;
        private GameManager _gameManager;
        private ResourcesManager _resourcesManager;
        private AssetBundleManager _assetBundleManager;
        private AudioManager _audioManager;
        private DataManager _dataManger;
        private NetworkManager _networkManager;
        private UIManager _uiManager;
        private EnhancedTextManager _enhancedTextManager;

        protected void RegisterMessage(IView view, List<string> notiNames)
        {
            if (null == notiNames || 0 == notiNames.Count)
            {
                return;
            }
            Controller.Instance.RegisterViewCommand(view, notiNames.ToArray());
        }

        protected void RemoveMessage(IView view, List<string> notiNames)
        {
            if (null == notiNames || 0 == notiNames.Count)
            {
                return;
            }
            Controller.Instance.RemoveViewCommand(view, notiNames.ToArray());
        }

        protected Facade Facade
        {
            get
            {
                if (null == _facade)
                {
                    _facade = Facade.Instance;
                }
                return _facade;
            }
        }

        public GameManager GameManager
        {
            get
            {
                if (null == _gameManager)
                {
                    _gameManager = Facade.GetManager<GameManager>(ManagerName.Game);
                }
                return _gameManager;
            }
        }

        public ResourcesManager ResourcesManager
        {
            get
            {
                if (null == _resourcesManager)
                {
                    _resourcesManager = Facade.GetManager<ResourcesManager>(ManagerName.Resources);
                }
                return _resourcesManager;
            }
        }

        public AssetBundleManager AssetBundleManager
        {
            get
            {
                if (null == _assetBundleManager)
                {
                    _assetBundleManager = Facade.GetManager<AssetBundleManager>(ManagerName.AssetBundle);
                }
                return _assetBundleManager;
            }
        }

        public AudioManager AudioManager
        {
            get
            {
                if (null == _audioManager)
                {
                    _audioManager = Facade.GetManager<AudioManager>(ManagerName.Audio);
                }
                return _audioManager;
            }
        }

        public DataManager DataManager
        {
            get
            {
                if (null == _dataManger)
                {
                    _dataManger = Facade.GetManager<DataManager>(ManagerName.Data);
                }
                return _dataManger;
            }
        }

        public NetworkManager NetworkManager
        {
            get
            {
                if (null == _networkManager)
                {
                    _networkManager = Facade.GetManager<NetworkManager>(ManagerName.Network);
                }
                return _networkManager;
            }
        }

        public UIManager UIManager
        {
            get
            {
                if (null == _uiManager)
                {
                    _uiManager = Facade.GetManager<UIManager>(ManagerName.UI);
                }
                return _uiManager;
            }
        }

        public EnhancedTextManager EnhancedTextManager
        {
            get
            {
                if (null == _enhancedTextManager)
                {
                    _enhancedTextManager = Facade.GetManager<EnhancedTextManager>(ManagerName.EnhancedText);
                }
                return _enhancedTextManager;
            }
        }

    }
}
