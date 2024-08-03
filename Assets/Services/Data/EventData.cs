namespace Services.Data
{
    using MemoryPack;

    [MemoryPackable]
    public partial class EventData
    {
        private string Type { get; set; }
        private string Data { get; set; }
    }
}