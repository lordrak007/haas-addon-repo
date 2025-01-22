using elan2mqtt.Helpers;
using elan2mqtt.Model.eLan;
using HomeAssistantDiscoveryNet;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace elan2mqtt.Model.MQTT
{
    internal class DiscoveryHelper
    {
        public static readonly string HomeAssistantMainTopic = "homeassistant";
        public static readonly string ElkoEPManufacturer = "Elko EP";
        public async Task<List<MQTTConfigHolder>> CreateDiscoveryObject(ELanDevice dev)
        {
            List<MQTTConfigHolder> cfgList = new List<MQTTConfigHolder>();

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

                    };
                    cfgList.Add(new MQTTConfigHolder() { MqttConfig = cfg, Topic = ConstructDicoveryTopic(cfg.Component, $"{dev.DeviceInfo.Address}") });
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
                        //JsonAttributesTopic = $"{MQTTConnector.MQTTMainTopic}/{dev.DeviceInfo.Address}/{MQTTConnector.MQTTStatusTopic}",
                        CommandOnTemplate = "{%- if brightness is defined -%} {\"brightness\":{{ (brightness * " + dev.ActionsInfo.Brightness.Max + " / 255 ) | int }}} {%- else -%} {\"brightness\":100} {%- endif -%}",
                        CommandOffTemplate = "{\"brightness\": 0 }",
                        BrightnessTemplate = "{{ (value_json.brightness * 255 / " + dev.ActionsInfo.Brightness.Max + ") | int }}",
                        StateTemplate = "{%- if value_json.brightness > 0 -%}on{%- else -%}off{%- endif -%}"
                    };
                    cfgList.Add(new MQTTConfigHolder() { MqttConfig = cfg, Topic = ConstructDicoveryTopic(cfg.Component, $"{dev.DeviceInfo.Address}") });
                }
            }
            else if (dev.DeviceType == ElanDeviceTypeEnum.Switch)
            {
                var cfg = new MqttSwitchDiscoveryConfig()
                {
                    PayloadOff = "{\"on\":false}",
                    PayloadOn = "{\"on\":true}",
                    StateOff = "off",
                    StateOn = "on",
                    ValueTemplate = "{%- if value_json.on -%}on{%- else -%}off{%- endif -%}",
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
                        Identifiers = new List<string>() { $"eLan-switch-{dev.DeviceInfo.Address}" }
                    },
                    CommandTopic = $"{MQTTConnector.MQTTMainTopic}/{dev.DeviceInfo.Address}/{MQTTConnector.MQTTCommandTopic}",
                    StateTopic = $"{MQTTConnector.MQTTMainTopic}/{dev.DeviceInfo.Address}/{MQTTConnector.MQTTStatusTopic}",
                    JsonAttributesTopic = $"{MQTTConnector.MQTTMainTopic}/{dev.DeviceInfo.Address}/{MQTTConnector.MQTTStatusTopic}",
                };
                cfgList.Add(new MQTTConfigHolder() { MqttConfig = cfg, Topic = ConstructDicoveryTopic(cfg.Component, $"{dev.DeviceInfo.Address}") });
            }
            else if (dev.DeviceType == ElanDeviceTypeEnum.Heating)
            {
                var cfgIN = new MqttSensorDiscoveryConfig()
                {
                    Name = dev.DeviceInfo.Label + "-IN",
                    UniqueId = $"{MQTTConnector.MQTTMainTopic}-{dev.DeviceInfo.Address}-IN",
                    Device = new MqttDiscoveryDevice()
                    {
                        Name = dev.DeviceInfo.Label,
                        Manufacturer = ElkoEPManufacturer,
                        Model = dev.DeviceInfo.ProductType,
                        Connections = new List<List<string>>()
                        {
                            new List<string>() { "mac", dev.DeviceInfo.Address.ToString() }
                        },
                        Identifiers = new List<string>() { $"eLan-thermostat-{dev.DeviceInfo.Address}" }
                    },
                    StateTopic = $"{MQTTConnector.MQTTMainTopic}/{dev.DeviceInfo.Address}/{MQTTConnector.MQTTStatusTopic}",
                    JsonAttributesTopic = $"{MQTTConnector.MQTTMainTopic}/{dev.DeviceInfo.Address}/{MQTTConnector.MQTTStatusTopic}",
                    DeviceClass = "temperature",
                    ValueTemplate = "{{ value_json[\"temperature IN\"] }}",
                    UnitOfMeasurement = "°C"
                };
                cfgList.Add(new MQTTConfigHolder() { MqttConfig = cfgIN, Topic = ConstructDicoveryTopic(cfgIN.Component, $"{dev.DeviceInfo.Address}/IN") });

                var cfgOUT = new MqttSensorDiscoveryConfig()
                {
                    Name = dev.DeviceInfo.Label + "-OUT",
                    UniqueId = $"{MQTTConnector.MQTTMainTopic}-{dev.DeviceInfo.Address}-OUT",
                    Device = new MqttDiscoveryDevice()
                    {
                        Name = dev.DeviceInfo.Label,
                        Manufacturer = ElkoEPManufacturer,
                        Model = dev.DeviceInfo.ProductType,
                        Connections = new List<List<string>>()
                        {
                            new List<string>() { "mac", dev.DeviceInfo.Address.ToString() }
                        },
                        Identifiers = new List<string>() { $"eLan-thermostat-{dev.DeviceInfo.Address}" }
                    },
                    StateTopic = $"{MQTTConnector.MQTTMainTopic}/{dev.DeviceInfo.Address}/{MQTTConnector.MQTTStatusTopic}",
                    JsonAttributesTopic = $"{MQTTConnector.MQTTMainTopic}/{dev.DeviceInfo.Address}/{MQTTConnector.MQTTStatusTopic}",
                    DeviceClass = "temperature",
                    ValueTemplate = "{{ value_json[\"temperature OUT\"] }}",
                    UnitOfMeasurement = "°C"

                };
                cfgList.Add(new MQTTConfigHolder() { MqttConfig = cfgOUT, Topic = ConstructDicoveryTopic(cfgOUT.Component, $"{dev.DeviceInfo.Address}/OUT") });

                var cfgON = new MqttSensorDiscoveryConfig()
                {
                    Name = dev.DeviceInfo.Label + "-ON",
                    UniqueId = $"{MQTTConnector.MQTTMainTopic}-{dev.DeviceInfo.Address}-ON",
                    Device = new MqttDiscoveryDevice()
                    {
                        Name = dev.DeviceInfo.Label,
                        Manufacturer = ElkoEPManufacturer,
                        Model = dev.DeviceInfo.ProductType,
                        Connections = new List<List<string>>()
                        {
                            new List<string>() { "mac", dev.DeviceInfo.Address.ToString() }
                        },
                        Identifiers = new List<string>() { $"eLan-thermostat-{dev.DeviceInfo.Address}" }
                    },
                    StateTopic = $"{MQTTConnector.MQTTMainTopic}/{dev.DeviceInfo.Address}/{MQTTConnector.MQTTStatusTopic}",
                    JsonAttributesTopic = $"{MQTTConnector.MQTTMainTopic}/{dev.DeviceInfo.Address}/{MQTTConnector.MQTTStatusTopic}",
                    ValueTemplate = "{%- if value_json.on -%}on{%- else -%}off{%- endif -%}"
                };
                cfgList.Add(new MQTTConfigHolder() { MqttConfig = cfgON, Topic = ConstructDicoveryTopic(cfgON.Component, $"{dev.DeviceInfo.Address}/ON") });
            }
            else if (dev.DeviceType == ElanDeviceTypeEnum.Thermometer)
            {
                var cfgIN = new MqttSensorDiscoveryConfig()
                {
                    Name = dev.DeviceInfo.Label + "-IN",
                    UniqueId = $"{MQTTConnector.MQTTMainTopic}-{dev.DeviceInfo.Address}-IN",
                    Device = new MqttDiscoveryDevice()
                    {
                        Name = dev.DeviceInfo.Label,
                        Manufacturer = ElkoEPManufacturer,
                        Model = dev.DeviceInfo.ProductType,
                        Connections = new List<List<string>>()
                        {
                            new List<string>() { "mac", dev.DeviceInfo.Address.ToString() }
                        },
                        Identifiers = new List<string>() { $"eLan-thermometer-{dev.DeviceInfo.Address}" }
                    },
                    StateTopic = $"{MQTTConnector.MQTTMainTopic}/{dev.DeviceInfo.Address}/{MQTTConnector.MQTTStatusTopic}",
                    JsonAttributesTopic = $"{MQTTConnector.MQTTMainTopic}/{dev.DeviceInfo.Address}/{MQTTConnector.MQTTStatusTopic}",
                    DeviceClass = "temperature",
                    ValueTemplate = "{{ value_json[\"temperature IN\"] }}",
                    UnitOfMeasurement = "°C"

                };
                cfgList.Add(new MQTTConfigHolder() { MqttConfig = cfgIN, Topic = ConstructDicoveryTopic(cfgIN.Component, $"{dev.DeviceInfo.Address}/IN") });

                var cfgOUT = new MqttSensorDiscoveryConfig()
                {
                    Name = dev.DeviceInfo.Label + "-OUT",
                    UniqueId = $"{MQTTConnector.MQTTMainTopic}-{dev.DeviceInfo.Address}-OUT",
                    Device = new MqttDiscoveryDevice()
                    {
                        Name = dev.DeviceInfo.Label,
                        Manufacturer = ElkoEPManufacturer,
                        Model = dev.DeviceInfo.ProductType,
                        Connections = new List<List<string>>()
                        {
                            new List<string>() { "mac", dev.DeviceInfo.Address.ToString() }
                        },
                        Identifiers = new List<string>() { $"eLan-thermometer-{dev.DeviceInfo.Address}" }
                    },
                    StateTopic = $"{MQTTConnector.MQTTMainTopic}/{dev.DeviceInfo.Address}/{MQTTConnector.MQTTStatusTopic}",
                    JsonAttributesTopic = $"{MQTTConnector.MQTTMainTopic}/{dev.DeviceInfo.Address}/{MQTTConnector.MQTTStatusTopic}",
                    DeviceClass = "temperature",
                    ValueTemplate = "{{ value_json[\"temperature OUT\"] }}",
                    UnitOfMeasurement = "°C"

                };
                cfgList.Add(new MQTTConfigHolder() { MqttConfig = cfgOUT, Topic = ConstructDicoveryTopic(cfgOUT.Component, $"{dev.DeviceInfo.Address}/OUT") });

            }
            else if (dev.DeviceType == ElanDeviceTypeEnum.Detector)
            {
                string icon = string.Empty;
                switch (dev.DeviceSubType)
                {
                    case ElanDeviceSubTypeEnum.Window:
                        icon = "mdi:window-open";
                        break;
                    case ElanDeviceSubTypeEnum.Smoke:
                        icon = "mdi:smoke-detector";
                        break;
                    case ElanDeviceSubTypeEnum.Motion:
                        icon = "mdi:motion-sensor";
                        break;
                    case ElanDeviceSubTypeEnum.Flood:
                        icon = "mdi:waves";
                        break;
                }

                // "detect" acting
                var cfgDetect = new MqttSensorDiscoveryConfig()
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
                        Identifiers = new List<string>() { $"eLan-detector-{dev.DeviceInfo.Address}" }
                    },
                    StateTopic = $"{MQTTConnector.MQTTMainTopic}/{dev.DeviceInfo.Address}/{MQTTConnector.MQTTStatusTopic}",
                    JsonAttributesTopic = $"{MQTTConnector.MQTTMainTopic}/{dev.DeviceInfo.Address}/{MQTTConnector.MQTTStatusTopic}",
                    ValueTemplate = "{%- if value_json.detect -%}on{%- else -%}off{%- endif -%}",
                    Icon = icon
                };
                cfgList.Add(new MQTTConfigHolder() { MqttConfig = cfgDetect, Topic = ConstructDicoveryTopic(cfgDetect.Component, $"{dev.DeviceInfo.Address}") });

                // battery status
                var cfgBattery = new MqttSensorDiscoveryConfig()
                {
                    Name = dev.DeviceInfo.Label + " battery",
                    UniqueId = $"{MQTTConnector.MQTTMainTopic}-{dev.DeviceInfo.Address}-battery",
                    Device = new MqttDiscoveryDevice()
                    {
                        Name = dev.DeviceInfo.Label,
                        Manufacturer = ElkoEPManufacturer,
                        Model = dev.DeviceInfo.ProductType,
                        Connections = new List<List<string>>()
                        {
                            new List<string>() { "mac", dev.DeviceInfo.Address.ToString() }
                        },
                        Identifiers = new List<string>() { $"eLan-detector-{dev.DeviceInfo.Address}" }
                    },
                    StateTopic = $"{MQTTConnector.MQTTMainTopic}/{dev.DeviceInfo.Address}/{MQTTConnector.MQTTStatusTopic}",
                    JsonAttributesTopic = $"{MQTTConnector.MQTTMainTopic}/{dev.DeviceInfo.Address}/{MQTTConnector.MQTTStatusTopic}",
                    DeviceClass = "battery",
                    ValueTemplate = "{%- if value_json.battery -%}100{%- else -%}0{%- endif -%}"
                };
                cfgList.Add(new MQTTConfigHolder() { MqttConfig = cfgBattery, Topic = ConstructDicoveryTopic(cfgBattery.Component, $"{dev.DeviceInfo.Address}/battery") });


                // START - RFWD window/door detector

                if (dev.DeviceInfo.ProductType == "RFWD-100" || dev.DeviceInfo.ProductType == "RFSF-1B")
                {
                    var cfgAlarm = new MqttSensorDiscoveryConfig()
                    {
                        Name = dev.DeviceInfo.Label + " alarm",
                        UniqueId = $"{MQTTConnector.MQTTMainTopic}-{dev.DeviceInfo.Address}-alarm",
                        Device = new MqttDiscoveryDevice()
                        {
                            Name = dev.DeviceInfo.Label,
                            Manufacturer = ElkoEPManufacturer,
                            Model = dev.DeviceInfo.ProductType,
                            Connections = new List<List<string>>()
                        {
                            new List<string>() { "mac", dev.DeviceInfo.Address.ToString() }
                        },
                            Identifiers = new List<string>() { $"eLan-detector-{dev.DeviceInfo.Address}" }
                        },
                        StateTopic = $"{MQTTConnector.MQTTMainTopic}/{dev.DeviceInfo.Address}/{MQTTConnector.MQTTStatusTopic}",
                        JsonAttributesTopic = $"{MQTTConnector.MQTTMainTopic}/{dev.DeviceInfo.Address}/{MQTTConnector.MQTTStatusTopic}",
                        ValueTemplate = "{%- if value_json.alarm -%}on{%- else -%}off{%- endif -%}",
                        Icon = "mdi:alarm-light"
                    };
                    cfgList.Add(new MQTTConfigHolder() { MqttConfig = cfgAlarm, Topic = ConstructDicoveryTopic(cfgAlarm.Component, $"{dev.DeviceInfo.Address}/alarm") });
                }
                if (dev.DeviceInfo.ProductType == "RFWD-100")
                {
                    // tamper
                    var cfgTamper = new MqttSensorDiscoveryConfig()
                    {
                        Name = dev.DeviceInfo.Label + " tamper",
                        UniqueId = $"{MQTTConnector.MQTTMainTopic}-{dev.DeviceInfo.Address}-tamper",
                        Device = new MqttDiscoveryDevice()
                        {
                            Name = dev.DeviceInfo.Label,
                            Manufacturer = ElkoEPManufacturer,
                            Model = dev.DeviceInfo.ProductType,
                            Connections = new List<List<string>>()
                        {
                            new List<string>() { "mac", dev.DeviceInfo.Address.ToString() }
                        },
                            Identifiers = new List<string>() { $"eLan-detector-{dev.DeviceInfo.Address}" }
                        },
                        StateTopic = $"{MQTTConnector.MQTTMainTopic}/{dev.DeviceInfo.Address}/{MQTTConnector.MQTTStatusTopic}",
                        JsonAttributesTopic = $"{MQTTConnector.MQTTMainTopic}/{dev.DeviceInfo.Address}/{MQTTConnector.MQTTStatusTopic}",
                        ValueTemplate = "{%- if value_json.alarm -%}on{%- else -%}off{%- endif -%}",
                        Icon = "mdi:gesture-tap"
                    };
                    cfgList.Add(new MQTTConfigHolder() { MqttConfig = cfgTamper, Topic = ConstructDicoveryTopic(cfgTamper.Component, $"{dev.DeviceInfo.Address}/tamper") });
                    // automat
                    var cfgAutomat = new MqttSensorDiscoveryConfig()
                    {
                        Name = dev.DeviceInfo.Label + " automat",
                        UniqueId = $"{MQTTConnector.MQTTMainTopic}-{dev.DeviceInfo.Address}-automat",
                        Device = new MqttDiscoveryDevice()
                        {
                            Name = dev.DeviceInfo.Label,
                            Manufacturer = ElkoEPManufacturer,
                            Model = dev.DeviceInfo.ProductType,
                            Connections = new List<List<string>>()
                        {
                            new List<string>() { "mac", dev.DeviceInfo.Address.ToString() }
                        },
                            Identifiers = new List<string>() { $"eLan-detector-{dev.DeviceInfo.Address}" }
                        },
                        StateTopic = $"{MQTTConnector.MQTTMainTopic}/{dev.DeviceInfo.Address}/{MQTTConnector.MQTTStatusTopic}",
                        JsonAttributesTopic = $"{MQTTConnector.MQTTMainTopic}/{dev.DeviceInfo.Address}/{MQTTConnector.MQTTStatusTopic}",
                        ValueTemplate = "{%- if value_json.automat -%}on{%- else -%}off{%- endif -%}",
                        Icon = "mdi:arrow-decision-auto"
                    };
                    cfgList.Add(new MQTTConfigHolder() { MqttConfig = cfgAutomat, Topic = ConstructDicoveryTopic(cfgAutomat.Component, $"{dev.DeviceInfo.Address}/automat") });

                    //disarm
                    var cfgDisarm = new MqttSensorDiscoveryConfig()
                    {
                        Name = dev.DeviceInfo.Label + " disarm",
                        UniqueId = $"{MQTTConnector.MQTTMainTopic}-{dev.DeviceInfo.Address}-disarm",
                        Device = new MqttDiscoveryDevice()
                        {
                            Name = dev.DeviceInfo.Label,
                            Manufacturer = ElkoEPManufacturer,
                            Model = dev.DeviceInfo.ProductType,
                            Connections = new List<List<string>>()
                        {
                            new List<string>() { "mac", dev.DeviceInfo.Address.ToString() }
                        },
                            Identifiers = new List<string>() { $"eLan-detector-{dev.DeviceInfo.Address}" }
                        },
                        StateTopic = $"{MQTTConnector.MQTTMainTopic}/{dev.DeviceInfo.Address}/{MQTTConnector.MQTTStatusTopic}",
                        JsonAttributesTopic = $"{MQTTConnector.MQTTMainTopic}/{dev.DeviceInfo.Address}/{MQTTConnector.MQTTStatusTopic}",
                        ValueTemplate = "{%- if value_json.disarm -%}on{%- else -%}off{%- endif -%}",
                        Icon = "mdi:lock-alert"
                    };
                    cfgList.Add(new MQTTConfigHolder() { MqttConfig = cfgDisarm, Topic = ConstructDicoveryTopic(cfgDisarm.Component, $"{dev.DeviceInfo.Address}/disarm") });

                }
            }
            return cfgList;
        }
        /// <summary>
        /// create topic to where put the device config
        /// </summary>
        /// <param name="component">component type eg. light, sensor, etc</param>
        /// <param name="deviceMAC"Unique identifier of the device></param>
        /// <returns></returns>
        public static string ConstructDicoveryTopic(string component, string deviceMAC)
        {
            return $"{HomeAssistantMainTopic}/{component}/{deviceMAC}/config";
        }
    }
}


/*
 
 ##########################################################################################
            # Device info library
            ##########################################################################################
            #
            ##########################################################################################
            # RFUS-61 - singel channel multi function relay
            ##########################################################################################
            # {"device info":{"type":"appliance","product type":"RFUS-61","address":123456,"label":"xxxx","vote":false},
            # 	"actions info": {
            # 		"on": {
            # 			"type": "bool"
            # 		},
            # 		"delayed off": {
            # 			"type": null
            # 		},
            # 		"delayed on": {
            # 			"type": null
            # 		},
            # 		"delayed off: set time": {
            # 			"type": "int",
            # 			"min": 2,
            # 			"max": 3600,
            # 			"step": 1
            # 		},
            # 		"delayed on: set time": {
            # 			"type": "int",
            # 			"min": 2,
            # 			"max": 3600,
            # 			"step": 1
            # 		},
            # 		"automat": {
            # 			"type": "bool"
            # 		} 
            # 	},
            # 	"primary actions": ["on"],
            # 	"secondary actions": [["delayed off", "delayed off: set time"], ["delayed on", "delayed on: set time"],"automat"],
            # 	"settings": {
            # 	"delayed off: set time": 2400,
            # 	"delayed on: set time": 2
            # 	},"id":"13212"}
            #
            # State:
            # {
            # 	"on": false,
            # 	"delay": false,
            # 	"automat": false,
            # 	"locked": false,
            # 	"delayed off: set time": 2400,
            # 	"delayed on: set time": 2
            # }

            ##########################################################################################
            # RFSA-66M - six channel multifunction relay (each channel is reported as separate device)
            ########################################################################################
            # {"id":"16619","device info":{"address":123456,"label":"xxxxx","type":"irrigation","product type":"RFSA-66M"},
            # 	"actions info": {
            # 		"on": {
            # 			"type": "bool"
            # 		},
            # 		"delayed off": {
            # 			"type": null
            # 		},
            # 		"delayed on": {
            # 			"type": null
            # 		},
            # 		"delayed off: set time": {
            # 			"type": "int",
            # 			"min": 2,
            # 			"max": 3600,
            # 			"step": 1
            # 		},
            # 		"delayed on: set time": {
            # 			"type": "int",
            # 			"min": 2,
            # 			"max": 3600,
            # 			"step": 1
            # 		},
            # 		"automat": {
            # 			"type": "bool"
            # 		} 
            # 	},
            # 	"primary actions": ["on"],
            # 	"secondary actions": [["delayed off", "delayed off: set time"], ["delayed on", "delayed on: set time"],"automat"],
            # 	"settings": {
            # 	"delayed off: set time": 1800,
            # 	"delayed on: set time": 0
            # 	}
            # }
            # State:
            # {
            # 	"on": false,
            # 	"delay": false,
            # 	"automat": false,
            # 	"locked": false,
            # 	"delayed off: set time": 1800,
            # 	"delayed on: set time": 0
            # }
            ##########################################################################################
            # RFSA-11B - single channel single function relay 
            ########################################################################################
            # {"id":"18457","device info":{"address":123456,"label":"abc","type":"appliance","product type":"RFSA-11B"},
            # 	"actions info": {
            # 		"on": {
            # 			"type": "bool"
            # 		},
            # 		"automat": {
            # 			"type": "bool"
            # 		} 
            # 	},
            # 	"primary actions": ["on"],
            # 	"secondary actions": ["automat"],
            # 	"settings": {}
            # }
            # State:
            # {
            # 	"on": true,
            # 	"automat": true,
            # 	"locked": false
            # }
            ##########################################################################################
            # RFSA-62B - dual channel multifunction relay 
            ########################################################################################
            # {
            # 	"id": "43124","device info":{"type":"appliance","product type":"RFSA-62B","address":123456,"label":"abc"},
            # 	"actions info": {
            # 		"on": {
            # 			"type": "bool"
            # 		},
            # 		"delayed off": {
            # 			"type": null
            # 		},
            # 		"delayed on": {
            # 			"type": null
            # 		},
            # 		"delayed off: set time": {
            # 			"type": "int",
            # 			"min": 2,
            # 			"max": 3600,
            # 			"step": 1
            # 		},
            # 		"delayed on: set time": {
            # 			"type": "int",
            # 			"min": 2,
            # 			"max": 3600,
            # 			"step": 1
            # 		},
            # 		"automat": {
            # 			"type": "bool"
            # 		} 
            # 	},
            # 	"primary actions": ["on"],
            # 	"secondary actions": [["delayed off", "delayed off: set time"], ["delayed on", "delayed on: set time"],"automat"],
            # 	"settings": {
            # 	"delayed off: set time": 15,
            # 	"delayed on: set time": 0
            # 	}
            # }
            # State:
            # {
            # 	"on": false,
            # 	"delay": false,
            # 	"automat": false,
            # 	"locked": false,
            # 	"delayed off: set time": 15,
            # 	"delayed on: set time": 0
            # }
            ##########################################################################################
            # RFSAI-61B - singel channel multi function relay with button
            ##########################################################################################
            # {
            # 	"id": "41008", "device info": {"type": "ventilation", "product type": "RFSAI-61B", "address": 123456, "label": "abc", "vote": false},
            # 	"actions info": {
            # 		"on": {
            # 			"type": "bool"
            # 		},
            # 		"delayed off": {
            # 			"type": null
            # 		},
            # 		"delayed on": {
            # 			"type": null
            # 		},
            # 		"delayed off: set time": {
            # 			"type": "int",
            # 			"min": 2,
            # 			"max": 3600,
            # 			"step": 1
            # 		},
            # 		"delayed on: set time": {
            # 			"type": "int",
            # 			"min": 2,
            # 			"max": 3600,
            # 			"step": 1
            # 		},
            # 		"automat": {
            # 			"type": "bool"
            # 		}
            # 	},
            # 	"primary actions": ["on"],
            # 	"secondary actions": [["delayed off", "delayed off: set time"], ["delayed on", "delayed on: set time"], "automat"],
            # 	"settings": {
            #             "delayed off: set time": 2,
            #             "delayed on: set time": 2
            # 	}
            # }
            # State:
            # {
            # 	"on": false,
            # 	"delay": false,
            # 	"automat": false,
            # 	"locked": false,
            # 	"delayed off: set time": 2,
            # 	"delayed on: set time": 2
            # }

            ##########################################################################################
            # RFSF-1B - flood detector
            ##########################################################################################
            # {"id":"55275","device info":{"address":239860,"label":"Voda","type":"flood detector","product type":"RFSF-1B"},
            # 	"actions info": {
            # 		"automat": {
            # 			"type": "bool"
            # 		},
            # 		"deactivate": {
            # 			"type": null
            # 		},
            # 		"disarm": {
            # 			"type": "bool"
            # 		} 
            # 	},
            # 	"primary actions": ["deactivate","disarm"],
            # 	"secondary actions": ["automat"],
            # 	"settings": {
            # 	"disarm": false
            # 	}
            # }
            # State:
            # {
            # 	"alarm": false,
            # 	"detect": false,
            # 	"automat": true,
            # 	"battery": true,
            # 	"disarm": false
            # }



            # User should set type to light. But sometimes...
            # That is why we will always treat RFDA-11B as a light dimmer
            #
            if ('light' in d[mac]['info']['device info']['type']) or ('lamp' in d[mac]['info']['device info']['type']) or (d[mac]['info']['device info']['product type'] == 'RFDA-11B'):
                logger.info(d[mac]['info']['device info'])

                if ('on' in d[mac]['info']['primary actions']):
                    logger.info("Primary action of light is ON")
                    discovery = {
                        'schema': 'basic',
                        'name': d[mac]['info']['device info']['label'],
                        'unique_id': ('eLan-' + mac),
                        'device': {
                            'name': d[mac]['info']['device info']['label'],
                            'identifiers' : ('eLan-light-' + mac),
                            'connections': [["mac",  mac]],
                            'mf': 'Elko EP',
                            'mdl': d[mac]['info']['device info']['product type']
                        },
                        'command_topic': d[mac]['control_topic'],
                        'state_topic': d[mac]['status_topic'],
                        'json_attributes_topic': d[mac]['status_topic'],
                        'payload_off': '{"on":false}',
                        'payload_on': '{"on":true}',
                        'state_value_template':
                        '{%- if value_json.on -%}{"on":true}{%- else -%}{"on":false}{%- endif -%}'
                    }
                    mqtt_cli.publish('homeassistant/light/' + mac + '/config',
                                    bytearray(json.dumps(discovery), 'utf-8'))
                    logger.info("Discovery published for " + d[mac]['url'])
                    logger.debug(json.dumps(discovery))

                if ('brightness' in d[mac]['info']['primary actions']) or (d[mac]['info']['device info']['product type'] == 'RFDA-11B'):
                    logger.info("Primary action of light is BRIGHTNESS")
                    discovery = {
                        'schema': 'template',
                        'name': d[mac]['info']['device info']['label'],
                        'unique_id': ('eLan-' + mac),
                        'device': {
                            'name': d[mac]['info']['device info']['label'],
                            'identifiers' : ('eLan-dimmer-' + mac),
                            'connections': [["mac",  mac]],
                            'mf': 'Elko EP',
                            'mdl': d[mac]['info']['device info']['product type']
                        },
                        'state_topic': d[mac]['status_topic'],
                        #'json_attributes_topic': d[mac]['status_topic'],
                        'command_topic': d[mac]['control_topic'],
                        'command_on_template':
                        '{%- if brightness is defined -%} {"brightness": {{ (brightness * '
                        + str(d[mac]['info']['actions info']['brightness']
                              ['max']) +
                        ' / 255 ) | int }} } {%- else -%} {"brightness": 100 } {%- endif -%}',
                        'command_off_template': '{"brightness": 0 }',
                        'state_template':
                        '{%- if value_json.brightness > 0 -%}on{%- else -%}off{%- endif -%}',
                        'brightness_template':
                        '{{ (value_json.brightness * 255 / ' + str(
                            d[mac]['info']['actions info']['brightness']
                            ['max']) + ') | int }}'
                    }
                    mqtt_cli.publish('homeassistant/light/' + mac + '/config',
                                    bytearray(json.dumps(discovery), 'utf-8'))
                    logger.info("Discovery published for " + d[mac]['url'])
                    logger.debug(json.dumps(discovery))

            #
            # Switches
            # RFSA-6xM units and "appliance" class of eLan
            # Note: handled as ELSE of light entities to avoid lights on RFSA-6xM units
            elif ('appliance' in d[mac]['info']['device info']['type']) or (d[mac]['info']['device info']['product type'] == 'RFSA-61M') or (d[mac]['info']['device info']['product type'] == 'RFSA-66M') or (d[mac]['info']['device info']['product type'] == 'RFSA-11B')  or (d[mac]['info']['device info']['product type'] == 'RFUS-61') or (d[mac]['info']['device info']['product type'] == 'RFSA-62B'):
                logger.info(d[mac]['info']['device info'])
                # "on" primary action is required for switches
                if ('on' in d[mac]['info']['primary actions']):
                    logger.info("Primary action of device is ON")
                    discovery = {
                        'schema': 'basic',
                        'name': d[mac]['info']['device info']['label'],
                        'unique_id': ('eLan-' + mac),
                        'device': {
                            'name': d[mac]['info']['device info']['label'],
                            'identifiers': ('eLan-switch-' + mac),
                            'connections': [["mac",  mac]],
                            'mf': 'Elko EP',
                            'mdl': d[mac]['info']['device info']['product type']
                        },
                        'command_topic': d[mac]['control_topic'],
                        'state_topic': d[mac]['status_topic'],
                        'json_attributes_topic': d[mac]['status_topic'],
                        'payload_off': '{"on":false}',
                        'payload_on': '{"on":true}',
                        'state_off': 'off',
                        'state_on' : 'on',
                        'value_template':
                        '{%- if value_json.on -%}on{%- else -%}off{%- endif -%}'
                    }
                    mqtt_cli.publish('homeassistant/switch/' + mac + '/config',
                                    bytearray(json.dumps(discovery), 'utf-8'))
                    logger.info("Discovery published for " + d[mac]['url'])
                    logger.debug(json.dumps(discovery))


            #
            # Thermostats
            #
            # User should set type to heating. But sometimes...
            # That is why we will always treat RFSTI-11G a temperature sensor/thermostat
            #
            if (d[mac]['info']['device info']['type'] == 'heating') or (d[mac]['info']['device info']['product type'] == 'RFSTI-11G'):
                logger.info(d[mac]['info']['device info'])

                discovery = {
                    'name': d[mac]['info']['device info']['label'] + '-IN',
                    'unique_id': ('eLan-' + mac + '-IN'),
                    'device': {
                        'name': d[mac]['info']['device info']['label'],
                        'identifiers' : ('eLan-thermostat-' + mac),
                        'connections': [["mac",  mac]],
                        'mf': 'Elko EP',
                        'mdl': d[mac]['info']['device info']['product type']
                    },
                    'device_class': 'temperature',
                    'state_topic': d[mac]['status_topic'],
                    'json_attributes_topic': d[mac]['status_topic'],
                    'value_template': '{{ value_json["temperature IN"] }}',
                    'unit_of_measurement': '°C'
                }
                mqtt_cli.publish('homeassistant/sensor/' + mac + '/IN/config',
                                bytearray(json.dumps(discovery), 'utf-8'))
                logger.info("Discovery published for " + d[mac]['url'])
                logger.debug(json.dumps(discovery))

                discovery = {
                    'name': d[mac]['info']['device info']['label'] + '-OUT',
                    'unique_id': ('eLan-' + mac + '-OUT'),
                    'device': {
                        'name': d[mac]['info']['device info']['label'],
                        'identifiers' : ('eLan-thermostat-' + mac),
                        'connections': [["mac",  mac]],
                        'mf': 'Elko EP',
                        'mdl': d[mac]['info']['device info']['product type']
                    },
                    'state_topic': d[mac]['status_topic'],
                    'json_attributes_topic': d[mac]['status_topic'],
                    'device_class': 'temperature',
                    'value_template': '{{ value_json["temperature OUT"] }}',
                    'unit_of_measurement': '°C'
                }
                mqtt_cli.publish('homeassistant/sensor/' + mac + '/OUT/config',
                                bytearray(json.dumps(discovery), 'utf-8'))

                logger.info("Discovery published for " + d[mac]['url'])
                logger.debug(json.dumps(discovery))
#
# Note - needs to be converted to CLIMATE class
#
                discovery = {
                    'name': d[mac]['info']['device info']['label'] + '-ON',
                    'unique_id': ('eLan-' + mac + '-ON'),
                    'device': {
                        'name': d[mac]['info']['device info']['label'],
                        'identifiers' : ('eLan-thermostat-' + mac),
                        'connections': [["mac",  mac]],
                        'mf': 'Elko EP',
                        'mdl': d[mac]['info']['device info']['product type']
                    },
                    'state_topic': d[mac]['status_topic'],
                    'json_attributes_topic': d[mac]['status_topic'],
#                    'device_class': 'heat',
                    'value_template':
                    '{%- if value_json.on -%}on{%- else -%}off{%- endif -%}'
#                    'command_topic': d[mac]['control_topic']
                }
                mqtt_cli.publish('homeassistant/sensor/' + mac + '/ON/config',
                                bytearray(json.dumps(discovery), 'utf-8'))

                logger.info("Discovery published for " + d[mac]['url'])
                logger.debug(json.dumps(discovery))
            #
            # Thermometers
            #
            # User should set type to thermometer. But sometimes...
            #

            if (d[mac]['info']['device info']['type'] == 'thermometer') or (d[mac]['info']['device info']['product type'] == 'RFTI-10B'):
                logger.info(d[mac]['info']['device info'])

                discovery = {
                    'name': d[mac]['info']['device info']['label'] + '-IN',
                    'unique_id': ('eLan-' + mac + '-IN'),
                    'device': {
                        'name': d[mac]['info']['device info']['label'],
                        'identifiers': ('eLan-thermometer-' + mac),
                        'connections': [["mac",  mac]],
                        'mf': 'Elko EP',
                        'mdl': d[mac]['info']['device info']['product type']
                    },
                    'device_class': 'temperature',
                    'state_topic': d[mac]['status_topic'],
                    'json_attributes_topic': d[mac]['status_topic'],
                    'value_template': '{{ value_json["temperature IN"] }}',
                    'unit_of_measurement': '°C'
                }
                mqtt_cli.publish('homeassistant/sensor/' + mac + '/IN/config',
                                bytearray(json.dumps(discovery), 'utf-8'))
                logger.info("Discovery published for " + d[mac]['url'])
                logger.debug(json.dumps(discovery))

                discovery = {
                    'name': d[mac]['info']['device info']['label'] + '-OUT',
                    'unique_id': ('eLan-' + mac + '-OUT'),
                    'device': {
                        'name': d[mac]['info']['device info']['label'],
                        'identifiers': ('eLan-thermometer-' + mac),
                        'connections': [["mac",  mac]],
                        'mf': 'Elko EP',
                        'mdl': d[mac]['info']['device info']['product type']
                    },
                    'state_topic': d[mac]['status_topic'],
                    'json_attributes_topic': d[mac]['status_topic'],
                    'device_class': 'temperature',
                    'value_template': '{{ value_json["temperature OUT"] }}',
                    'unit_of_measurement': '°C'
                }
                mqtt_cli.publish('homeassistant/sensor/' + mac + '/OUT/config',
                                bytearray(json.dumps(discovery), 'utf-8'))

                logger.info("Discovery published for " + d[mac]['url'])
                logger.debug(json.dumps(discovery))



            #
            # Detectors
            #
            # RFWD-100 status messages
            # {alarm: true, detect: false, tamper: “closed”, automat: false, battery: true, disarm: false}
            # {alarm: true, detect: true, tamper: “closed”, automat: false, battery: true, disarm: false}
            # RFSF-1B status message
            # {"alarm": false,	"detect": false, "automat": true, "battery": true, "disarm": false }

            if ('detector' in d[mac]['info']['device info']['type']) or ('RFWD-' in d[mac]['info']['device info']['product type']) or ('RFSD-' in d[mac]['info']['device info']['product type']) or ('RFMD-' in d[mac]['info']['device info']['product type']) or ('RFSF-' in d[mac]['info']['device info']['product type']):
                logger.info(d[mac]['info']['device info'])

                icon = ''

                # A wild guess of icon
                if ('window' in d[mac]['info']['device info']['type']) or ('RFWD-' in d[mac]['info']['device info']['product type']):
                    icon = 'mdi:window-open'
                    if ('door' in str(d[mac]['info']['device info']['label']).lower()):
                        icon = 'mdi:door-open'

                if ('smoke' in d[mac]['info']['device info']['type']) or ('RFSD-' in d[mac]['info']['device info']['product type']):
                    icon = 'mdi:smoke-detector'

                if ('motion' in d[mac]['info']['device info']['type']) or ('RFMD-' in d[mac]['info']['device info']['product type']):
                    icon = 'mdi:motion-sensor'

                if ('flood' in d[mac]['info']['device info']['type']) or ('RFSF-' in d[mac]['info']['device info']['product type']):
                    icon = 'mdi:waves'


                # Silently expect that all detectors provide "detect" action
                discovery = {
                    'name': d[mac]['info']['device info']['label'],
                    'unique_id': ('eLan-' + mac),
                    'device': {
                        'name': d[mac]['info']['device info']['label'],
                        'identifiers' : ('eLan-detector-' + mac),
                        'connections': [["mac",  mac]],
                        'mf': 'Elko EP',
                        'mdl': d[mac]['info']['device info']['product type']
                    },
                    'state_topic': d[mac]['status_topic'],
                    'json_attributes_topic': d[mac]['status_topic'],
#                    'device_class': 'heat',
                    'value_template':
                    '{%- if value_json.detect -%}on{%- else -%}off{%- endif -%}'
#                    'command_topic': d[mac]['control_topic']
                }

                if (icon != ''):
                    discovery['icon'] = icon

                mqtt_cli.publish('homeassistant/sensor/' + mac + '/config',
                                bytearray(json.dumps(discovery), 'utf-8'))

                logger.info("Discovery published for " + d[mac]['url'])
                logger.debug(json.dumps(discovery))

                # Silently expect that all detectors provide "battery" status
                # Battery
                discovery = {
                    'name': d[mac]['info']['device info']['label'] + 'battery',
                    'unique_id': ('eLan-' + mac + '-battery'),
                    'device': {
                        'name': d[mac]['info']['device info']['label'],
                        'identifiers' : ('eLan-detector-' + mac),
                        'connections': [["mac",  mac]],
                        'mf': 'Elko EP',
                        'mdl': d[mac]['info']['device info']['product type']
                    },
                    'device_class': 'battery',
                    'state_topic': d[mac]['status_topic'],
                    #'json_attributes_topic': d[mac]['status_topic'],
                    'value_template':
                    '{%- if value_json.battery -%}100{%- else -%}0{%- endif -%}'
#                    'command_topic': d[mac]['control_topic']
                }
                mqtt_cli.publish('homeassistant/sensor/' + mac + '/battery/config',
                                bytearray(json.dumps(discovery), 'utf-8'))

                logger.info("Discovery published for " + d[mac]['url'])
                logger.debug(json.dumps(discovery))


                # START - RFWD window/door detector
                if (d[mac]['info']['device info']['product type'] == 'RFWD-100') or (d[mac]['info']['device info']['product type'] == 'RFSF-1B'):
                    # RFWD-100 status messages
                    # {alarm: true, detect: false, tamper: “closed”, automat: false, battery: true, disarm: false}
                    # {alarm: true, detect: true, tamper: “closed”, automat: false, battery: true, disarm: false}
                    # RFSF-1B
                    # {"alarm": false,	"detect": false, "automat": true, "battery": true, "disarm": false }
                    # Alarm
                    discovery = {
                        'name': d[mac]['info']['device info']['label'] + 'alarm',
                        'unique_id': ('eLan-' + mac + '-alarm'),
                        'icon': 'mdi:alarm-light',
                        'device': {
                            'name': d[mac]['info']['device info']['label'],
                            'identifiers' : ('eLan-detector-' + mac),
                            'connections': [["mac",  mac]],
                            'mf': 'Elko EP',
                            'mdl': d[mac]['info']['device info']['product type']
                        },
                        'state_topic': d[mac]['status_topic'],
                        'json_attributes_topic': d[mac]['status_topic'],
                        'value_template':
                        '{%- if value_json.alarm -%}on{%- else -%}off{%- endif -%}'
    #                    'command_topic': d[mac]['control_topic']
                    }
                    mqtt_cli.publish('homeassistant/sensor/' + mac + '/alarm/config',
                                    bytearray(json.dumps(discovery), 'utf-8'))

                    logger.info("Discovery published for " + d[mac]['url'])
                    logger.debug(json.dumps(discovery))

                if (d[mac]['info']['device info']['product type'] == 'RFWD-100'):
                    # Tamper
                    # RFWD-100 status messages
                    # {alarm: true, detect: false, tamper: “closed”, automat: false, battery: true, disarm: false}
                    # {alarm: true, detect: true, tamper: “closed”, automat: false, battery: true, disarm: false}
                    discovery = {
                        'name': d[mac]['info']['device info']['label'] + 'tamper',
                        'unique_id': ('eLan-' + mac + '-tamper'),
                        'icon': 'mdi:gesture-tap',
                        'device': {
                            'name': d[mac]['info']['device info']['label'],
                            'identifiers' : ('eLan-detector-' + mac),
                            'connections': [["mac",  mac]],
                            'mf': 'Elko EP',
                            'mdl': d[mac]['info']['device info']['product type']
                        },
                        'state_topic': d[mac]['status_topic'],
                        'json_attributes_topic': d[mac]['status_topic'],
                        'value_template':
                        '{%- if value_json.tamper == "opened" -%}on{%- else -%}off{%- endif -%}'
    #                    'command_topic': d[mac]['control_topic']
                    }
                    mqtt_cli.publish('homeassistant/sensor/' + mac + '/tamper/config',
                                    bytearray(json.dumps(discovery), 'utf-8'))

                    logger.info("Discovery published for " + d[mac]['url'])
                    logger.debug(json.dumps(discovery))

                    # Automat
                    discovery = {
                        'name': d[mac]['info']['device info']['label'] + 'automat',
                        'unique_id': ('eLan-' + mac + '-automat'),
                        'icon': 'mdi:arrow-decision-auto',
                        'device': {
                            'name': d[mac]['info']['device info']['label'],
                            'identifiers' : ('eLan-detector-' + mac),
                            'connections': [["mac",  mac]],
                            'mf': 'Elko EP',
                            'mdl': d[mac]['info']['device info']['product type']
                        },
                        'state_topic': d[mac]['status_topic'],
                        'json_attributes_topic': d[mac]['status_topic'],
                        'value_template':
                        '{%- if value_json.automat -%}on{%- else -%}off{%- endif -%}'
    #                    'command_topic': d[mac]['control_topic']
                    }
                    mqtt_cli.publish('homeassistant/sensor/' + mac + '/automat/config',
                                    bytearray(json.dumps(discovery), 'utf-8'))

                    logger.info("Discovery published for " + d[mac]['url'])
                    logger.debug(json.dumps(discovery))

                    # Disarm
                    discovery = {
                        'name': d[mac]['info']['device info']['label'] + 'disarm',
                        'unique_id': ('eLan-' + mac + '-disarm'),
                        'icon': 'mdi:lock-alert',
                        'device': {
                            'name': d[mac]['info']['device info']['label'],
                            'identifiers' : ('eLan-detector-' + mac),
                            'connections': [["mac",  mac]],
                            'mf': 'Elko EP',
                            'mdl': d[mac]['info']['device info']['product type']
                        },
                        'state_topic': d[mac]['status_topic'],
                        'json_attributes_topic': d[mac]['status_topic'],
                        'value_template':
                        '{%- if value_json.disarm -%}on{%- else -%}off{%- endif -%}'
    #                    'command_topic': d[mac]['control_topic']
                    }
                    mqtt_cli.publish('homeassistant/sensor/' + mac + '/disarm/config',
                                    bytearray(json.dumps(discovery), 'utf-8'))

                    logger.info("Discovery published for " + d[mac]['url'])
                    logger.debug(json.dumps(discovery))

                # END - RFWD window/door detector
 
 
 
 */