using Newtonsoft.Json;
using System;
using System.Collections.Generic;

[Serializable]
public class ProjectSnapshot
{
    [JsonProperty("projectName")]
    public string ProjectName { get; set; }

    [JsonProperty("modules")]
    public List<ModuleInfo> Modules { get; set; }

    [JsonProperty("sdkName")]
    public string SDKName { get; set; }

}

[Serializable]
public class ModuleInfo
{
    [JsonProperty("moduleName")]
    public string ModuleName { get; set; }

    [JsonProperty("sdkName")]
    public string SDKName { get; set; }
}
