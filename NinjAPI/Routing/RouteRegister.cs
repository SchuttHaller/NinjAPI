using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Routing;

namespace NinjAPI.Routing
{
    public static class RouteRegister
    {
        public static void NinjaRoutes(this HttpConfiguration config, string apiPrefix)
        {
            string _prefix = $"{apiPrefix}{(!string.IsNullOrEmpty(apiPrefix) ? "/" : "")}";
            //default route
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: $"{_prefix}{{controller}}",
                defaults: new { action = "Get", httpMethod = new HttpMethodConstraint(HttpMethod.Get) }
            );
        }
    }
}
