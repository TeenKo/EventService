namespace Services.Provider
{
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using Data;

    public interface IEventProvider : IDisposable
    {
        List<EventData> Events { get; }

        void Push(EventData eventData);

        UniTask SaveEventsAsync(List<EventData> events);

        UniTask<List<EventData>> LoadEventsAsync();

        void Delete(int id);
    }
}