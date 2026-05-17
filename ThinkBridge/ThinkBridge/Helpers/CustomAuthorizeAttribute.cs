using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace ThinkBridge.Helpers
{
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly string[] allowedRoles;

        public CustomAuthorizeAttribute(params string[] roles)
        {
            this.allowedRoles = roles;
        }

        protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
        {
            var userRole = Convert.ToString(httpContext.Session["Role"]);
            var userId = Convert.ToString(httpContext.Session["UserId"]);

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userRole))
                return false;

            foreach (var role in allowedRoles)
            {
                if (role.Equals(userRole, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectToRouteResult(
               new RouteValueDictionary
               {
                    { "controller", "Account" },
                    { "action", "Login" }
               });
        }
    }
}