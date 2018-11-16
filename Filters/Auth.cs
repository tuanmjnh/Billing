using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Billing.Filters
{
    public class Auth : ActionFilterAttribute
    {
        public string Role { get; set; }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var result = new RedirectToRouteResult(
                new System.Web.Routing.RouteValueDictionary {
                    { "area", "" },
                    { "controller", "Auth" },
                    { "action", "Index" },
                    { "continue", TM.Url.ContinueUrl() } });
            if (Authentication.Auth.isAuth)
            {
                //var roles = Role.Split(',');
                if (Role != null && !Role.Split(',').Contains(Authentication.Auth.AuthUser.roles))
                {
                    filterContext.Controller.TempData.Add("MsgDanger", "Bạn không có quyền truy cập. Vui lòng liên hệ với admin!");
                    filterContext.Result = result;
                }
            }
            else
            {
                filterContext.Result = result;
            }
            base.OnActionExecuting(filterContext);
        }
    }
}