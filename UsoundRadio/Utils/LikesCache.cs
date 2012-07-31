using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using UsoundRadio.Models;
using UsoundRadio.Data;

namespace UsoundRadio.Utils
{
    /// <summary>
    /// Maintains a cache of the user's likes.
    /// This helps performance as we use this information each time we fetch a 
    /// song for the user.
    /// 
    /// Thread-safe.
    /// </summary>
    public class LikesCache
    {
        private readonly ConcurrentDictionary<Guid, Like[]> cachedLikes = new ConcurrentDictionary<Guid, Like[]>();

        /// <summary>
        /// Notifies the cache that the user's likes/dislikes have changed.
        /// </summary>
        /// <param name="clientId">The client ID of the user whose likes/dislikes have changed.</param>
        public void OnLikesChanged(Guid clientId)
        {            
            Like[] ignored;
            cachedLikes.TryRemove(clientId, out ignored);

            // Spawn a thread to re-cache this user's likes.
            Task.Factory.StartNew(() => ForClient(clientId));
        }

        /// <summary>
        /// Gets the user's likes from the cache.
        /// If his likes aren't in the cache, they'll be fetched and added to
        /// the cache.
        /// </summary>
        /// <param name="clientId">The client-generated ID of the user.</param>
        /// <returns>The user's likes.</returns>
        public Like[] ForClient(Guid clientId)
        {
            return cachedLikes.GetOrAdd(clientId, GetLikesFromDatabase);
        }

        private Like[] GetLikesFromDatabase(Guid clientId)
        {
            using (var session = RavenStore.Instance.OpenSession())
            {
                var user = session.Query<User>().FirstOrDefault(u => u.ClientIdentifier == clientId);
                if (user == null)
                {
                    return new Like[0];
                }
                else
                {
                    return session
                        .Query<Like>()
                        .Where(l => l.UserId == user.Id)
                        .ToArray();
                }
            }
        }
    }
}