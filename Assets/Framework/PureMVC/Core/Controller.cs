using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using PureMVC.Interfaces;
using PureMVC.Patterns.Observer;

namespace PureMVC.Core
{
    public class Controller: IController
    {
        private static Controller _instance;
        protected IView _view;

        protected readonly ConcurrentDictionary<string, Func<ICommand>> _dicStrCmd;
        protected readonly ConcurrentDictionary<IView, List<string>> _dicViewCmd;

        public Controller()
        {
            _dicStrCmd = new ConcurrentDictionary<string, Func<ICommand>>();
            _dicViewCmd = new ConcurrentDictionary<IView, List<string>>();
        }

        public static Controller Instance
        {
            get
            {
                if (null == _instance)
                {
                    _instance = new Controller();
                    _instance.InitializeController();
                }
                return _instance;
            }
        }
        
        protected void InitializeController()
        {
            _view = View.Instance;
        }

        public void ExecuteCommand(INotification notification)
        {
            if (_dicStrCmd.TryGetValue(notification.Name, out Func<ICommand> commandFunc))
            {
                ICommand commandInstance = commandFunc();
                commandInstance.Execute(notification);
            }
        }

        public void RegisterCommand(string notificationName, Func<ICommand> commandFunc)
        {
            if (_dicStrCmd.TryGetValue(notificationName, out Func<ICommand> _) == false)
            {
                _view.RegisterObserver(notificationName, new Observer(ExecuteCommand, this));
            }
            _dicStrCmd[notificationName] = commandFunc;
        }

        public void RegisterViewCommand(IView view, string[] commandNames)
        {
            if (_dicViewCmd.ContainsKey(view))
            {
                if (_dicViewCmd.TryGetValue(view, out List<string> list))
                {
                    for (int i = 0; i < commandNames.Length; i++)
                    {
                        if (list.Contains(commandNames[i]))
                        {
                            continue;
                        }
                        list.Add(commandNames[i]);
                    }
                }
            }
            else
            {
                _dicViewCmd[view] = new List<string>(commandNames);
            }
        }

        public void RemoveCommand(string notificationName)
        {
            if (_dicStrCmd.TryRemove(notificationName, out Func<ICommand> _))
            {
                _view.RemoveObserver(notificationName, this);
            }
        }

        public void RemoveViewCommand(IView view, string[] commandNames)
        {
            if (_dicViewCmd.ContainsKey(view))
            {
                if (_dicViewCmd.TryGetValue(view, out List<string> list))
                {
                    for (int i = 0; i < commandNames.Length; i++)
                    {
                        if (!list.Contains(commandNames[i]))
                        {
                            continue;
                        }
                        list.Remove(commandNames[i]);
                    }
                }
            }
        }

        public bool HasCommand(string notificationName)
        {
            return _dicStrCmd.ContainsKey(notificationName);
        }
    }
}
