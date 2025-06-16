

using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;

namespace H2CarBatteryIndicator.Services
{
    public class BleService : IBleService
    {
        private readonly IBluetoothLE ble;
        private readonly IList<IDevice> foundDevices;
        private IDevice connectedDevice;
        private IAdapter adapter;
        private IService service;
        private ICharacteristic characteristic;

        private string deviceName;
        private Guid serviceGuid;
        private Guid characteristicGuid;

        public BleService(string deviceName, Guid serviceGuid, Guid characteristicGuid)
        {
            DeviceName = deviceName;
            this.serviceGuid = serviceGuid;
            this.characteristicGuid = characteristicGuid;
            foundDevices = new List<IDevice>();

            //Get BLE adapter object from Application:
            this.ble = CrossBluetoothLE.Current;
            adapter = CrossBluetoothLE.Current.Adapter;
        }
        public string DeviceName
        {
            get { return deviceName; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Device name is null");
                }
                deviceName = value;
            }
        }
        public bool IsConnected => connectedDevice.State == DeviceState.Connected;

        public bool AdapterIsOn()
        {
            return ble.IsOn;
        }

        public async Task<DeviceState> ConnectToDeviceAsync()
        {
            try
            {
                if (!ble.IsOn)
                {
                    throw new InvalidOperationException("Bluetooth is off");
                }
                await ScanAllDevicesAsync();

                connectedDevice = foundDevices.FirstOrDefault(d => d.Name == deviceName);

                if (connectedDevice != null)
                {
                    await adapter.ConnectToDeviceAsync(connectedDevice);

                    service = await connectedDevice.GetServiceAsync(serviceGuid);
                    characteristic = await service.GetCharacteristicAsync(characteristicGuid);
                    if (service == null || characteristic == null)
                    {
                        throw new ArgumentException("Device service  not found");

                    }
                    return connectedDevice.State;
                }
                return DeviceState.Disconnected;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return DeviceState.Disconnected;
            }
            finally
            {
                await adapter.StopScanningForDevicesAsync();
            }
        }

        public async Task<DeviceState> DisconnectFromDeviceAsync()
        {
            if (connectedDevice != null && connectedDevice.State != DeviceState.Disconnected)
            {
                await adapter.DisconnectDeviceAsync(connectedDevice);
            }
            return connectedDevice.State;
        }

        public DeviceState GetConnectionState()
        {
            if (!ble.IsOn)
            {
                throw new InvalidOperationException("Bluetooth is off");
            }

            if (connectedDevice is null)
            {
                return DeviceState.Disconnected;
            }
            return connectedDevice.State;
        }

        public async Task<double> ReadCharacteristic()
        {
            if (service == null || characteristic == null)
            {
                throw new ArgumentException("Device service or characteristic not found");
            }

            var bytes = await characteristic.ReadAsync();
            double value = BitConverter.ToDouble(bytes.data, 0);
            return value;
        }
        // Method to subscribe to characteristic updates
        public async Task SubscribeToCharacteristicUpdatesAsync(Action<int> onValueUdated)
        {
            if (!ble.IsOn)
            {
                throw new InvalidOperationException("Bluetooth is off");
            }
            if (characteristic == null)
            {
                Console.WriteLine("Characteristic not found.");
                return;
            }

            try
            {
                // Check if notifications or indications are supported
                if (characteristic.CanUpdate)
                {
                    // Subscribe to notifications
                    characteristic.ValueUpdated += (s, e) =>
                    {
                        // Convert the byte[] to a readable value
                        int value = e.Characteristic.Value[0];
                        onValueUdated(value);
                        Console.WriteLine("Updated characteristic value: " + value);
                    };

                    await characteristic.StartUpdatesAsync();
                    Console.WriteLine("Subscribed to characteristic updates.");
                }
                else
                {
                    Console.WriteLine("Characteristic does not support updates.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error subscribing to characteristic updates: " + ex.Message);
            }
        }

        // Method to stop updates
        public async Task UnsubscribeFromCharacteristicUpdatesAsync()
        {
            if (characteristic == null)
            {
                Console.WriteLine("Characteristic not found.");
                return;
            }

            try
            {
                await characteristic.StopUpdatesAsync();
                Console.WriteLine("Unsubscribed from characteristic updates.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error unsubscribing from updates: " + ex.Message);
            }
        }
        private async Task ScanAllDevicesAsync()
        {
            foundDevices.Clear();

            adapter.DeviceDiscovered += (s, a) =>
            {
                if (!foundDevices.Contains(a.Device))
                    foundDevices.Add(a.Device);
            };

            await adapter.StartScanningForDevicesAsync();
        }
    }
}
