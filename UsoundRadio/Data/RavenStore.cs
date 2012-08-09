using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UsoundRadio.Common;
using UsoundRadio.Models;
using Raven.Abstractions.Data;
using Raven.Client;
using Raven.Client.Document;

namespace UsoundRadio.Data
{
    public class RavenStore
    {
        public static IDocumentStore Instance
        {
            get
            {
                return Dependency.Get<IDocumentStore>();
            }
        }

        public static IDocumentStore CreateDocumentStore()
        {
            var connectionStringName =
#if DEBUG
 "InMemoryRavenDB";
#else
 "RavenDB";
#endif
            var parser = ConnectionStringParser<RavenConnectionStringOptions>
                .FromConnectionStringName(connectionStringName);
            parser.Parse();

            // If we're configured to run in memory, we're debug; running locally.
            // Otherwise, we're live, connect to the real database on AppHarbor.
            var store = default(IDocumentStore);
            var isLocal = parser.ConnectionStringOptions.Url.Contains("localhost");
            if (isLocal)
            {
                store = new Raven.Client.Embedded.EmbeddableDocumentStore
                {
                    RunInMemory = true
                };
            }
            else
            {
                store = new DocumentStore
                {
                    ApiKey = parser.ConnectionStringOptions.ApiKey,
                    Url = parser.ConnectionStringOptions.Url,
                };
            }

            store.Conventions.IdentityPartsSeparator = "-";
            store.Initialize();
            return store;
        }

        public static void Log(string message, IDocumentSession session = null)
        {
            var isNewSession = session == null;
            var log = new Log { Message = message, TimeStamp = DateTime.Now };
            using (var raven = session != null ? session : Instance.OpenSession())
            {
                raven.Store(log);
                if (isNewSession)
                {
                    raven.SaveChanges();
                }
            }
        }
    }
}