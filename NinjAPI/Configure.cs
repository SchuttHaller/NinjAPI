using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NinjAPI.Results;
using NinjAPI.Routing;
using System.Net.Http.Headers;
using System.Web.Http;

namespace NinjAPI
{
    public static class Configure
    {
        public static void NinjAPIConfig(this HttpConfiguration config,
            NullValueHandling NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            bool SetCamelCase = false,
            string apiPrefix = "api")
        {
            // Web API configuration and services
            config.MessageHandlers.Add(new ResultHandler());

            // Rutas de API web
            config.MapHttpAttributeRoutes(new RouteProvider());


            // routing
            config.NinjaRoutes(apiPrefix);

            
            // remove xml support
            config.Formatters.Remove(config.Formatters.XmlFormatter);
            //JSON SerializerSettings
            config.Formatters.JsonFormatter.SerializerSettings.NullValueHandling = NullValueHandling;
            config.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling;

            if (SetCamelCase)
            {
                config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            }

            // allow from web browser
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
        }
    }
}
