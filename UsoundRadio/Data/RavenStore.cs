using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Raven.Abstractions.Data;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;

namespace UsoundRadio.Data
{
    public class RavenStore
    {
        public IDocumentStore CreateDocumentStore()
        {
            var parser = ConnectionStringParser<RavenConnectionStringOptions>
                .FromConnectionStringName("RavenDB");
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

            store.Initialize();
            IndexCreation.CreateIndexes(typeof(RavenStore).Assembly, store);
            return store;
        }
    }
}