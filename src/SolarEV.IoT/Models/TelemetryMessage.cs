using System.Numerics;
using Newtonsoft.Json;

namespace SolarEV.IoT.Models
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Accelerometer
    {
        [JsonProperty("x")]
        public float X { get; set; }

        [JsonProperty("y")]
        public float Y { get; set; }

        [JsonProperty("z")]
        public float Z { get; set; }
    }

    public class Geolocation
    {
        [JsonProperty("lat")]
        public double Lat { get; set; }

        [JsonProperty("lon")]
        public double Lon { get; set; }

        [JsonProperty("alt")]
        public double Alt { get; set; }
    }

    public class Sensors
    {
        [JsonProperty("battery")]
        public double Battery { get; set; }

        [JsonProperty("accelerometer")]
        public Accelerometer Accelerometer { get; set; }

        [JsonProperty("barometer")]
        public float Barometer { get; set; }

        [JsonProperty("geolocation")]
        public Geolocation Geolocation { get; set; }

        [JsonProperty("magnetometer")]
        public double Magnetometer { get; set; }

        [JsonProperty("gyroscope")]
        public Quaternion Gyroscope { get; set; }
    }
}
