using elan2mqtt.Helpers;
using elan2mqtt.Model.eLan;
using HomeAssistantDiscoveryNet;

namespace elan2mqtt.Model.MQTT
{
    internal class DiscoveryHelper
    {

        public static readonly string HomeAssistantMainTopic = "homeassistant";
        public static readonly string ElkoEPManufacturer = "Elko EP";
        public async Task<MqttDiscoveryConfig> CreateDiscoveryObject(ELanDevice dev)
        {
            if (dev.DeviceType == ElanDeviceTypeEnum.Light)
            {
                if (dev.DeviceSubType == ElanDeviceSubTypeEnum.OnOff)
                {
                    var cfg = new MqttDefaultLightDiscoveryConfig
                    {
                        PayloadOff = "{\"on\":false}",
                        PayloadOn = "{\"on\":true}",
                        StateValueTemplate = "{%- if value_json.on -%}{\"on\":true}{%- else -%}{\"on\":false}{%- endif -%}",
                        Name = dev.DeviceInfo.Label,
                        UniqueId = $"{MQTTConnector.MQTTMainTopic}-{dev.DeviceInfo.Address}",
                        Device = new MqttDiscoveryDevice()
                        {
                            Name = dev.DeviceInfo.Label,
                            Manufacturer = ElkoEPManufacturer,
                            Model = dev.DeviceInfo.ProductType,
                            Connections = new List<List<string>>()
                        {
                            new List<string>() { "mac", dev.DeviceInfo.Address.ToString() }
                        },
                            Identifiers = new List<string>() { $"eLan-light-{dev.DeviceInfo.Address}" }
                        },
                        CommandTopic = $"{MQTTConnector.MQTTMainTopic}/{dev.DeviceInfo.Address}/{MQTTConnector.MQTTCommandTopic}",
                        StateTopic = $"{MQTTConnector.MQTTMainTopic}/{dev.DeviceInfo.Address}/{MQTTConnector.MQTTStatusTopic}",
                        JsonAttributesTopic = $"{MQTTConnector.MQTTMainTopic}/{dev.DeviceInfo.Address}/{MQTTConnector.MQTTStatusTopic}",
                        Icon = "mdi:lightbulb"


                    };
                    return cfg;
                }
                else if (dev.DeviceSubType == ElanDeviceSubTypeEnum.Dimmer)
                {
                    var cfg = new MqttTemplateLightDiscoveryConfig
                    {
                        
                        Name = dev.DeviceInfo.Label,
                        UniqueId = $"{MQTTConnector.MQTTMainTopic}-{dev.DeviceInfo.Address}",
                        Device = new MqttDiscoveryDevice()
                        {
                            Name = dev.DeviceInfo.Label,
                            Manufacturer = ElkoEPManufacturer,
                            Model = dev.DeviceInfo.ProductType,
                            Connections = new List<List<string>>()
                        {
                            new List<string>() { "mac", dev.DeviceInfo.Address.ToString() }
                        },
                            Identifiers = new List<string>() { $"eLan-dimmer-{dev.DeviceInfo.Address}" }
                        },
                        CommandTopic = $"{MQTTConnector.MQTTMainTopic}/{dev.DeviceInfo.Address}/{MQTTConnector.MQTTCommandTopic}",
                        StateTopic = $"{MQTTConnector.MQTTMainTopic}/{dev.DeviceInfo.Address}/{MQTTConnector.MQTTStatusTopic}",
                        JsonAttributesTopic = $"{MQTTConnector.MQTTMainTopic}/{dev.DeviceInfo.Address}/{MQTTConnector.MQTTStatusTopic}",
                        CommandOnTemplate = "{%- if brightness is defined -%} {\"brightness\":{{ (brightness * " + dev.ActionsInfo.Brightness.Max + " / 255 ) | int }}} {%- else -%} {\"brightness\":100} {%- endif -%}",
                        CommandOffTemplate = "{\"brightness\": 0 }",
                        BrightnessTemplate = "{{ (value_json.brightness * 255 / " + dev.ActionsInfo.Brightness.Max + ") | int }}",
                        StateTemplate = "{%- if value_json.brightness > 0 -%}on{%- else -%}off{%- endif -%}"
                    };
                    return cfg;
                }
            }
            return null;
        }

        public static string GetDiscoveryTopic(ELanDevice dev)
        {
            if (dev.DeviceType == ElanDeviceTypeEnum.Light)
            {
                return $"{HomeAssistantMainTopic}/light/{dev.DeviceInfo.Address}/config";
            }
            else if (dev.DeviceType == ElanDeviceTypeEnum.Switch)
            {
                return $"{HomeAssistantMainTopic}/switch/{dev.DeviceInfo.Address}/config";
            }
            else if (dev.DeviceType == ElanDeviceTypeEnum.Heating)
            {
                return $"{HomeAssistantMainTopic}/climate/{dev.DeviceInfo.Address}/config";
            }
            else if (dev.DeviceType == ElanDeviceTypeEnum.Thermometer)
            {
                return $"{HomeAssistantMainTopic}/sensor/{dev.DeviceInfo.Address}/config";
            }
            else if (dev.DeviceType == ElanDeviceTypeEnum.Detector)
            {
                return $"{HomeAssistantMainTopic}/sensor/{dev.DeviceInfo.Address}/config";
            }

            return string.Empty;
        }
    }
}
