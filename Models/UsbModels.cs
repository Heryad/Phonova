namespace Phonova.Models
{
    public class UsbDevice
    {
        public string PnpDeviceId { get; set; } = "";
        public string LocationPath { get; set; } = "";
        public string Description { get; set; } = "";
        public string Manufacturer { get; set; } = "";
        public string Status { get; set; } = "";
        public bool IsAppleDevice { get; set; }
        public DateTime DetectedAt { get; set; }
        public string? Udid { get; set; }
    }

    public class PortMappingEntry
    {
        public int LogicalPort { get; set; }
        public string UsbLocationPath { get; set; } = "";
        public DateTime AssignedAt { get; set; }
        public bool IsConnected { get; set; }
    }

    public class HubInfo
    {
        public DateTime LastCalibrated { get; set; }
        public int TotalPorts { get; set; }
    }

    public class PortMappingConfiguration
    {
        public HubInfo? HubInfo { get; set; }
        public List<PortMappingEntry> Mappings { get; set; } = new();
    }
}
