using System.Text.Json.Serialization;
using static System.Collections.Specialized.BitVector32;

namespace elan2mqtt.Model.eLan
{
    public class ElkoEpDevFromJson
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("device info")]
        public DeviceInfoFromJson DeviceInfo { get; set; }

        [JsonPropertyName("actions info")]
        public ActionsInfoFromJson ActionsInfo { get; set; }

        [JsonPropertyName("primary actions")]
        public List<object> PrimaryActions { get; set; }

        [JsonPropertyName("secondary actions")]
        public List<object> SecondaryActions { get; set; }

        [JsonPropertyName("settings")]
        public Dictionary<string, int> Settings { get; set; }

        // Bezparametrický konstruktor
        public ElkoEpDevFromJson() { }

        // Konstruktor, který přijímá instanci třídy ElkoEpDevFromJson a zkopíruje všechny jeho proměnné do nové instance
        public ElkoEpDevFromJson(ElkoEpDevFromJson toCopy)
        {
            Id = toCopy.Id;
            DeviceInfo = new DeviceInfoFromJson
            {
                Address = toCopy.DeviceInfo.Address,
                Label = toCopy.DeviceInfo.Label,
                Type = toCopy.DeviceInfo.Type,
                ProductType = toCopy.DeviceInfo.ProductType
            };
            ActionsInfo = new ActionsInfoFromJson
            {
                On = toCopy.ActionsInfo.On,
                DelayedOff = toCopy.ActionsInfo.DelayedOff,
                DelayedOn = toCopy.ActionsInfo.DelayedOn,
                DelayedOffSetTime = toCopy.ActionsInfo.DelayedOffSetTime,
                DelayedOnSetTime = toCopy.ActionsInfo.DelayedOnSetTime,
                Automat = toCopy.ActionsInfo.Automat,
                Red = toCopy.ActionsInfo.Red,
                Green = toCopy.ActionsInfo.Green,
                Blue = toCopy.ActionsInfo.Blue,
                Brightness = toCopy.ActionsInfo.Brightness,
                Demo = toCopy.ActionsInfo.Demo,
                Increase = toCopy.ActionsInfo.Increase,
                Decrease = toCopy.ActionsInfo.Decrease,
                IncreaseSetTime = toCopy.ActionsInfo.IncreaseSetTime,
                DecreaseSetTime = toCopy.ActionsInfo.DecreaseSetTime,
                RequestedTemperature = toCopy.ActionsInfo.RequestedTemperature,
                OpenWindowSensitivity = toCopy.ActionsInfo.OpenWindowSensitivity,
                OpenWindowOffTime = toCopy.ActionsInfo.OpenWindowOffTime
            };
            PrimaryActions = new List<object>(toCopy.PrimaryActions);
            SecondaryActions = new List<object>(toCopy.SecondaryActions);
            Settings = new Dictionary<string, int>(toCopy.Settings);
        }

    }

    public class DeviceInfoFromJson
    {
        [JsonPropertyName("address")]
        public int Address { get; set; }

        [JsonPropertyName("label")]
        public string Label { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("product type")]
        public string ProductType { get; set; }

        // Bezparametrický konstruktor
        public DeviceInfoFromJson() { }
    }

    public class ActionsInfoFromJson
    {
        [JsonPropertyName("on")]
        public ActionTypeFromJson On { get; set; }

        [JsonPropertyName("delayed off")]
        public ActionTypeFromJson DelayedOff { get; set; }

        [JsonPropertyName("delayed on")]
        public ActionTypeFromJson DelayedOn { get; set; }

        [JsonPropertyName("delayed off: set time")]
        public ActionTypeFromJson DelayedOffSetTime { get; set; }

        [JsonPropertyName("delayed on: set time")]
        public ActionTypeFromJson DelayedOnSetTime { get; set; }

        [JsonPropertyName("automat")]
        public ActionTypeFromJson Automat { get; set; }

        [JsonPropertyName("red")]
        public ActionTypeFromJson Red { get; set; }

        [JsonPropertyName("green")]
        public ActionTypeFromJson Green { get; set; }

        [JsonPropertyName("blue")]
        public ActionTypeFromJson Blue { get; set; }

        [JsonPropertyName("brightness")]
        public ActionTypeFromJson Brightness { get; set; }

        [JsonPropertyName("demo")]
        public ActionTypeFromJson Demo { get; set; }

        [JsonPropertyName("increase")]
        public ActionTypeFromJson Increase { get; set; }

        [JsonPropertyName("decrease")]
        public ActionTypeFromJson Decrease { get; set; }

        [JsonPropertyName("increase: set time")]
        public ActionTypeFromJson IncreaseSetTime { get; set; }

        [JsonPropertyName("decrease: set time")]
        public ActionTypeFromJson DecreaseSetTime { get; set; }

        [JsonPropertyName("requested temperature")]
        public ActionTypeFromJson RequestedTemperature { get; set; }

        [JsonPropertyName("open window sensitivity")]
        public ActionTypeFromJson OpenWindowSensitivity { get; set; }

        [JsonPropertyName("open window off time")]
        public ActionTypeFromJson OpenWindowOffTime { get; set; }

        // Bezparametrický konstruktor
        public ActionsInfoFromJson() { }
    }

    public class ActionTypeFromJson
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("min")]
        public int? Min { get; set; }

        [JsonPropertyName("max")]
        public int? Max { get; set; }

        [JsonPropertyName("step")]
        public double? Step { get; set; }

        // Bezparametrický konstruktor
        public ActionTypeFromJson() { }
    }
}
