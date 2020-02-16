using System;
using System.Collections.Generic;
using UnityEngine;
using PureMVC.Interfaces;
using PureMVC.Core;
using PureMVC.Patterns.Observer;
using PureMVC.Const;
using PureMVC.Command;

namespace PureMVC.Patterns.Facade
{
    public class Facade : IFacade
    {
        private static Facade _instance;
        protected Controller _controller;
        protected IModel _model;
        protected IView _view;
        private bool _hasStartUp = false;

        //public Facade()
        //{
        //}

        public static Facade Instance
        {
            get
            {
                if (null == _instance)
                {
                    _instance = new Facade();
                    _instance.InitializeFacade();
                }
                return _instance;
            }
        }

        private static GameObject _gameManager;
        private Dictionary<string, object> _dicManager = new Dictionary<string, object>();

        GameObject GameManager
        {
            get
            {
                if (null == _gameManager)
                {
                    _gameManager = new GameObject("[GameManager]");
                }
                //protected const string Singleton_MSG = "Facade Singleton already constructed!";
                //throw new Exception(Singleton_MSG);
                return _gameManager;
            }
        }

        protected void InitializeFacade()
        {
            InitializeModel();
            InitializeController();
            InitializeView();
            RegisterCommand(NotiConst.START_UP, () => new StartUpCommand());
        }

        protected void InitializeController()
        {
            _controller = Controller.Instance;
        }

        protected void InitializeModel()
        {
            _model = Model.GetInstance(() => new Model());
        }

        protected void InitializeView()
        {
            _view = View.Instance;
        }

        public void RegisterCommand(string notificationName, Func<ICommand> commandFunc)
        {
            _controller.RegisterCommand(notificationName, commandFunc);
        }

        public void RegisterMultiCommand(Func<ICommand> commandFunc, params string[] notificationNames)
        {
            int count = notificationNames.Length;
            for (int i = 0; i < count; i++)
            {
                RegisterCommand(notificationNames[i], commandFunc);
            }
        }

        public void RemoveCommand(string notificationName)
        {
            _controller.RemoveCommand(notificationName);
        }

        public void RemoveMultiCommand(params string[] notificationNames)
        {
            int count = notificationNames.Length;
            for (int i = 0; i < count; i++)
            {
                RemoveCommand(notificationNames[i]);
            }
        }

        public bool HasCommand(string notificationName)
        {
            return _controller.HasCommand(notificationName);
        }

        public void SendMessageCommand(string name, object body = null)
        {
            _controller.ExecuteCommand(new Notification(name, body));
        }

        public void RegisterProxy(IProxy proxy)
        {
            _model.RegisterProxy(proxy);
        }

        public IProxy RetrieveProxy(string proxyName)
        {
            return _model.RetrieveProxy(proxyName);
        }

        public IProxy RemoveProxy(string proxyName)
        {
            return _model.RemoveProxy(proxyName);
        }

        public bool HasProxy(string proxyName)
        {
            return _model.HasProxy(proxyName);
        }

        public void RegisterMediator(IMediator mediator)
        {
            _view.RegisterMediator(mediator);
        }

        public IMediator RetrieveMediator(string mediatorName)
        {
            return _view.RetrieveMediator(mediatorName);
        }

        public IMediator RemoveMediator(string mediatorName)
        {
            return _view.RemoveMediator(mediatorName);
        }

        public bool HasMediator(string mediatorName)
        {
            return _view.HasMediator(mediatorName);
        }

        public void SendNotification(string notificationName, object body = null, string type = null)
        {
            NotifyObservers(new Notification(notificationName, body, type));
        }

        public void NotifyObservers(INotification notification)
        {
            _view.NotifyObservers(notification);
        }

        public void AddManager(string typeName, object obj)
        {
            if (!_dicManager.ContainsKey(typeName))
            {
                _dicManager.Add(typeName, obj);
            }
        }

        public T AddManager<T>(string typeName) where T : Component
        {
            _dicManager.TryGetValue(typeName, out object result);
            if (null != result)
            {
                return (T)result;
            }
            Component c = GameManager.AddComponent<T>();
            _dicManager.Add(typeName, c);
            return default;
        }

        public T GetManager<T>(string typeName) where T : class
        {
            if (!_dicManager.ContainsKey(typeName))
            {
                return default(T);
            }
            _dicManager.TryGetValue(typeName, out object manager);
            return (T)manager;
        }

        public void RemoveManager(string typeName)
        {
            if (!_dicManager.ContainsKey(typeName))
            {
                return;
            }
            _dicManager.TryGetValue(typeName, out object manager);
            Type type = manager.GetType();
            if (type.IsSubclassOf(typeof(MonoBehaviour)))
            {
                UnityEngine.Object.Destroy((Component)manager);
            }
            _dicManager.Remove(typeName);
        }

        public void StartUp()
        {
            if (true == _hasStartUp)
            {
                return;
            }
            SendMessageCommand(NotiConst.START_UP);
            RemoveCommand(NotiConst.START_UP);
        }
    }
}
