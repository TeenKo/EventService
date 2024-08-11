namespace Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using Data;
    using Provider;
    using Transport;

    [Serializable]
    public class EventService : IDisposable, IEventService
    {
        private EventServiceData _eventServiceData;
        private List<EventData> _processedData = new();

        private IEventProvider _eventDataProvider;
        private IEventTransport _eventTransport;

        private CancellationTokenSource _serviceSource;
        
        private UniTask _initialization;

        public EventService(IEventProvider eventDataProvider, IEventTransport eventTransport,
            EventServiceData eventServiceData)
        {
            _serviceSource = new CancellationTokenSource();
            _eventTransport = eventTransport;
            _eventDataProvider = eventDataProvider;
            _eventServiceData = eventServiceData;
            _initialization = InitializeAsync().AsAsyncUnitUniTask();
        }
        
        public void TrackEvent(EventData eventData)
        {
            _eventDataProvider.Push(eventData);
        }

        private async UniTask InitializeAsync()
        {
            var eventsData = await _eventDataProvider.LoadEventsAsync();
            
            foreach (var eventData in eventsData)
                TrackEvent(eventData);
            SendEventProcessAsync().Forget();
        }

        private async UniTaskVoid SendEventProcessAsync()
        {
            await _initialization;
            
            while (!_serviceSource.IsCancellationRequested)
            {
                var events = _eventDataProvider.Events;

                if (events.Count <= 0)
                {
                    await UniTask.WaitForSeconds(_eventServiceData.cooldownWhenNoEvents,
                        cancellationToken: _serviceSource.Token);
                    continue;
                }

                _processedData.AddRange(events);
                
                foreach (var eventData in _processedData)
                {
                    var sendResult = await _eventTransport.SendEventsAsync(eventData, _processedData.IndexOf(eventData));
    
                    if (sendResult.Success)
                    {
                        _eventDataProvider.Delete(sendResult.Index);
                    }
                }
                
                await UniTask.WaitForSeconds(_eventServiceData.cooldownBeforeSend,
                    cancellationToken: _serviceSource.Token);
            }
        }

        public void Dispose()
        {
            _eventDataProvider?.Dispose();

            _serviceSource?.Cancel();
            _serviceSource?.Dispose();
            
            _eventTransport?.Dispose();
        }
    }
}