using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Swan;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Computer;
using Unosquare.WiringPi;

namespace Raspberry.GPIO.NET
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Pi.Init<BootstrapWiringPi>();
            Terminal.WriteLine($"{nameof(Pi.Info)}: {Pi.Info}");

            Terminal.WriteLine("Network adapters:");
            var adapters = await NetworkSettings.Instance.RetrieveAdapters();
            foreach (var adapter in adapters)
            {
                Terminal.WriteLine($"\tAdapter: {adapter.Name}" +
                                  $"\t\t IPv4: {adapter.IPv4.Humanize()}" +
                                  $"\t\t IPv6: {adapter.IPv4.Humanize()}" +
                                    $"\t\t{(adapter.IsWireless ? "WiFi ("+adapter.AccessPointName+")" : "Ethernet")}");
            }

            Terminal.WriteLine("Polling for bluetooth controllers/devices");
            await Pi.Bluetooth.PowerOn();
            while (true)
            {
                Terminal.WriteLine("Controllers found: ");
                var controllers = await Pi.Bluetooth.ListControllers();
                var bluetoothController = controllers.Select(controller => new BluetoothController(controller)).ToList();
                foreach (var controller in bluetoothController)
                {
                    Terminal.WriteLine($"\t{controller.Name} (MAC: {controller.MACAddress})");
                }
                Terminal.WriteLine("Devices found: ");

                try
                {
                    var devices = await Pi.Bluetooth.ListDevices();
                    var bluetoothDevices = devices
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Select(device => new BluetoothDevice(device)).ToList();
                    foreach (var device in bluetoothDevices
                        .Select((value, index) => new
                    {
                        Device = value,
                        Index =index
                    }))
                    {
                        var index = device.Index + 1;
                        Console.WriteLine($"\t[{index}] {device.Device.Name}");
                    }
                    //var response = Terminal.ReadNumber("Want to connect to a device? (0 = skip)", 0);
                    //if (response > 0)
                    //{
                    //    var deviceToConnect = bluetoothDevices.ElementAt(response-1);
                    //    Terminal.WriteLine($"Connecting to {deviceToConnect.Name}");
                    //    var deviceInfo = await Pi.Bluetooth.DeviceInfo(deviceToConnect.MACAddress);
                    //    Terminal.WriteLine("DeviceInfo:");
                    //    Terminal.WriteLine($"\t{deviceInfo}");

                    //    var connected = await Pi.Bluetooth.Connect(bluetoothController.Single().MACAddress, deviceToConnect.MACAddress);
                    //    Terminal.WriteLine($"{(connected ? "Paired" : "Not paired")}");
                    //}
                    //Pi.Timing.SleepMilliseconds(5000);

                }
                catch (Exception ex)
                {
                    Terminal.WriteLine(ex.GetBaseException().ToString());
                }

               
            }
        }
    }

    internal class BluetoothController
    {
        public BluetoothController(string controller)
        {
            var rawDeviceString = controller.Split(" ");
            MACAddress = rawDeviceString[1];
            Name = rawDeviceString[2];
        }

        public string Name { get; set; }

        public string MACAddress { get; set; }
    }

    internal class BluetoothDevice
    {
        public string Name { get; set; }
        public string MACAddress { get; set; }
        public BluetoothDevice(string value)
        {
            var rawDeviceString = value.Split(" ");
            MACAddress = rawDeviceString[1];
            Name = rawDeviceString[2];
        }
    }
}
