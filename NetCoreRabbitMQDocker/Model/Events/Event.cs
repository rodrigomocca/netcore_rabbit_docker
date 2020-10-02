namespace Model.Events
{
    public class Event : IEvent
    {
        public Event(string id, string message)
        {
            Id = id;
            Message = message;
        }

        public string Id { get; set; }

        public string Message { get; set; }
    }
}