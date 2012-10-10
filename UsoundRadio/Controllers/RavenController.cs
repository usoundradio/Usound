using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Raven.Client;
using UsoundRadio.Common;
using UsoundRadio.Models;

namespace UsoundRadio.Controllers
{
    public class RavenController : Controller
    {
        public void Log(string message)
        {
            var log = new Log { Message = message, TimeStamp = DateTime.Now };
            var expiration = DateTime.UtcNow.AddDays(30);
            this.RavenSession.Store(log);
            this.RavenSession.Advanced.GetMetadataFor(log)["Raven-Expiration-Date"] = new Raven.Json.Linq.RavenJValue(expiration);
        }

        public IDocumentSession RavenSession { get; private set; }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            this.RavenSession = Get.A<IDocumentStore>().OpenSession();
            
            base.OnActionExecuting(filterContext);
        }

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext.Exception == null && !filterContext.IsChildAction)
            {
                this.RavenSession.SaveChanges();
            }

            base.OnActionExecuted(filterContext);
        }
    }
}