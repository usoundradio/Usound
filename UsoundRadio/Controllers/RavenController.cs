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
            this.RavenDb.Store(log);
            this.RavenDb.Advanced.GetMetadataFor(log)["Raven-Expiration-Date"] = new Raven.Json.Linq.RavenJValue(expiration);
        }

        public IDocumentSession RavenDb { get; private set; }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            this.RavenDb = Get.A<IDocumentStore>().OpenSession();
            
            base.OnActionExecuting(filterContext);
        }

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext.Exception == null && !filterContext.IsChildAction)
            {
                this.RavenDb.SaveChanges();
            }

            base.OnActionExecuted(filterContext);
        }
    }
}