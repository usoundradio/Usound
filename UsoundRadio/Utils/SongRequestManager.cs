using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections.Concurrent;
using UsoundRadio.Common;

namespace UsoundRadio.Utils
{
    public class SongRequestManager
    {
        private readonly List<SongRequest> mustLock_songRequests = new List<SongRequest>();
        
        public void RequestSong(int songId, Guid clientId)
        {
            var request = new SongRequest
            {
                ClientId = clientId,
                DateTime = DateTime.Now,
                PlayedForClients = new ConcurrentBag<Guid> { clientId },
                SongId = songId
            };
            this.UpdateRequests(l => l.Add(request));
        }

        /// <summary>
        /// Gets the ID of a song request to play for the client.
        /// Returns null if no song request is available.
        /// </summary>
        /// <param name="clientId">The client ID of user requesting the song.</param>
        /// <returns>Returns null if no song request is available.</returns>
        public int? GetSongRequest(Guid clientId)
        {
            WeedOutOldSongRequests();
            
            var songRequest = FindSongRequest(l => l.FirstOrDefault(s => !s.PlayedForClients.Contains(clientId)));
            var isSongDislikedByUser = songRequest != null && Dependency.Get<LikesCache>().ForClient(clientId).Any(l => l.SongId == songRequest.SongId && l.LikeStatus == Models.SongLike.Dislike);
            if (songRequest != null && !isSongDislikedByUser)
            {
                songRequest.PlayedForClients.Add(clientId);
                return songRequest.SongId;
            }
            return null;
        }

        private void WeedOutOldSongRequests()
        {
            var expiration = TimeSpan.FromMinutes(10);
            var now = DateTime.Now;
            UpdateRequests(list => list.RemoveAll(s => now.Subtract(s.DateTime) > expiration));
        }

        private void UpdateRequests(Action<List<SongRequest>> update)
        {
            lock (this.mustLock_songRequests)
            {
                update(this.mustLock_songRequests);
            }
        }

        private SongRequest FindSongRequest(Func<List<SongRequest>, SongRequest> predicate)
        {
            lock (this.mustLock_songRequests)
            {
                return predicate(this.mustLock_songRequests);
            }
        }

        class SongRequest
        {
            public Guid ClientId { get; set; }
            public int SongId { get; set; }
            public DateTime DateTime { get; set; }
            public ConcurrentBag<Guid> PlayedForClients { get; set; }
        }
    }
}