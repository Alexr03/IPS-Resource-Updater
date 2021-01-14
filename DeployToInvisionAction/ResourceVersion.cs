using System;
using Newtonsoft.Json;

namespace DeployToInvisionAction
{
    public class ResourceVersion
    {
        [JsonProperty("id")] public int Id { get; set; }

        [JsonProperty("version")] public string Version { get; set; }

        [JsonProperty("changelog")] public string Changelog { get; set; }

        [JsonProperty("hidden")] public bool Hidden { get; set; }

        [JsonProperty("date")] public DateTime Date { get; set; }
    }
}