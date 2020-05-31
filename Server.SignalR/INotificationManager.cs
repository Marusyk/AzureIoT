using System.Threading.Tasks;

namespace Server.SignalR
{
    public interface INotificationManager
    {
        Task Send(string elevatorId, EventType eventType, object payload);
    }
}
