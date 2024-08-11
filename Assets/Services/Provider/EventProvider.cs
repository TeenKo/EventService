namespace Services.Provider
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Cysharp.Threading.Tasks;
    using Data;
    using MemoryPack;

    [Serializable]
    public class EventProvider : IEventProvider
    {
        public List<EventData> Events { get; }
        
        private string _filePath;

        public EventProvider(string filePath)
        {
            _filePath = filePath;
            Events = new List<EventData>();
        }
        
        public void Push(EventData eventData)
        {
            Events.Add(eventData);
        }

        public async UniTask SaveEventsAsync(List<EventData> events)
        {
            var data = MemoryPackSerializer.Serialize(events);
            
            await File.WriteAllBytesAsync(_filePath, data);
        }

        public async UniTask<List<EventData>> LoadEventsAsync()
        {
            if (!File.Exists(_filePath))
            {
                return new List<EventData>();
            }

            var data = await File.ReadAllBytesAsync(_filePath);

            if (data == null || data.Length == 0)
            {
                return new List<EventData>();
            }

            return MemoryPackSerializer.Deserialize<List<EventData>>(data);
        }

        public void Delete(int id)
        {
            if (id >= 0 && id < Events.Count)
            {
                Events.RemoveAt(id);
            }
        }

        public async void Dispose()
        {
            await SaveEventsAsync(Events);
        }
    }
}