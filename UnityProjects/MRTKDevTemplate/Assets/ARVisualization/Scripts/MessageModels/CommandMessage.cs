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

    [EnumMember(Value = "openInIDE")]
    OPEN_IN_IDE,
}

[Serializable]
public class CommandMessage
{
    [JsonProperty("command")]
    public CommandType Command { get; set; }

    [JsonProperty("reason")]
    public string Reason { get; set; }

    [JsonProperty("path")]
    public string Path { get; set; }

    [JsonProperty("line")]
    public int Line { get; set; }
}
