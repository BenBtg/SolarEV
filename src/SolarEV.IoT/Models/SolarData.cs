using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SolarEV.IoT.Models
{
    public class Telemetries
    {
        [JsonProperty("timestamp")]
        public int Timestamp { get; set; }

        [JsonProperty("generating")]
        public double Generating { get; set; }

        [JsonProperty("exporting")]
        public double Exporting { get; set; }

        [JsonProperty("generated")]
        public double Generated { get; set; }

        [JsonProperty("exported")]
        public double Exported { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }
}