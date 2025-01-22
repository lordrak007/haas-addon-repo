using elan2mqtt.Helpers;
using Microsoft.Extensions.Configuration;
using RestSharp;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;



namespace elan2mqtt
{
    internal class Main
    {
        MQTTConnector mqtt;
        eLanConnector elan;
        IConfiguration config;
        readonly string HA_Config_path = "/data/options.json";
        readonly string LocalConfigDocker = "/app/LocalConfig.json";
        readonly string LocalConfigWindows = "./LocalConfig.json";

        ILogger log = ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);

        bool isHaasIO = false;
        public Main()
        {




            log.LogInformation("eLan to MQTT starting ...");
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
                log.LogDebug("I think this is docker hosted by Home Assistant");
                log.LogDebug("Getting MQTT configuration from supervisor ...");
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
                        log.LogDebug($"MQTT configuration received.");
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
                        log.LogError("Can not receive configuration for MQTT, exiting.");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    log.LogError($"Error while getting MQTT configuration: {ex.Message}, \r\n {ex.InnerException}");
                    return;
                }

                // setup config from addon config file
                config = new ConfigurationBuilder().AddJsonFile(HA_Config_path, false, false).Build();
                //log.LogDebug("Configuration variables");
                //config.GetChildren().ToList().ForEach(x => Console.WriteLine($"{x.Key} => {x.Value}"));
                // remote trailing http[s]:// from given host url
                elanHost = config["eLan URL"].Replace("http://", "").Replace("https://", "");
                elanUser = config["eLan username"];
                elanPassword = config["eLan password"];
            }
            else if (File.Exists(LocalConfigDocker))
            {
                
                log.LogDebug("I think this is running in linux image");
                log.LogDebug("Getting configuration from local file ...");
                config = new ConfigurationBuilder().AddJsonFile(LocalConfigDocker, false, false).Build();
                mqttHost = config.GetSection("connection").GetSection("mqtt")["host"];
                mqttUser = config.GetSection("connection").GetSection("mqtt")["user"];
                mqttPassword = config.GetSection("connection").GetSection("mqtt")["password"];
                mqttPort = config.GetSection("connection").GetSection("mqtt")["port"];
                mqttUseSSL = config.GetSection("connection").GetSection("mqtt")["usessl"] == "true";
                mqttVersion = config.GetSection("connection").GetSection("mqtt")["version"]?.Replace(".", "");
                elanHost = config.GetSection("connection").GetSection("elan")["host"];
                elanUser = config.GetSection("connection").GetSection("elan")["user"];
                elanPassword = config.GetSection("connection").GetSection("elan")["password"];
            }
            else if (File.Exists(LocalConfigWindows))
            {

                log.LogDebug("I think this is running on Windows");
                log.LogDebug("Getting configuration from local file ...");
                config = new ConfigurationBuilder().AddJsonFile(LocalConfigWindows, false, false).Build();
                mqttHost = config.GetSection("connection").GetSection("mqtt")["host"];
                mqttUser = config.GetSection("connection").GetSection("mqtt")["user"];
                mqttPassword = config.GetSection("connection").GetSection("mqtt")["password"];
                mqttPort = config.GetSection("connection").GetSection("mqtt")["port"];
                mqttUseSSL = config.GetSection("connection").GetSection("mqtt")["usessl"] == "true";
                mqttVersion = config.GetSection("connection").GetSection("mqtt")["version"]?.Replace(".", "");
                elanHost = config.GetSection("connection").GetSection("elan")["host"];
                elanUser = config.GetSection("connection").GetSection("elan")["user"];
                elanPassword = config.GetSection("connection").GetSection("elan")["password"];
            }
            else
            {
                // get configuration from other sources
                // jako třeba argumenty programu nebo ?
                log.LogError("No configuration found, exiting.");
                throw new Exception("No configuration found, exiting.");
            }

            log.LogDebug("Final connect settings");
            log.LogDebug($"MQTT host: {mqttHost}");
            log.LogDebug($"MQTT user: {mqttUser}");
            log.LogDebug($"MQTT password: *****");
            log.LogDebug($"eLan host: {elanHost}");
            log.LogDebug($"eLan user: {elanUser}");
            log.LogDebug($"eLan password: *****");


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
                log.LogDebug($"MQTT: command {e.Address} => {e.Payload}");
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
