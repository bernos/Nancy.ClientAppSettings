using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nancy.ClientAppSettings
{
    public static class ContextExtensions
    {
        public static ClientAppSettings GetClientAppSettings(this NancyContext context)
        {
            var settings = context.Items[ClientAppSettings.ContextKey] as ClientAppSettings;

            if (settings != null) return settings;
            
            settings = new ClientAppSettings();
            context.Items[ClientAppSettings.ContextKey] = settings;

            return settings;
        }
    }
}
