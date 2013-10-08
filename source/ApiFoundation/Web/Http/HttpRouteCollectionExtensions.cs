using System.Web.Http;
using System.Web.Http.Routing;
using ApiFoundation.Security.Cryptography;

namespace ApiFoundation.Web.Http
{
    public static class HttpRouteCollectionExtensions
    {
        //public static IHttpRoute MapTimestampRoute(this HttpRouteCollection routes, string name, string routeTemplate, object defaults, object constraints, ITimestampProvider timestampProvider)
        //{
        //    if (timestampProvider == null)
        //    {
        //        timestampProvider = new DefaultTimestampProvider();
        //    }

        //    var handler = new TimestampHandler(timestampProvider);

        //    return routes.MapHttpRoute(name, routeTemplate, defaults, constraints, handler);
        //}

        //public static IHttpRoute MapTimestampRoute(this HttpRouteCollection routes, string name, string routeTemplate, object defaults, ITimestampProvider timestampProvider)
        //{
        //    return routes.MapTimestampRoute(name, routeTemplate, defaults, null, timestampProvider);
        //}

        //public static IHttpRoute MapTimestampRoute(this HttpRouteCollection routes, string name, string routeTemplate, ITimestampProvider timestampProvider)
        //{
        //    return routes.MapTimestampRoute(name, routeTemplate, null, timestampProvider);
        //}

        //public static IHttpRoute MapTimestampRoute(this HttpRouteCollection routes, string name, string routeTemplate)
        //{
        //    return routes.MapTimestampRoute(name, routeTemplate, null);
        //}
    }
}