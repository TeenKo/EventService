namespace Services
{
    using Data;

    public interface IEventService
    {
        void TrackEvent(EventData eventData);
    }
}