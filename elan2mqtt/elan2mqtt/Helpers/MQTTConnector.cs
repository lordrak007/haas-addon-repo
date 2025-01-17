using MQTTnet;
using MQTTnet.Formatter;
using System.Text;
using MQTTnet.Packets;
using System.Text.RegularExpressions;
using MQTTnet.Extensions;


namespace elan2mqtt.Helpers
{


    internal class MQTTConnector : IDisposable
    {
        IMqttClient? _mqttClient { get; set; }
        public static readonly string MQTTMainTopic = "eLan";
        public static readonly string MQTTStatusTopic = "status";
        public static readonly string MQTTCommandTopic = "command";
        public static readonly string MQTTSubscribeTopic = $"{MQTTMainTopic}/+/{MQTTCommandTopic}";
        public static readonly string MQTTDiscoveryTopic = "homeassistant";
        public static readonly string MQTTClientID = "eLan2MQTT_NET";

        public bool IsConnected { get { return _mqttClient == null ? false : _mqttClient.IsConnected; } }
        bool _shutDown = false;
        // Definice události
        public event EventHandler<MessageReceivedEventArgs>? MessageReceived;

        public MQTTConnector()
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="useTls"></param>
        /// <param name="protocolVersion">Supported values are 310,311 and 500</param>
        /// <returns></returns>
        public async Task Connect(string address, string username, string password, bool useTls = false, string protocolVersion = "311")
        {
            MqttProtocolVersion proVer;
            switch (protocolVersion)
            {
                case "310":
                    proVer = MqttProtocolVersion.V310;
                    break;
                case "311":
                    proVer = MqttProtocolVersion.V311;
                    break;
                case "500":
                    proVer = MqttProtocolVersion.V500;
                    break;
                default:
                    proVer = MqttProtocolVersion.V311;
                    break;
            }

            _shutDown = false;
            var mqttFactory = new MqttClientFactory();

            _mqttClient = mqttFactory.CreateMqttClient();
            var mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(address)
                .WithProtocolVersion(proVer)
                .WithCredentials(username, password)
                .WithClientId(MQTTClientID)
                .WithTlsOptions(new MqttClientTlsOptions()
                {
                    UseTls = useTls,
                    IgnoreCertificateChainErrors = true,
                    IgnoreCertificateRevocationErrors = true,
                    AllowUntrustedCertificates = true
                })
                .Build();


            // Setup message handling
            _mqttClient.ApplicationMessageReceivedAsync += async e =>
            {
                Console.WriteLine("Received application message.");
                // Convert Payload to string
                var payload = e.ApplicationMessage?.Payload == null ? null : Encoding.UTF8.GetString(e.ApplicationMessage.Payload);


                //Console.WriteLine(string.Format(
                //    " TimeStamp: {0} -- Message: ClientId = {1}, Topic = {2}, Payload = {3}, QoS = {4}, Retain-Flag = {5}\r\n\r\n",

                //    DateTime.Now,
                //    e.ClientId,
                //    e.ApplicationMessage?.Topic,
                //    payload,
                //    e.ApplicationMessage?.QualityOfServiceLevel,
                //    e.ApplicationMessage?.Retain));


                // Get device ID by regex
                //pozor, tady dostávám adresu zařízení, ne ID zařízení, protože to tsk máme nastaveno v MQTT!
                string pattern = @"^eLan/(?<deviceId>[^/]+)/command$";
                Match match = Regex.Match(e.ApplicationMessage?.Topic, pattern);
                string deviceId = string.Empty;
                if (match.Success)
                {
                    deviceId = match.Groups["deviceId"].Value;
                    //Console.WriteLine($"Device ID: {deviceId}");
                    if (payload != null)
                    {
                        MessageReceived?.Invoke(this, new MessageReceivedEventArgs(payload, deviceId));
                    }
                }
                else
                {
                    Console.WriteLine("Device ID nebylo nalezeno.");
                }


                return;
            };
            _mqttClient.ConnectedAsync += async e =>
            {
                Console.WriteLine($"The MQTT client is connected.");
                // subscribe to the topic
                await _mqttClient.SubscribeAsync(new MqttTopicFilter { Topic = MQTTSubscribeTopic }).ConfigureAwait(false);
                Console.WriteLine("The MQTT client is subscribed.");
            };
            _mqttClient.DisconnectedAsync += async e =>
            {
                // maybe failure, so try to reconnect except shutdown
                if (!_shutDown)
                {
                    Console.WriteLine("The MQTT client reconnecting.");
                    await _mqttClient.ReconnectAsync();
                }
            };

            try
            {
                // In MQTTv5 the response contains much more information.
                var response = await _mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"The MQTT client is NOT connected, error: " + ex.ToString());
            }
        }

        public async Task PublishByDeviceID(string deviceID, string payload)
        {
            if (_mqttClient != null && IsConnected)
            {
                var applicationMessage = new MqttApplicationMessageBuilder()
    .WithTopic($"{MQTTMainTopic}/{deviceID}/{MQTTStatusTopic}")
    .WithPayload(payload)
    .Build();
                Console.WriteLine($"The MQTT Publishing to topic: {applicationMessage.Topic} message: {payload.Linearize()}");
                await _mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
            }
        }
        public async Task Publish(string topic, string payload)
        {
            if (_mqttClient != null && IsConnected)
            {
                var applicationMessage = new MqttApplicationMessageBuilder()
    .WithTopic(topic)
    .WithPayload(payload)
    .Build();
                Console.WriteLine($"The MQTT Publishing to topic: {applicationMessage.Topic} message: {payload.Linearize()}");
                await _mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
            }
        }
        public async Task Publish(string topic, byte[] payload)
        {
            if (_mqttClient != null && IsConnected)
            {
                var applicationMessage = new MqttApplicationMessageBuilder().WithTopic(topic).WithPayload(payload).Build();
                Console.WriteLine($"The MQTT Publishing to topic: {applicationMessage.Topic} message: byte encoded");
                await _mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
            }
        }
        public async Task Disconnect()
        {
            _shutDown = true;
            if (_mqttClient != null && _mqttClient.IsConnected)
            {
                await _mqttClient.DisconnectAsync();
                _mqttClient.Dispose();
            }
            _mqttClient = null;
        }

        public void Dispose()
        {
            _shutDown = true;
            if (_mqttClient != null)
            {
                _mqttClient.Dispose();
            }
            _mqttClient = null;
        }
    }
}
