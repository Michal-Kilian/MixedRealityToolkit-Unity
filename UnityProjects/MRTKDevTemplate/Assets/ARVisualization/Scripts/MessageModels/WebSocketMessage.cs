using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Runtime.Serialization;

[JsonConverter(typeof(StringEnumConverter))]
public enum MessageType
{
    [EnumMember(Value = "projectStructure")]
    PROJECT_STRUCTURE,

    [EnumMember(Value = "executionSample")]
    EXECUTION_SAMPLE,

    [EnumMember(Value = "projectSnapshot")]
    PROJECT_SNAPSHOT,

    [EnumMember(Value = "openTabs")]
    OPEN_TABS,

    [EnumMember(Value = "projectOutdated")]
    PROJECT_OUTDATED,

    [EnumMember(Value = "command")]
    COMMAND,

    [EnumMember(Value = "requestProjectStructure")]
    REQUEST_PROJECT_STRUCTURE,
}

[Serializable]
public class WebSocketMessage
{
    [JsonProperty("type")]
    public MessageType Type { get; set; }

    [JsonProperty("source")]
    public string Source { get; set; }

    [JsonProperty("timestamp")]
    public long TimeStamp { get; set; }

    [JsonProperty("data")]
    public JObject Data { get; set; }
}

[Serializable]
public class BaseMessage
{
    [JsonProperty("message")]
    public string Message { get; set; }
}
