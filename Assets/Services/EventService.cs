namespace Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using Data;
    using MemoryPack;
    using UnityEngine;
    using UnityEngine.Networking;
    using Tools;

    public class EventService : MonoBehaviour, IDisposable
    {
        [SerializeField]
        private string serverUrl;
        [SerializeField]
        private float cooldownBeforeSend = 2.0f;

        private readonly Queue<EventData> _eventQueue = new();
        private bool _isSendingEvents;
        private bool _processingQueue;
        private string _path;
        
        private CancellationTokenSource _timerSource;
        private CancellationTokenSource _sendWebRequestSource;

        private void Start()
        {
            _path = ServiceTools.GetFilePath();
            ServiceTools.LoadEvents(_path, _eventQueue);
            ProcessQueueAsync().Forget();
        }

        public void TrackEvent(EventData eventData)
        {
            AddElementToQueue(eventData);
            
            if (!_processingQueue)
                ProcessQueueAsync().Forget();
        }

        private void AddElementToQueue(EventData eventData)
        {
            _eventQueue.Enqueue(eventData);
        }

        private async UniTaskVoid ProcessQueueAsync()
        {
            _processingQueue = true;
            
            while (_eventQueue.Count > 0)
            {
                await SendEventsAsync();
                
                _timerSource = new CancellationTokenSource();
                await UniTask.WaitForSeconds(cooldownBeforeSend, cancellationToken: _timerSource.Token);
            }

            _processingQueue = false;
        }

        private async UniTask SendEventsAsync()
        {
            if (_isSendingEvents || _eventQueue.Count == 0) return;

            _isSendingEvents = true;

            var eventsToSend = new List<EventData>();

            while (_eventQueue.Count > 0)
            {
                eventsToSend.Add(_eventQueue.Dequeue());
            }

            byte[] data;

            try
            {
                data = MemoryPackSerializer.Serialize(eventsToSend);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to serialize events: {ex.Message}");
                foreach (var eventData in eventsToSend)
                {
                    _eventQueue.Enqueue(eventData);
                }

                _isSendingEvents = false;
                return;
            }

            try
            {
                _sendWebRequestSource = new CancellationTokenSource();
                await SendPostRequestAsync(data);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to send events: {ex.Message}");
                foreach (var eventData in eventsToSend)
                {
                    AddElementToQueue(eventData);
                }
            }
            finally
            {
                _isSendingEvents = false;
                
                if (_eventQueue.Count > 0)
                {
                    await SendEventsAsync();
                }
            }
        }

        private async UniTask SendPostRequestAsync(byte[] data)
        {
            using var request = new UnityWebRequest(serverUrl, ConstContainer.PostMethod);
            request.uploadHandler = new UploadHandlerRaw(data);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader(ConstContainer.RequestHeader, ConstContainer.RequestValue);

            _sendWebRequestSource = new CancellationTokenSource();
            await request.SendWebRequest().WithCancellation(_sendWebRequestSource.Token);

            if (request.result != UnityWebRequest.Result.Success || request.responseCode != 200)
            {
                throw new Exception(request.error ?? $"Server returned {request.responseCode}");
            }
        }

        private void OnApplicationQuit()
        {
            ServiceTools.SaveEvents(_path, _eventQueue);
            Dispose();
        }

        public void Dispose()
        {
            _sendWebRequestSource?.Cancel();
            _sendWebRequestSource?.Dispose();
            
            _timerSource?.Cancel();
            _timerSource?.Dispose();
        }
    }
}