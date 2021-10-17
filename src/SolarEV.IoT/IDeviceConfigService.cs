using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SolarEV.IoT
{
    public interface IDeviceConfig
    {
        string DpsGlobalEndpoint { get; }
        string DeviceId { get; set; }
        string ScopeId { get; set; }
        string ModelId { get; }
        string DeviceKey { get; set; }

        string AssignedEndPoint { get; set; }

        // Task InitAsync();
        // Task SaveAsync();

        void ResetConfig();
    }
}
