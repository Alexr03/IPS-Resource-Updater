using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using DotNetEnv;
using Newtonsoft.Json;
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

        static bool UploadNewVersion()
        {
            var client = new RestClient(Env.GetString("ips_url"));
            var request =
                new RestRequest($"api/downloads/files/{Env.GetInt("resource_id")}/history") {Method = Method.POST};
            request.AddParameter("key", Env.GetString("api_key"), ParameterType.QueryString);
            request.AddParameter($"linked1_files[{Env.GetString("file_name")}]", Env.GetString("file_url"));
            request.AddParameter("version", Env.GetString("version"));
            request.AddParameter("changelog", Env.GetString("changelog"));
            request.AddParameter("save", 0);

            Log.Information(client.BuildUri(request).ToString());
            var restResponse = client.Execute(request);
            if (restResponse.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }
            Log.Error(restResponse.Content);
            return false;
        }
    }
}