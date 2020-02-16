using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using PureMVC.Interfaces;
using PureMVC.Patterns.Observer;

namespace PureMVC.Core
{
    public class View : IView
    {
        private static View _instance;
        protected readonly ConcurrentDictionary<string, IMediator> _dicMediator;
        protected readonly ConcurrentDictionary<string, IList<IObserver>> _dicObserver;

        public View()
        {
            _dicMediator = new ConcurrentDictionary<string, IMediator>();
            _dicObserver = new ConcurrentDictionary<string, IList<IObserver>>();
        }

        public static View Instance
        {
            get
            {
                if (null == _instance)
                {
                    _instance = new View();
                    _instance.InitializeView();
                }
                return _instance;
            }
        }

        protected void InitializeView()
        {
        }

        public void RegisterObserver(string notificationName, IObserver observer)
        {
            if (_dicObserver.TryGetValue(notificationName, out IList<IObserver> observers))
            {
                observers.Add(observer);
            }
            else
            {
                _dicObserver.TryAdd(notificationName, new List<IObserver> { observer });
            }
        }

        public void NotifyObservers(INotification notification)
        {
            if (_dicObserver.TryGetValue(notification.Name, out IList<IObserver> observers_ref))
            {
                var observers = new List<IObserver>(observers_ref);

                foreach (IObserver observer in observers)
                {
                    observer.NotifyObserver(notification);
                }
            }
        }

        public void RemoveObserver(string notificationName, object notifyContext)
        {
            if (_dicObserver.TryGetValue(notificationName, out IList<IObserver> observers))
            {
                for (int i = 0; i < observers.Count; i++)
                {
                    if (observers[i].CompareNotifyContext(notifyContext))
                    {
                        observers.RemoveAt(i);
                        break;
                    }
                }

                if (observers.Count == 0)
                    _dicObserver.TryRemove(notificationName, out IList<IObserver> _);
            }
        }

        public void RegisterMediator(IMediator mediator)
        {
            if (_dicMediator.TryAdd(mediator.MediatorName, mediator))
            {
                string[] interests = mediator.ListNotificationInterests();

                if (interests.Length > 0)
                {
                    IObserver observer = new Observer(mediator.HandleNotification, mediator);

                    for (int i = 0; i < interests.Length; i++)
                    {
                        RegisterObserver(interests[i], observer);
                    }
                }
                mediator.OnRegister();
            }
        }

        public IMediator RetrieveMediator(string mediatorName)
        {
            return _dicMediator.TryGetValue(mediatorName, out IMediator mediator) ? mediator : null;
        }

        public IMediator RemoveMediator(string mediatorName)
        {
            if (_dicMediator.TryRemove(mediatorName, out IMediator mediator))
            {
                string[] interests = mediator.ListNotificationInterests();
                for (int i = 0; i < interests.Length; i++)
                {
                    RemoveObserver(interests[i], mediator);
                }

                mediator.OnRemove();
            }
            return mediator;
        }

        public bool HasMediator(string mediatorName)
        {
            return _dicMediator.ContainsKey(mediatorName);
        }
    }
}

