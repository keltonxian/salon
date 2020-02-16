using PureMVC.Interfaces;
using PureMVC.Patterns.Facade;

namespace PureMVC.Command
{
    public class Command : ICommand
    {
        public virtual void Execute(INotification Notification)
        {

        }

        public virtual void SendNotification(string notificationName, object body = null, string type = null)
        {
            Facade.Instance.SendNotification(notificationName, body, type);
        }
    }
}
