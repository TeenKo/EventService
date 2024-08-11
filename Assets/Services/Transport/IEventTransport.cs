namespace Services.Transport
{
    using System;
    using Cysharp.Threading.Tasks;
    using Data;

    public interface IEventTransport : IDisposable
    {
        UniTask<EventSendResult> SendEventsAsync(EventData data, int index);
    }
}