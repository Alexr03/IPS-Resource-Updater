using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using DotNetEnv;
using Markdig;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using Serilog;

namespace DeployToInvisionAction
{
    public class Deploy
    {
        static void Main(string[] args)
        {
            Env.TraversePath().Load();
            CreateLogger();
            Log.Information(Environment.ExpandEnvironmentVariables("Started %resource_id%"));
            if (!UploadNewVersion())
            {
                Environment.Exit(1);
            }
        }

        private static void CreateLogger()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .MinimumLevel.Debug()
                .CreateLogger();
        }

        private static bool UploadNewVersion()
        {
            var client = new RestClient(Env.GetString("IPS_URL"));
            var request =
                new RestRequest($"api/downloads/files/{Env.GetInt("RESOURCE_ID")}/history") {Method = Method.POST};
            request.AddOrUpdateParameter("key", Env.GetString("API_KEY"), ParameterType.QueryString);
            request.AddOrUpdateParameter($"linked_files[{Env.GetString("FILE_NAME")}]", Env.GetString("FILE_URL"));
            var version = Env.GetString("VERSION");
            request.AddOrUpdateParameter("version", version);
            if (!string.IsNullOrEmpty(Env.GetString("CHANGELOG_FILE")))
            {
                var changelog = File.ReadAllText(Env.GetString("CHANGELOG_FILE"));
                changelog = Markdown.ToHtml(changelog);
                request.AddOrUpdateParameter("changelog", changelog);
            }
            else
            {
                request.AddOrUpdateParameter("changelog", Env.GetString("CHANGELOG"));
            }
            request.AddOrUpdateParameter("save", true);
            if (GetResourceVersions(client)[0].Version == version)
            {
                request.AddOrUpdateParameter("save", false);
            }

            if (File.Exists("Module.json"))
            {
                var obj = JsonConvert.DeserializeObject<JObject>(File.ReadAllText("Module.json"));
                request.AddOrUpdateParameter("version", obj["Dll"]?.ToString() ?? version);
            }

            var restResponse = client.Execute(request);
            if (restResponse.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }
            Log.Error(restResponse.Content);
            return false;
        }

        private static List<ResourceVersion> GetResourceVersions(IRestClient client)
        {
            var request =
                new RestRequest($"api/downloads/files/{Env.GetInt("RESOURCE_ID")}/history") {Method = Method.GET};
            request.AddParameter("key", Env.GetString("API_KEY"), ParameterType.QueryString);

            var restResponse = client.Execute(request);
            return JsonConvert.DeserializeObject<List<ResourceVersion>>(restResponse.Content);
        }
    }
}