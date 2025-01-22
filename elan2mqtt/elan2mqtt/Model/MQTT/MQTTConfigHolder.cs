using HomeAssistantDiscoveryNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elan2mqtt.Model.MQTT
{
    internal class MQTTConfigHolder
    {
        public MQTTConfigHolder() { }
        public MQTTConfigHolder(MqttDiscoveryConfig mqttConfig, string topic) {
            MqttConfig = mqttConfig;
            Topic = topic;
        }
        public MqttDiscoveryConfig MqttConfig { get; set; }
        public string Topic { get; set; }
    }
}
