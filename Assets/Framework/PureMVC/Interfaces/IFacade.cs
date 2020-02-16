using System;

namespace PureMVC.Interfaces
{
    public interface IFacade: INotifier
    {
        void RegisterProxy(IProxy proxy);
        IProxy RetrieveProxy(string proxyName);
        IProxy RemoveProxy(string proxyName);
        bool HasProxy(string proxyName);


        void RegisterCommand(string notificationName, Func<ICommand> commandFunc);
        void RegisterMultiCommand(Func<ICommand> commandFunc, params string[] notificationNames);
        void RemoveCommand(string notificationName);
        void RemoveMultiCommand(params string[] notificationNames);
        bool HasCommand(string notificationName);
        void SendMessageCommand(string name, object body = null);


        void RegisterMediator(IMediator mediator);
        IMediator RetrieveMediator(string mediatorName);
        IMediator RemoveMediator(string mediatorName);
        bool HasMediator(string mediatorName);


        void NotifyObservers(INotification notification);
    }
}
