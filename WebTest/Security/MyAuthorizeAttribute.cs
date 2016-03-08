using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebTest.Infrastructure;

namespace WebTest.Security
{
    public class RedirectController : Controller
    {
        public ActionResult RedirectToTourIndex()
        {
            //return RedirectToAction("Index", "Tour");
            return RedirectToAction("Error", "Gaefa");
        }
    }

    /*
    public class MyAuthorizeAttribute : AuthorizeAttribute
    {
        testEntities3 context = new testEntities3(); 
        private readonly string[] allowedroles;
        public MyAuthorizeAttribute(params string[] roles)
        {
            this.allowedroles = roles;
        }
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            bool authorize = false;
            foreach (var role in allowedroles)
            {
                var userRole = context.user_info.Where(m => m.id == GlobalVar.ID_USER && m.role == role);
                if (userRole.Count() > 0)
                {
                    authorize = true;
                }
            }
            return authorize;
        }
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = (new RedirectController()).RedirectToTourIndex();
        }
    }
    */
}