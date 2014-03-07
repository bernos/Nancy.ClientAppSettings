using System;
using System.Collections.Generic;
using System.Configuration;
using Nancy.Bootstrapper;
using Newtonsoft.Json;

namespace Nancy.ClientAppSettings
{
    public class ClientAppSettings
    {
        public const string ContextKey = "ClientAppSettings";

        public static ClientAppSettings Enable(IPipelines pipelines)
        {
            var settings = new ClientAppSettings();

            pipelines.BeforeRequest.AddItemToEndOfPipeline(ctx =>
            {
                ctx.Items[ContextKey] = settings;
                return null;
            });

            return settings;
        }

        private readonly IDictionary<string, object> _settings;
        private readonly IList<Func<NancyContext, IDictionary<string, object>>> _appenders;
        
        public string VariableName { get; set; }

        public ClientAppSettings()
        {
            _settings = new Dictionary<string, object>();
            _appenders = new List<Func<NancyContext, IDictionary<string, object>>>();
            VariableName = "Settings";

            Append(ctx =>
            {
                var basePath = "/";

                if (ctx.Request.Url.BasePath != null)
                {
                    basePath = ctx.Request.Url.BasePath + "/";
                }

                return new Dictionary<string, object>
                {
                    {"BasePath", basePath}
                };
            });
        }

        public ClientAppSettings Set(string key, object value)
        {
            _settings[key] = value;
            return this;
        }

        public ClientAppSettings Append(IDictionary<string, object> settings)
        {
            foreach (var pair in settings)
            {
                _settings[pair.Key] = pair.Value;
            }

            return this;
        }

        public ClientAppSettings Append(Func<NancyContext, IDictionary<string, object>> appenderFunc)
        {
            _appenders.Add(appenderFunc);
            return this;
        }

        public ClientAppSettings WithVariableName(string variableName)
        {
            VariableName = variableName;
            return this;
        }

        public ClientAppSettings WithAppSettings(params string[] keys)
        {
            foreach (var key in keys)
            {
                _settings[key] = ConfigurationManager.AppSettings[key];
            }

            return this;
        }

        public string ToJson(NancyContext context)
        {
            foreach (var appender in _appenders)
            {
                Append(appender(context));
            }

            return JsonConvert.SerializeObject(_settings);
        }
    }
}
