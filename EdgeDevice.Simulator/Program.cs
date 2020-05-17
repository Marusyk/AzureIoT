using System;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace EdgeDevice.Simulator
{
    internal class Program
    {
        private const string GlobalDeviceEndpoint = "global.azure-devices-provisioning.net";

        private static int CurrentFloor = -1;

        public static async Task Main(string[] args)
        {
            var configuration = ReadConfiguration();

            Console.WriteLine("Bootstrapping device...");

            using X509Certificate2 certificate = LoadCertificate(configuration.DeviceName);
            using var security = new SecurityProviderX509Certificate(certificate);
            var registrationResult = await RegisterDeviceAsync(configuration, security);

            var auth = new DeviceAuthenticationWithX509Certificate(registrationResult.DeviceId, security.GetAuthenticationCertificate());
            using DeviceClient deviceClient = DeviceClient.Create(registrationResult.AssignedHub, auth, TransportType.Mqtt);

            await deviceClient.SetMethodHandlerAsync(nameof(FloorCall), FloorCall, null);
            await SendDeviceToCloudMessagesAsync(configuration.DeviceName, deviceClient);
            Console.ReadKey();
        }

        private static async Task SendDeviceToCloudMessagesAsync(string deviceName, DeviceClient deviceClient)
        {
            while (true)
            {
                // Create JSON message
                var telemetryDataPoint = new
                {
                    floor = CurrentFloor,
                    device = deviceName
                };
                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                var message = new Message(Encoding.UTF8.GetBytes(messageString));

                // Send the telemetry message
                await deviceClient.SendEventAsync(message);
                Console.WriteLine("{0} > Current floow: {1}", DateTime.Now, messageString);

                await Task.Delay(5 * 1000);
            }
        }

        private static Configuration ReadConfiguration()
        {
            var configurationRoot = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var configuration = configurationRoot.Get<Configuration>();
            return configuration;
        }

        private static Task<MethodResponse> FloorCall(MethodRequest methodRequest, object userContext)
        {
            var data = Encoding.UTF8.GetString(methodRequest.Data);

            // Check the payload is a single integer value
            if (int.TryParse(data, out var floor))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Elevator call to {0} floor", data);
                SetFloot(floor);
                Console.ResetColor();

                // Acknowlege the direct method call with a 200 success message
                string result = "{\"result\":\"Elevator will be on " + floor + " in 10 sec" + "\"}";
                return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 200));
            }
            else
            {
                // Acknowlege the direct method call with a 400 error message
                string result = "{\"result\":\"Invalid parameter\"}";
                return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(result), 400));
            }

            async Task SetFloot(int floor)
            {
                await Task.Delay(10000);
                CurrentFloor = floor;
            }
        }

        private static X509Certificate2 LoadCertificate(string deviceName)
        {
            using var store = new X509Store(StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);
            var certificateCollection = store.Certificates.Find(X509FindType.FindBySubjectName, deviceName, false);

            if (certificateCollection.Count == 0)
            {
                throw new Exception($"No matching certificate found for subject '{deviceName}'");
            }

            return certificateCollection[0];
        }

        private static async Task<DeviceRegistrationResult> RegisterDeviceAsync(Configuration configuration, SecurityProviderX509Certificate security)
        {
            using var transport = new ProvisioningTransportHandlerMqtt(TransportFallbackType.TcpOnly);
            var provClient = ProvisioningDeviceClient.Create(GlobalDeviceEndpoint, configuration.DpsIdScope, security, transport);
            DeviceRegistrationResult registrationResult = await provClient.RegisterAsync();
            Console.WriteLine($"Registration {registrationResult.Status}");
            Console.WriteLine($"ProvisioningClient AssignedHub: {registrationResult.AssignedHub}; DeviceID: {registrationResult.DeviceId}");

            if (registrationResult.Status != ProvisioningRegistrationStatusType.Assigned)
                throw new Exception("IoT Hub not assigned!");
            return registrationResult;
        }
    }
}
