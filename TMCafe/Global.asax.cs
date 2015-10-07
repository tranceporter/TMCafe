using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace TMCafe
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

            SetupRefreshJob();
        }

        private static void SetupRefreshJob()
        {
            //remove a previous job
            Action remove = HttpContext.Current.Cache["TMCafeRefresh"] as Action;
            if (remove is Action)
            {
                HttpContext.Current.Cache.Remove("TMCafeRefresh");
                remove.EndInvoke(null);
            }

            //get the worker
            Action work = () =>
            {
                while (true)
                {
                    Thread.Sleep(60000);
                    WebClient refresh = new WebClient();
                    try
                    {
                        refresh.UploadString("http://tmcafe.apphb.com/", string.Empty);
                    }
                    catch (Exception)
                    {
                        //snip...
                    }
                    finally
                    {
                        refresh.Dispose();
                    }
                }
            };
            work.BeginInvoke(null, null);

            //add this job to the cache
            HttpContext.Current.Cache.Add(
                "TMCafeRefresh",
                work,
                null,
                Cache.NoAbsoluteExpiration,
                Cache.NoSlidingExpiration,
                CacheItemPriority.Normal,
                (s, o, r) => { SetupRefreshJob(); }
                );
        }
    }
}