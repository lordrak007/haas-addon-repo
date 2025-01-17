using elan2mqtt.Helpers;
using Microsoft.Extensions.Configuration;
using RestSharp;
using System.Text;


namespace elan2mqtt
{
    internal class Main
    {
        MQTTConnector mqtt;
        eLanConnector elan;
        IConfiguration config;
        readonly string HA_Config_path = "/data/options.json";
        bool isHaasIO = false;
        public Main()
        {
            Console.WriteLine("eLan to MQTT starting ...");
            // detection if this is running in Home Assistant OS by detection classic path of addon configuration. Ugly but functional.
            isHaasIO = File.Exists(HA_Config_path);

            string mqttHost = string.Empty;
            string mqttUser = string.Empty;
            string mqttPassword = string.Empty;
            string mqttPort = string.Empty;
            bool mqttUseSSL = false;
            string mqttVersion = string.Empty;

            string elanHost = string.Empty;
            string elanUser = string.Empty;
            string elanPassword = string.Empty;


            if (isHaasIO) {
                Console.WriteLine("I think this is docker hosted by Home Assistant");
                Console.WriteLine("Getting MQTT configuration from supervisor ...");
                try
                {
                    // get mqtt configuration from supervisor
                    RestClient klient = new RestClient("http://supervisor/services/mqtt");
                    var request = new RestRequest();
                    request.AddHeader("Authorization", $"Bearer {Environment.GetEnvironmentVariable("SUPERVISOR_TOKEN")}");

                    var odpoved = klient.Get(request);
                    string mqttConfigJson = string.Empty;
                    if (odpoved != null && odpoved.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        mqttConfigJson = odpoved.Content;
                        Console.WriteLine($"MQTT configuration received.");
                        var mqttConfig = new ConfigurationBuilder().AddJsonStream(new System.IO.MemoryStream(Encoding.UTF8.GetBytes(mqttConfigJson))).Build();
                        var configDataSection = mqttConfig.GetSection("data");
                        mqttHost = configDataSection["host"];
                        mqttUser = configDataSection["username"];
                        mqttPassword = configDataSection["password"];
                        mqttPort = configDataSection["port"];
                        mqttUseSSL = configDataSection["ssl"] == "true";
                        mqttVersion = configDataSection["protocol"]?.Replace(".", "");
                    }
                    else
                    {
                        Console.WriteLine("Can not receive configuration for MQTT, exiting.");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error while getting MQTT configuration: {ex.Message}, \r\n {ex.InnerException}");
                    return;
                }
            }

            // setup config from addon config file
            config = new ConfigurationBuilder().AddJsonFile(HA_Config_path, false, false).Build();
            //Console.WriteLine("Configuration variables");
            //config.GetChildren().ToList().ForEach(x => Console.WriteLine($"{x.Key} => {x.Value}"));
            // remote trailing http[s]:// from given host url
            elanHost = config["eLan URL"].Replace("http://","").Replace("https://","");
            elanUser = config["eLan username"];
            elanPassword = config["eLan password"];


            // connect to eLan for http and ws
            elan = new eLanConnector(elanHost, elanUser, elanPassword);
            elan.ElanStatusEventReceived += (s, e) =>
            {

                mqtt?.PublishByDeviceID(e.Device, e.Payload);
            };
            elan.ElanDeviceDiscoveryEventReceived += (s, e) =>
            {
                mqtt?.Publish(e.Topic, e.Payload);
            };
            elan.Connect();

            // connect to mqtt
            mqtt = new MQTTConnector();
            mqtt.MessageReceived += (s, e) =>
            {
                Console.WriteLine($"MQTT: command {e.Address} => {e.Payload}");
                elan?.SetDeviceStateAsync(e.Address, e.Payload).Wait();
            };
            mqtt.Connect(mqttHost, mqttUser, mqttPassword, mqttUseSSL, mqttVersion).WaitAsync(new TimeSpan(0, 0, 10));
        }

        public void Exit()
        {
            // zavirání ukončení spojení (například mqtt ...)
            mqtt?.Disconnect().Wait();
            elan?.Dispose();

        }
    }
}
