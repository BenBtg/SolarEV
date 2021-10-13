using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SolarEV.IoT.Models
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class DeviceInfo
    {
        [JsonProperty("manufacturer")]
        public string Manufacturer { get; set; }

        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("swVersion")]
        public string SwVersion { get; set; }

        [JsonProperty("osName")]
        public string OsName { get; set; }

        [JsonProperty("processorArchitecture")]
        public string ProcessorArchitecture { get; set; }

        [JsonProperty("processorManufacturer")]
        public string ProcessorManufacturer { get; set; }

        [JsonProperty("totalStorage")]
        public int TotalStorage { get; set; }

        [JsonProperty("totalMemory")]
        public int TotalMemory { get; set; }

        [JsonProperty("__t")]
        public string T { get => "c"; }
    }

    public class ReportedDeviceProperties
    {
        [JsonProperty("device_info")]
        public DeviceInfo DeviceInfo { get; set; }
    }
}
