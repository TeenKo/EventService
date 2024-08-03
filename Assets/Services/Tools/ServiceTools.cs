namespace Services.Tools
{
    using System.Collections.Generic;
    using System.IO;
    using Data;
    using MemoryPack;
    using UnityEngine;
    using System;

    public static class ServiceTools
    {
        public static string GetFilePath()
        {
            return Path.Combine(Application.persistentDataPath, ConstContainer.EventsFileName);
        }
        
        public static void SaveEvents(string path, Queue<EventData> events)
        {
            var eventsToSave = new List<EventData>(events);
            var data = MemoryPackSerializer.Serialize(eventsToSave);
            File.WriteAllBytes(path, data);
        }

        public static void LoadEvents(string path, Queue<EventData> events)
        {
            if (!File.Exists(path)) return;

            var data = File.ReadAllBytes(path);

            try
            {
                var eventList = MemoryPackSerializer.Deserialize<List<EventData>>(data);
                foreach (var eventData in eventList)
                {
                    events.Enqueue(eventData);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to deserialize events: {ex.Message}");
            }
        }
    }
}