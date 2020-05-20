using System;

namespace EventHub.Consumer
{
    public class DeviceEvent
    {
        public string eventType { get; set; }
        public Data data { get; set; }
        public DateTime eventTime { get; set; }
    }

    public class Data
    {
        public string hubName { get; set; }
        public string deviceId { get; set; }
    }
}
