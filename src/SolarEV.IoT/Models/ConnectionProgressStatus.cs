namespace SolarEV.IoT.Models
{
    public enum ConnectionProgressStatus
    {
        Disconnected = 0,
        Connected = 1,
        Disconnected_Retrying = 2,
        Disabled = 3,
        Provisioning = 4,
        Provisioned = 5,
        Connecting = 6
    }
}