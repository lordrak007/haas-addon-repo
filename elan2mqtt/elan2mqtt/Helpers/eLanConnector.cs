using elan2mqtt.Model.eLan;
using elan2mqtt.Model.MQTT;
using MQTTnet.Extensions;
using RestSharp;
using System.Net;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using Websocket.Client;

namespace elan2mqtt.Helpers
{
    internal class eLanConnector : IDisposable
    {
        //     keep_alive_interval = 1 * 60  # interval between mandatory messages to keep connections open (and to renew session) in s (eLan session expires in 0.5 h)
        CookieContainer sharedCookie = new CookieContainer();
        string _pass = string.Empty;
        string _username = string.Empty;
        string _eLanAddress = string.Empty;
        RestClient _restClient;
        WebsocketClient _wsClient;
        ElanDevices _eLanDevices = new ElanDevices();
        System.Timers.Timer _MQTTDiscoveryTimer = new System.Timers.Timer();
        System.Timers.Timer _MQTTUpdateTimer = new System.Timers.Timer();

        static string Hash(string input) => Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(input)));
        public eLanConnector(string address, string username, string password)
        {
            _username = username;
            _eLanAddress = address;
            _pass = password;

            _restClient = initClient($"http://{address}");

            _MQTTDiscoveryTimer.Interval = 1000 * 60 * 10; // 10 minutes
            _MQTTUpdateTimer.Interval = 1000 * 60 * 1; // 1 minute
            _MQTTDiscoveryTimer.Elapsed += async (sender, e) => { await UpdateDeviceListFromeLan(); generateMQTTDiscoveryMessages(); };
            _MQTTUpdateTimer.Elapsed += async (sender, e) => generateElanStatusMessages();
            _MQTTDiscoveryTimer.Start();
            _MQTTUpdateTimer.Start();

            Console.WriteLine("eLan connector initialized");
        }

        async Task generateMQTTDiscoveryMessages()
        {
            DiscoveryHelper dh = new DiscoveryHelper();
            // discovery processing
            if (true)
            {
                _eLanDevices.ForEach(d =>
                {
                    var json = dh.CreateDiscoveryObject(d).Result;
                    if (json != null)
                    {
                        ElanDeviceDiscoveryEventReceived?.Invoke(this, new ElanDeviceDiscoveryEventReceivedEventArgs(DiscoveryHelper.GetDiscoveryTopic(d), json.ToJsonBxtes()));
                    }
                });
            }
        }

        async Task generateElanStatusMessages()
        {
            _eLanDevices.ForEach(d =>
            {
                var payload = GetElanDeviceState(d).Result;
                if (payload != null && d.DeviceInfo != null)
                {
                    ElanStatusEventReceived?.Invoke(this, new ElanStatusEventReceivedEventArgs(payload, d.DeviceInfo.Address.ToString()));
                }
            });
        }

        public async void Connect()
        {
            DiscoveryHelper dh = new DiscoveryHelper();
            await login(_restClient, _username, _pass);
            await UpdateDeviceListFromeLan();
            // discovery processing
            generateMQTTDiscoveryMessages();
            // status update - forced at connect
            generateElanStatusMessages();
            connectWebSocket(_eLanAddress);

        }


        RestClient initClient(string URI)
        {
            var options = new RestClientOptions(URI)
            {
                CookieContainer = sharedCookie
            };
            return new RestClient(options);
        }
        async Task login(RestClient client, string username, string password)
        {
            Console.WriteLine("eLan - trying log in");
            var r = new RestRequest("login");
            var h = Hash(password).ToLower();
            r.AddBody($"name={username}&key={h}");
            try
            {
                var response = await client.PostAsync(r);
                // Add cookies accessible for all other connections
                client.Options.CookieContainer.Add(response?.Cookies.FirstOrDefault(c => c.Name == "AuthAPI"));
                Console.WriteLine("eLan - logged in");

            }
            catch (Exception ex)
            {
                Console.WriteLine("eLan - can not log in. " + ex.Message);
                Thread.Sleep(5000);
                await login(client, username, password);
            }

        }

        async Task connectWebSocket(string address)
        {
            if (_wsClient != null)
            {
                await _wsClient.Stop(WebSocketCloseStatus.NormalClosure, "Reconnecting");
                _wsClient.Dispose();
            }
            var url = new Uri($"ws://{address}/api/ws");
            var factory = new Func<ClientWebSocket>(() => new ClientWebSocket
            {
                Options =
                    {
                    KeepAliveInterval = TimeSpan.FromSeconds(5),
                    Cookies = sharedCookie
                }
            });

            _wsClient = new WebsocketClient(url, factory);
            _wsClient.MessageReceived.Subscribe(ProcessWebSocketMessage);
            _wsClient.DisconnectionHappened.Subscribe(OnWebSocketDisconnected);
            //_wsClient.ReconnectTimeout = TimeSpan.FromSeconds(10);
            _wsClient.ReconnectionHappened.Subscribe(OnWebSocketReconnected);
            await _wsClient.Start();
            Console.WriteLine("WS connected");

        }
        void ProcessWebSocketMessage(ResponseMessage msg)
        {
            //Console.WriteLine($"eLan: msg.Text: {msg.Text},  msg.MessageType: {msg.MessageType}");
            if (!string.IsNullOrWhiteSpace(msg.Text))
            {
                var jsonDevice = JsonNode.Parse(msg.Text).AsObject();
                string device = jsonDevice["device"].ToString();
                string payload = jsonDevice["result"].ToString();
                var dev = _eLanDevices.Where(d => d.Id == device).FirstOrDefault();
                Console.WriteLine($"************* eLan: {dev.DeviceInfo.Address} ({dev.DeviceInfo.Label}),  data: {payload.Linearize()}");
                ElanStatusEventReceived?.Invoke(this, new ElanStatusEventReceivedEventArgs(payload, dev.DeviceInfo.Address.ToString()));
            }
        }

        void OnWebSocketDisconnected(DisconnectionInfo info)
        {
            Console.WriteLine($"WS disconnected: {info.CloseStatusDescription}, ex: {info.Exception}");
            if (info.Exception != null && info.Exception.Message.Contains("status code '401'"))
            {
                Console.WriteLine("WS - trying to relogin");
                login(_restClient, _username, _pass).Wait();
                connectWebSocket(_eLanAddress);
            }
        }

        void OnWebSocketReconnected(ReconnectionInfo info)
        {
            Console.WriteLine($"WS reconnected: {info.Type}");
        }


        public event EventHandler<ElanStatusEventReceivedEventArgs>? ElanStatusEventReceived;
        public event EventHandler<ElanDeviceDiscoveryEventReceivedEventArgs>? ElanDeviceDiscoveryEventReceived;

        async public Task SetDeviceStateAsync(ELanDevice device, string payload)
        {
            // jak udělám volání PUT?
            var req = new RestRequest(new Uri(device.DeviceURL));
            req.AddJsonBody(payload);
            await _restClient.PutAsync(req);
            //Console.WriteLine();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="address">Physical address of the Actor</param>
        /// <param name="payload">Playload data</param>
        /// <returns></returns>
        async public Task SetDeviceStateAsync(string address, string payload)
        {
            var dev = _eLanDevices.Where(d => d.DeviceInfo?.Address.ToString() == address).FirstOrDefault();
            if (dev != null)
            {
                await SetDeviceStateAsync(dev, payload);
            }
            else
            {
                Console.WriteLine($"REST - got update for non exist device in eLan, ID: {address}, payload: {payload.Linearize()}");
            }
        }
        /// <summary>
        /// Connect to eLan, get list of devices (and add missing) and update ID and update their properties
        /// </summary>
        /// <returns></returns>
        public async Task UpdateDeviceListFromeLan()
        {
            await _restClient.GetAsync(new RestRequest("api/devices", Method.Get) { Timeout = new TimeSpan(0, 0, 3) }).ContinueWith((task) =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    var response = task.Result;
                    if (response.IsSuccessful)
                    {
                        var jsonDevices = JsonNode.Parse(response.Content).AsObject();
                        foreach (var dev in jsonDevices)
                        {
                            var existingDevice = this._eLanDevices.FirstOrDefault(d => d.Id == Convert.ToString(dev.Key));
                            if (existingDevice == null)
                            {
                                this._eLanDevices.Add(new ELanDevice(dev.Key, jsonDevices[dev.Key]["url"].ToString()));
                            }
                        }
                    }
                }
            });
            // update properties
            foreach (var dev in this._eLanDevices)
            {
                await UpdateDevicePropertiesFromeLan(dev);
            }
        }
        /// <summary>
        /// Update device properties from eLan
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public async Task UpdateDevicePropertiesFromeLan(ELanDevice device)
        {
            if (device.DeviceURL == null)
            {
                Console.WriteLine($"eLan Device {device.Id} has no Device URL, cannot update device info");
                return;
            }
            await _restClient.GetAsync(new RestRequest(device.DeviceURL, Method.Get) { Timeout = new TimeSpan(0, 0, 3) }).ContinueWith((task) =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    var response = task.Result;
                    if (response.IsSuccessful)
                    {
                        device.UpdateDevice(response.Content);
                    }
                }
            });
        }

        public async Task<string> GetElanDeviceState(ELanDevice device)
        {
            // get state of the device
            if (device.StateURL == null)
            {
                Console.WriteLine($"Device {device.Id} has no State URL, cannot get Device State");
                return string.Empty;
            }
            var response = await _restClient.GetAsync(new RestRequest(device.StateURL, Method.Get) { Timeout = new TimeSpan(0, 0, 3) });
            Console.WriteLine($"eLan get device state: {device.Id} , {response.Content.Linearize()}");
            return response.Content;
        }

        public void Dispose()
        {
            _MQTTDiscoveryTimer.Stop();
            _MQTTUpdateTimer.Stop();
            _wsClient?.Stop(WebSocketCloseStatus.NormalClosure, "Disposing");
            _wsClient?.Dispose();
            _restClient?.Dispose();
        }
    }
}
