using Newtonsoft.Json;
using System;
using System.Collections.Generic;

[Serializable]
public class OpenTabs
{
    [JsonProperty("count")]
    public int Count { get; set; }

    [JsonProperty("tabs")]
    public List<string> Tabs { get; set; }
}
