using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using UsoundRadio.Common;

namespace UsoundRadio
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            BundleConfig.RegisterBundles(BundleTable.Bundles);

            AuthConfig.RegisterAuth();
            BundleTable.EnableOptimizations = true;

            // On boot up, start looking for new songs.
            var finder = new NewSongFinder();
            var findingTask = finder.FindSongsAsync();

#if DEBUG
            // If we're running in debug mode (locally) with a fresh in-memory database, 
            // we'll wait to discover all the new songs.
            findingTask.Wait();
#endif
        }
    }
}