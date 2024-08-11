namespace Services.Transport
{
    using System;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using Data;
    using MemoryPack;
    using UnityEngine.Networking;

    public class EventTransport : IEventTransport
    {
        private readonly string _serverUrl;
        private CancellationTokenSource _tokenSource;

        public EventTransport(string serverUrl)
        {
            _serverUrl = serverUrl;
        }

        public async UniTask<EventSendResult> SendEventsAsync(EventData data, int index)
        {
            _tokenSource = new CancellationTokenSource();
            
            var result = new EventSendResult
            {
                Index = index,
                Success = true,
                Error = string.Empty
            }; 

            var newData = MemoryPackSerializer.Serialize(data);
            
            using var request = new UnityWebRequest(_serverUrl, ConstContainer.PostMethod);
            request.uploadHandler = new UploadHandlerRaw(newData);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader(ConstContainer.RequestHeader, ConstContainer.RequestValue);

            await request.SendWebRequest().WithCancellation(_tokenSource.Token);

            if (request.result == UnityWebRequest.Result.Success && request.responseCode == 200)
                return result;
            
            result.Success = false;
            result.Index = index;
            result.Error = $"Server returned {request.responseCode}";
            
            return result;

        }

        public void Dispose()
        {
            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
        }
    }
}