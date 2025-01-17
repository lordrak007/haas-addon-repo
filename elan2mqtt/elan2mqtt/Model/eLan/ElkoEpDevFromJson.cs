using System.Text.Json.Serialization;

namespace elan2mqtt.Model.eLan
{



	/// <summary>
	/// Classes generated from devices jsons from Elan-003 devices
	/// </summary>

    public class ElkoEpDevFromJson
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("device info")]
        public DeviceInfoFromJson DeviceInfo { get; set; }

        [JsonPropertyName("actions info")]
        public ActionsInfoFromJson ActionsInfo { get; set; }

        [JsonPropertyName("primary actions")]
        public List<string> PrimaryActions { get; set; }

        [JsonPropertyName("secondary actions")]
        public List<object> SecondaryActions { get; set; }

        [JsonPropertyName("settings")]
        public Dictionary<string, int> Settings { get; set; }

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
    }

 
}
