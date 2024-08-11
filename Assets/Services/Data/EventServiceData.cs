namespace Services.Data
{
    using System;

    [Serializable]
    public class EventServiceData
    {
        public string serverUrl = "";
        public float cooldownBeforeSend = 2.0f;
        public float cooldownWhenNoEvents = 0.5f;
    }
}