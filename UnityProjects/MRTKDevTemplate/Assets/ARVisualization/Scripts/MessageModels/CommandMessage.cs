using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Runtime.Serialization;

[JsonConverter(typeof(StringEnumConverter))]
public enum CommandType
{
    [EnumMember(Value = "pause")]
    PAUSE,

    [EnumMember(Value = "resume")]
    RESUME,
}

[Serializable]
public class CommandMessage
{
    [JsonProperty("command")]
    public CommandType Command { get; set; }

    [JsonProperty("reason")]
    public string Reason { get; set; }
}
