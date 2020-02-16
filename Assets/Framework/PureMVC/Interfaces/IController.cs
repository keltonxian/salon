//
using System;

namespace PureMVC.Interfaces
{
    public interface IController
    {
        void RegisterCommand(string notificationName, Func<ICommand> commandFunc);
        void RegisterViewCommand(IView view, string[] commandNames);
        void ExecuteCommand(INotification notification);
        void RemoveCommand(string notificationName);
        void RemoveViewCommand(IView view, string[] commandNames);
        bool HasCommand(string notificationName);
    }
}
