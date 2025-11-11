using Newtonsoft.Json;
using System;
using System.Collections.Generic;

[Serializable]
public class ExecutionSample
{
    [JsonProperty("timestamp")]
    public long TimeStamp { get; set; }

    [JsonProperty("threadName")]
    public string ThreadName { get; set; }

    [JsonProperty("frames")]
    public List<Frame> Frames { get; set; }

    [Serializable]
    public class Frame
    {
        [JsonProperty("className")]
        public string ClassName { get; set; }

        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("line")]
        public int Line { get; set; }
    }
}
