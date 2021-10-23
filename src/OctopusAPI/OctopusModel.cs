using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace src.OctopusAPI
{
  // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Result
    {
        [JsonProperty("consumption")]
        public double Consumption { get; set; }

        [JsonProperty("interval_start")]
        public DateTime IntervalStart { get; set; }

        [JsonProperty("interval_end")]
        public DateTime IntervalEnd { get; set; }
    }

    public class Root
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("next")]
        public string Next { get; set; }

        [JsonProperty("previous")]
        public object Previous { get; set; }

        [JsonProperty("results")]
        public List<Result> Results { get; set; }
    }
}