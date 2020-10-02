using System.Threading.Tasks;
using Model.Events;

namespace Model
{
    public interface IRabbitManager
    {
        Task Publish(IEvent @event);
    }
}