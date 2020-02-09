using System.Web.Http;

namespace NinjAPI.UseExample
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.NinjAPIConfig();
        }
    }
}
