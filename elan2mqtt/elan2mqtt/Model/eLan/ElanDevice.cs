using MQTTnet.Extensions;

namespace elan2mqtt.Model.eLan
{

    public class ElanDevices : List<ELanDevice>
    {

    }
    public class ELanDevice : ElkoEpDevFromJson
    {
        public ELanDevice(string id, string deviceURL)
        {
            Id = id;
            DeviceURL = deviceURL;

        }

        public void CopyFromElkoEpDevFromJson(ElkoEpDevFromJson copy)
        {
            Id = copy.Id;
            DeviceInfo = new DeviceInfoFromJson
            {
                Address = copy.DeviceInfo.Address,
                Label = copy.DeviceInfo.Label,
                Type = copy.DeviceInfo.Type,
                ProductType = copy.DeviceInfo.ProductType
            };
            ActionsInfo = new ActionsInfoFromJson
            {
                On = copy.ActionsInfo.On,
                DelayedOff = copy.ActionsInfo.DelayedOff,
                DelayedOn = copy.ActionsInfo.DelayedOn,
                DelayedOffSetTime = copy.ActionsInfo.DelayedOffSetTime,
                DelayedOnSetTime = copy.ActionsInfo.DelayedOnSetTime,
                Automat = copy.ActionsInfo.Automat,
                Red = copy.ActionsInfo.Red,
                Green = copy.ActionsInfo.Green,
                Blue = copy.ActionsInfo.Blue,
                Brightness = copy.ActionsInfo.Brightness,
                Demo = copy.ActionsInfo.Demo,
                Increase = copy.ActionsInfo.Increase,
                Decrease = copy.ActionsInfo.Decrease,
                IncreaseSetTime = copy.ActionsInfo.IncreaseSetTime,
                DecreaseSetTime = copy.ActionsInfo.DecreaseSetTime,
                RequestedTemperature = copy.ActionsInfo.RequestedTemperature,
                OpenWindowSensitivity = copy.ActionsInfo.OpenWindowSensitivity,
                OpenWindowOffTime = copy.ActionsInfo.OpenWindowOffTime
            };
            PrimaryActions = new List<string>(copy.PrimaryActions);
            SecondaryActions = new List<object>(copy.SecondaryActions);
            Settings = new Dictionary<string, int>(copy.Settings);
        }

        /// <summary>
        /// Device URL in eLan
        /// </summary>
        public string DeviceURL { get; set; }
        /// <summary>
        /// URL to get device state
        /// </summary>
        public string StateURL { get { return $"{DeviceURL}/state"; } }

        public ElanDeviceTypeEnum DeviceType { get; private set; } = ElanDeviceTypeEnum.Unknown;
        public ElanDeviceSubTypeEnum DeviceSubType { get; private set; } = ElanDeviceSubTypeEnum.none;

        public async Task UpdateDevice(string json)
        {
            var o = ElDevExtensions.FromJson<ElkoEpDevFromJson>(json);
            if (o == null)
            {
                Console.WriteLine($"eLan Device {Id} can not be updated. Given json probably is not eLan device: {json.Linearize()}");
                return;
            }
            this.CopyFromElkoEpDevFromJson(o);
            Console.WriteLine($"eLan Updating Device: {Id} => {DeviceURL}, dev label {DeviceInfo?.Label}, dev address {DeviceInfo?.Address}, type {DeviceInfo?.Type}");

            setDeviceType(this);
        }

        void setDeviceType(ELanDevice dev)
        {
            // User should set type to light. But sometimes...
            // That is why we will always treat RFDA-11B as a light dimmer
            if (dev.DeviceInfo.Type.Contains("light") || dev.DeviceInfo.Type.Contains("lamp") || dev.DeviceInfo.ProductType == "RFDA-11B")
            {
                if (dev.DeviceInfo.Type.Contains("brightness") || dev.DeviceInfo.Type.Contains("dimmed") || dev.DeviceInfo.ProductType == "RFDA-11B")
                {
                    DeviceType = ElanDeviceTypeEnum.Light;
                    DeviceSubType = ElanDeviceSubTypeEnum.Dimmer;
                }
                else
                {
                    DeviceType = ElanDeviceTypeEnum.Light;
                    DeviceSubType = ElanDeviceSubTypeEnum.OnOff;
                }
            }
            //            elif ('appliance' in d[mac]['info']['device info']['type']) or (d[mac]['info']['device info']['product type'] == 'RFSA-61M') or (d[mac]['info']['device info']['product type'] == 'RFSA-66M') or (d[mac]['info']['device info']['product type'] == 'RFSA-11B')  or (d[mac]['info']['device info']['product type'] == 'RFUS-61') or (d[mac]['info']['device info']['product type'] == 'RFSA-62B'):
            else if (dev.DeviceInfo.Type.Contains("appliance") || dev.DeviceInfo.ProductType == "RFSA-61M" || dev.DeviceInfo.ProductType == "RFSA-66M" || dev.DeviceInfo.ProductType == "RFSA-11B" || dev.DeviceInfo.ProductType == "RFUS-61" || dev.DeviceInfo.ProductType == "RFSA-62B")
            {
                DeviceType = ElanDeviceTypeEnum.Switch;
                DeviceSubType = ElanDeviceSubTypeEnum.OnOff;
            }
            else if (dev.DeviceInfo.Type.Contains("heating") || dev.DeviceInfo.ProductType == "RFSA-61B")
            {
                DeviceType = ElanDeviceTypeEnum.Heating;
            }
            else if (dev.DeviceInfo.Type.Contains("thermometer") || dev.DeviceInfo.ProductType == "RFSA-61T")
            {
                DeviceType = ElanDeviceTypeEnum.Thermometer;
            }
            else if (dev.DeviceInfo.Type.Contains("detector") || dev.DeviceInfo.ProductType == "RFSA-61D")
            {
                DeviceType = ElanDeviceTypeEnum.Detector;
            }
            else if (dev.PrimaryActions.Contains("on"))
            {
                DeviceType = ElanDeviceTypeEnum.Switch;
            }
            else
                DeviceType = ElanDeviceTypeEnum.Unknown;
        }

        public override string ToString()
        {
            return $"Device {Id}: {DeviceInfo?.Label} ({DeviceInfo?.Type})";
        }
    }

    public enum ElanDeviceTypeEnum
    {
        Unknown = 0,
        Light = 10,
        Switch = 20,
        Heating = 30,
        Thermometer = 35,
        Detector = 40,
    }
    public enum ElanDeviceSubTypeEnum
    {
        none = 0,
        OnOff = 10,
        Dimmer = 13,
        Color = 15,
    }
}
