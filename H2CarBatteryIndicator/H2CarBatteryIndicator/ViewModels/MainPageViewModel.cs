using CommunityToolkit.Mvvm.Input;
using H2CarBatteryIndicator.Services;
using Plugin.BLE.Abstractions;
using System.ComponentModel;
using static H2CarBatteryIndicator.Constants.AppConstants;
namespace H2CarBatteryIndicator.ViewModels
{
    public partial class MainPageViewModel : INotifyPropertyChanged
    {
        private readonly IBleService bleService;
        private bool isBusy;
        private bool isConnecting;
        private string connectionStatus = "Not connected";
        private DateTime? lastReceivedTime = null;
        private Timer timer;
        public DateTime? LastReceivedTime
        {
            get => lastReceivedTime;
            set
            {
                if (lastReceivedTime == value)
                {
                    return;
                }
                lastReceivedTime = value;
                OnPropertyChanged(nameof(LastReceivedTime));
            }
        }

        private int batteryPercentageValue;
        public MainPageViewModel(IBleService _bleService)
        {
            this.bleService = _bleService;
        }


        public string ConnectionStatus
        {
            get => connectionStatus;
            set
            {
                connectionStatus = value;
                OnPropertyChanged(nameof(ConnectionStatus));
            }
        }
        public bool IsConnecting
        {
            get => isConnecting;
            set
            {
                if (isConnecting == value)
                {
                    return;
                }
                isConnecting = value;
                OnPropertyChanged(nameof(IsConnecting));
            }
        }
        public bool IsBusy
        {
            get => isBusy;
            set
            {
                if (isBusy == value)
                { return; }
                isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
            }
        }
        public int BatteryPercentageValue
        {
            get => batteryPercentageValue;
            set
            {
                if (batteryPercentageValue == value)
                {
                    return;
                }
                batteryPercentageValue = value;
                OnPropertyChanged(nameof(BatteryPercentageValue));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public async Task OnPageAppear()
        {
            if (!bleService.AdapterIsOn())
            {
                await ShowBleOffWindow();
            }
            else
            {
                await ConnectToBleDevice();
                InitializeReconnectTimer();
            }

        }

        [RelayCommand]
        public async Task ReconnectDevice()
        {
            try
            {
                if (bleService.GetConnectionState() == DeviceState.Connected)
                {
                    await bleService.DisconnectFromDeviceAsync();
                }
                await ConnectToBleDevice();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        [RelayCommand]
        public async Task<bool> ConnectToBleDevice()
        {
            try
            {
                if (IsBusy)
                {
                    return false;
                }

                IsBusy = true;

                //Check if device is already connected:
                var deviceState = bleService.GetConnectionState();
                if (deviceState == DeviceState.Connected)
                {
                    return true;
                }

                //Get location to throw exception if location services are off:
                var deviceLocation = await Geolocation.Default.GetLocationAsync();

                //Connect to device:
                ConnectionStatus = "Connecting...";
                IsConnecting = true;
                deviceState = await bleService.ConnectToDeviceAsync();
                if (deviceState == DeviceState.Connected)
                {
                    //Subscribe to characteristics for batteryPercentage and receival time:
                    await bleService.SubscribeToCharacteristicUpdatesAsync(value =>
                    {
                        BatteryPercentageValue = value;
                        LastReceivedTime = DateTime.Now;
                    });
                }

                MapDeviceStateToConnectionStatus(deviceState);
                return true;
            }
            catch (PermissionException ex)
            {
                Console.WriteLine(ex.Message);
                await RequestPermissions();
                return false;
            }
            catch (FeatureNotEnabledException ex)
            {
                Console.WriteLine(ex.Message);
                await Shell.Current.DisplayAlert("Location services are off", "Please turn on location services to use this app", "OK");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            finally
            {
                IsConnecting = false;
                IsBusy = false;
            }
        }


        [RelayCommand]
        public async Task RequestPermissions()
        {
            var status = PermissionStatus.Unknown;

            status = await Permissions.RequestAsync<Permissions.Bluetooth>();

            if (status != PermissionStatus.Granted)
            {

                if (Permissions.ShouldShowRationale<Permissions.Bluetooth>())
                {
                    await Shell.Current.DisplayAlert("Needs permission", "Permission required for establishing connection with H2 car bluetooth module", "OK");
                }

                if (status != PermissionStatus.Granted)
                {
                    await Shell.Current.DisplayAlert("Permission required!", "Bluetooth permission is required for communication with H2 car", "OK");
                }
            }
        }
        private void MapDeviceStateToConnectionStatus(DeviceState state)
        {
            switch (state)
            {
                case DeviceState.Connected:
                    ConnectionStatus = "Car module connected";
                    break;
                case DeviceState.Disconnected:
                    ConnectionStatus = "Car module disconnected";
                    break;

            }

        }
        private async void CheckDeviceStatus(object state)
        {
            try
            {
                if (IsBusy)
                {
                    return;
                }
                if (!bleService.AdapterIsOn())
                {
                    timer.Dispose();
                    await ShowBleOffWindow();
                }
                var deviceState = bleService.GetConnectionState();
                MapDeviceStateToConnectionStatus(deviceState);
                if (deviceState == DeviceState.Disconnected)
                {
                    //try to reconnect:
                    await ReconnectDevice();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }
        private void InitializeReconnectTimer()
        {
            timer = new Timer(CheckDeviceStatus, null, 0, CheckDeviceConnectionStateInterval);
        }
        private async Task ShowBleOffWindow()
        {
            ConnectionStatus = "Bluetooth Off!";

            await Shell.Current.DisplayAlert("Bluetooth Off", "Turn on device bluetooth and restart app", "OK");
        }
    }
}
