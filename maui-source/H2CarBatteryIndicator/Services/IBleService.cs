using Plugin.BLE.Abstractions;

namespace H2CarBatteryIndicator.Services
{
    public interface IBleService
    {
        Task<DeviceState> ConnectToDeviceAsync();
        Task<DeviceState> DisconnectFromDeviceAsync();
        DeviceState GetConnectionState();
        Task<double> ReadCharacteristic();
        bool AdapterIsOn();
        public Task SubscribeToCharacteristicUpdatesAsync(Action<int> onValueUdated);
        public Task UnsubscribeFromCharacteristicUpdatesAsync();

    }
}
