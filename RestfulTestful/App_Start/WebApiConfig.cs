using RestfulTestful.SQLiteModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;

namespace RestfulTestful
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Konfiguracja i usługi składnika Web API

            // Trasy składnika Web API
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            //config.Routes.MapHttpRoute(
            //    name: "OdataApi",
            //    routeTemplate: "odata/{controller}/{id}",
            //    defaults: new { id = RouteParameter.Optional }
            //);
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Product>("Products");
            config.Routes.MapODataServiceRoute("odata", "odata", builder.GetEdmModel());
            config.Routes.MapHttpRoute(
                 name: "DefaultApiCat",
                 routeTemplate: "api/{controller}/{id}/{category}",
                defaults: new { category = RouteParameter.Optional, id = RouteParameter.Optional }
            );
        }
    }
}
