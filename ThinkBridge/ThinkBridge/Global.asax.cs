using System.Data.Entity;
using System.Web.Mvc;
using System.Web.Routing;
using ThinkBridge.Models;
using static ThinkBridge.Models.ApplicationDbContext;

namespace ThinkBridge
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            // Register the database initializer — this triggers Seed() on first run
            Database.SetInitializer(new ApplicationDbInitializer());
        }
    }
}