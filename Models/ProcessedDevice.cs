using System;
using System.Collections.Generic;

namespace Phonova.Models
{
    public class ProcessedDevice
    {
        public string? DeviceName { get; set; }
        public string? Model { get; set; }
        public string? Color { get; set; }
        public string? Storage { get; set; }
        public string? Serial { get; set; }
        public string? Imei { get; set; }
        public string? IcloudStatus { get; set; }
        public string? FmiStatus { get; set; }
        public string? SimStatus { get; set; }
        public string? MdmStatus { get; set; }
        public string? BatteryHealth { get; set; }
        public string? BatteryCycles { get; set; }
        public string? ProductType { get; set; }
        public string? EnclosureCode { get; set; }
        public string? IosVersion { get; set; }
        public string? Region { get; set; }
        public Dictionary<string, string> KernelTests { get; set; } = new();
        public Dictionary<string, string> AppTests { get; set; } = new();
        public List<string> Comments { get; set; } = new();
        public string? Customer { get; set; }
        public string? Tester { get; set; }
        public DateTime DateTime { get; set; }
    }
}
