using System;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.CompilerServices;
using Nancy.Bootstrapper;
using Newtonsoft.Json;

namespace Nancy.ClientAppSettings
{
    public class ClientAppSettings
    {
        public const string ContextKey = "ClientAppSettings";

        public static ClientAppSettings Enable(IPipelines pipelines)
        {
            var appSettings = new ClientAppSettings();

            pipelines.BeforeRequest.AddItemToEndOfPipeline(ctx =>
            {
                ctx.Items[ContextKey] = new ClientAppSettings(appSettings);
                return null;
            });

            return appSettings;
        }

        private readonly IDictionary<string, object> _settings;
        private readonly IList<Func<NancyContext, IDictionary<string, object>>> _appenders;
        private readonly ClientAppSettings _parent;
        public string VariableName { get; set; }

        public ClientAppSettings(ClientAppSettings parent) : this()
        {
            _parent = parent;
        }

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

        public IDictionary<string, object> ToDictionary(NancyContext context)
        {
            var settings = (_parent == null) ? new Dictionary<string, object>() : new Dictionary<string, object>(_parent.ToDictionary(context));    
         
            foreach (var appender in _appenders)
            {
                Append(appender(context));
            }

            foreach (var kvp in _settings)
            {
                settings[kvp.Key] = kvp.Value;
            }

            return settings;
        }  

        public string ToJson(NancyContext context)
        {
            return JsonConvert.SerializeObject(ToDictionary(context));
        }
    }
}
