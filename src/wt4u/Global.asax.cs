using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace wt4u
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        void Application_Error(object sender, EventArgs e)
        {
            Exception exc = Server.GetLastError();

            if (exc.GetType() == typeof(HttpException))
            {
                HttpException httpException = exc as HttpException;
                this.Response.RedirectToRoute(new { controller = "Home", action = "Error", error = "HTTP Error " + httpException.GetHttpCode().ToString() });
            }
            else if (exc.GetType() == typeof(SqlException) && exc.Message.Contains("Timeout expired"))
            {
                this.Response.RedirectToRoute(new { controller = "Home", action = "Error", error = "Server Timeout" });
            }
            else if (exc.GetType() == typeof(Exception))
            {
                this.Response.RedirectToRoute(new { controller = "Home", action = "Error", error = "Server Error" });
            }
            Server.ClearError();
        }
    }
}
