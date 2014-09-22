using System.Web.Http;

/// <summary>
/// Summary description for WebApiConfig
/// </summary>
public class WebApiConfig
{
    public WebApiConfig()
    {
    }

    public static void Register(HttpConfiguration config)
    {
        config.Routes.MapHttpRoute(
            name: "DefaultApi",
            routeTemplate: "api/{controller}/{id}",
            defaults: new { id = RouteParameter.Optional }
        );

        config.Routes.MapHttpRoute("DefaultApiWithActionAndId", "api/{controller}/{action}/{id}");

        //config.Routes.MapHttpRoute("DefaultApiWithId", "Api/{controller}/{id}", new { id = RouteParameter.Optional }, new { id = @"\d+" });
        //config.Routes.MapHttpRoute("DefaultApiWithAction", "Api/{controller}/{action}");
    }
}