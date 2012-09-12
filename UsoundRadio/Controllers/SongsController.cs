using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PrairieAsunder.Models;
using UsoundRadio.Data;
using UsoundRadio.Models;
using UsoundRadio.Utils;
using UsoundRadio.Common;
using System.Threading.Tasks;
using System.Reactive;
using System.Reactive.Linq;
using System.ComponentModel;

namespace UsoundRadio.Controllers
{
    public class SongsController : Controller
    {
        private static readonly Random Random = new Random();
        private static readonly List<Song> CachedSongs = InitializeCache();
        private static readonly SongRequestManager SongRequestManager = new SongRequestManager();
        private static readonly ConcurrentBag<Guid> PeopleWhoHaveUsedChavah = new ConcurrentBag<Guid>();
        
        public JsonResult GetSongMatches(string searchText)
        {
            var allSongs = CachedSongs;
            var matchingSongNames = allSongs.Where(s => s.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase));
            var matchingArtists = allSongs.Where(s => s.Artist.Contains(searchText, StringComparison.OrdinalIgnoreCase));
            var matchingAlbums = allSongs.Where(s => s.Album.Contains(searchText, StringComparison.OrdinalIgnoreCase));
            var matchingGenres = allSongs.Where(s => s.Genre.Contains(searchText, StringComparison.OrdinalIgnoreCase));
            var matchingCountries = allSongs.Where(s => s.Country.Contains(searchText, StringComparison.OrdinalIgnoreCase));
            var matches = matchingSongNames
                .Concat(matchingArtists)
                .Concat(matchingAlbums)
                .Concat(matchingGenres)
                .Concat(matchingCountries)
                .Take(50)
                .Select(s => s.ToDto(SongLike.None));
            return Json(matches, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSongs()
        {
            var allSongs = CachedSongs;
           
            return Json(allSongs, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult UpdateSongs(IEnumerable<Song> songs)
        {
            using (var session = RavenStore.Instance.OpenSession())
            {
                foreach (var song in songs)
                {
                    session.Store(song);
                }

                session.SaveChanges();
            }

            return Json(songs, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSongForClient(Guid clientId)
        {
            try
            {
                OnSongRequested(clientId);
                var song = GetSongForClientWithLikeWeights(clientId);
                return Json(song, JsonRequestBehavior.AllowGet);
            }
            catch (Exception error)
            {
                RavenStore.Log(error.ToString());
                throw;
            }
        }

        public FileResult GetSongAlbumArt(int songId)
        {
            using (var session = RavenStore.Instance.OpenSession())
            {
                var song = session.Query<Song>().First(s => s.Id == songId);
                var albumArtFile = Directory.EnumerateFiles(Constants.AlbumArtDirectory, string.Format("{0} - {1}*", song.Artist, song.Album)).FirstOrDefault();
                var artFileOrDefaultPath = albumArtFile != null ? albumArtFile : Path.Combine(Constants.AlbumArtDirectory, "default.jpg");
                return File(artFileOrDefaultPath, "image/jpeg");
            }
        }

        public FileResult GetSongFile(int songId)
        {
            using (var session = RavenStore.Instance.OpenSession())
            {
                var song = session.Query<Song>().First(s => s.Id == songId);
                var filePath = Path.Combine(Constants.MusicDirectory, song.FileName);
                return File(filePath, "audio/mpeg");
            }
        }

        public JsonResult GetSongById(Guid clientId, int songId)
        {
            OnSongRequested(clientId);
            using (var session = RavenStore.Instance.OpenSession())
            {
                var song = session.Query<Song>().FirstOrDefault(s => s.Id == songId);
                if (song != null)
                {
                    var like = Dependency.Get<LikesCache>()
                        .ForClient(clientId)
                        .FirstOrDefault(l => l.SongId == songId);
                    var songDto = song.ToDto(like.ToSongLikeEnum());
                    return Json(songDto, JsonRequestBehavior.AllowGet);
                }

                // This should never happen: a client requets a song ID that doesn't exist.
                var errorMessage = "Unable to find song with ID = " + songId.ToString();
                RavenStore.Log(errorMessage, session);
                throw new Exception(errorMessage);
            }
        }

        public int? GetRequestedSongId(Guid clientId)
        {
            return SongRequestManager.GetSongRequest(clientId);
        }

        public JsonResult GetSongForSongRequest(Guid clientId, int songId)
        {
            SongRequestManager.RequestSong(songId, clientId);
            return GetSongById(clientId, songId);
        }

        public void LikeById(Guid clientId, int songId)
        {
            UpdateLikeStatus(clientId, songId, SongLike.Like);
        }

        public void DislikeById(Guid clientId, int songId)
        {
            UpdateLikeStatus(clientId, songId, SongLike.Dislike);
        }

        public JsonResult GetSongByAlbum(Guid clientId, string album, string artist)
        {
            using (var session = RavenStore.Instance.OpenSession())
            {
                var albumSongs = session.Query<Song>().Where(s => s.Album == album);
                var song = albumSongs.ToList().RandomOrder().FirstOrDefault();
                if (song != null)
                {
                    return GetSongById(clientId, song.Id);
                }
                else
                {
                    RavenStore.Log("Unable to find an album matching name " + album);
                    return GetSongForClient(clientId);
                }
            }
        }

        public JsonResult GetSongByArtist(Guid clientId, string artist)
        {
            using (var session = RavenStore.Instance.OpenSession())
            {
                var artistSongs = session.Query<Song>().Where(s => s.Artist == artist);
                var song = artistSongs.ToList().RandomOrder().FirstOrDefault();
                if (song != null)
                {
                    return GetSongById(clientId, song.Id);
                }
                else
                {
                    RavenStore.Log("Unable to find a song name starting with " + artist);
                    return GetSongForClient(clientId);
                }
            }
        }

        public JsonResult GetTrendingSongs(int count)
        {
            using (var session = RavenStore.Instance.OpenSession())
            {
                var recentLikedSongIds = session
                    .Query<Like>()
                    .Where(l => l.LikeStatus == SongLike.Like)
                    .OrderByDescending(l => l.Date)
                    .Select(l => l.SongId)
                    .Take(count)
                    .AsEnumerable()
                    .Select(i => "songs-" + i.ToString())
                    .ToArray();

                var songs = session.Load<Song>(recentLikedSongIds)
                    .Where(s => s != null)
                    .Select(s => s.ToDto(SongLike.None));
                
                return Json(songs, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetRandomLikedSongs(Guid clientId, int count)
        {
            using (var session = RavenStore.Instance.OpenSession())
            {
                var user = session.Query<User>().FirstOrDefault(u => u.ClientIdentifier == clientId);
                if (user != null)
                {
                    var likedSongIds = session
                        .Query<Like>()
                        .Customize(x => x.RandomOrdering())
                        .Where(l => l.LikeStatus == SongLike.Like && l.UserId == user.Id)
                        .Select(l => l.SongId)
                        .Take(count)
                        .AsEnumerable()
                        .Select(i => "songs-" + i.ToString())
                        .ToArray();

                    var songs = session
                        .Load<Song>(likedSongIds)
                        .Where(s => s != null)
                        .AsEnumerable()
                        .Select(s => s.ToDto());

                    return Json(songs, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new Song[0], JsonRequestBehavior.AllowGet);
        }

        public Task UploadSong(Uri address, string fileName)
        {
            var downloader = new System.Net.WebClient();
            var filePath = Path.Combine(Constants.MusicDirectory, fileName);
            return Task.Factory.StartNew(() =>
                {
                    using (var webClient = new System.Net.WebClient())
                    {
                        webClient.DownloadFile(address, filePath);
                    }
                });
        }

        public JsonResult GetTopSongs(int count)
        {
            using (var session = RavenStore.Instance.OpenSession())
            {
                var results = session
                    .Query<Song>()
                    .OrderByDescending(s => s.CommunityRank)
                    .Take(count)
                    .ToArray()
                    .Select(s => s.ToDto(SongLike.None));

                return Json(results, JsonRequestBehavior.AllowGet);
            }
        }

        private void OnSongRequested(Guid clientId)
        {
            CreateUserIfNecessary(clientId);
            Task.Factory.StartNew(() => IncrementTotalPlayed(clientId));
        }

        private void CreateUserIfNecessary(Guid clientId)
        {
            var hasSeenUserToday = PeopleWhoHaveUsedChavah.Contains(clientId);
            if (!hasSeenUserToday)
            {
                PeopleWhoHaveUsedChavah.Add(clientId);
                using (var session = RavenStore.Instance.OpenSession())
                {
                    var user = session.Query<User>().FirstOrDefault(u => u.ClientIdentifier == clientId);
                    if (user == null)
                    {
                        session.Store(new User { ClientIdentifier = clientId });
                        session.SaveChanges();
                    }
                }
            }
        }

        private Song GetSongForClientWithLikeWeights(Guid clientId)
        {
            // Song weights algorithm described here:
            // http://stackoverflow.com/questions/3345788/algorithm-for-picking-thumbed-up-items/3345838#3345838

            var allSongs = CachedSongs;
            var likeDislikeSongs = Dependency.Get<LikesCache>().ForClient(clientId);
            var songsWithWeight =
                (
                    from song in allSongs
                    let likeStatus = GetLikeStatusForSong(song, likeDislikeSongs)
                    select new
                    {
                        Weight = GetSongWeight(song, likeStatus),
                        Like = likeStatus,
                        Info = song
                    }
                ).ToArray();
            var totalWeights = songsWithWeight.Sum(s => s.Weight);
            var randomWeight = RandomDoubleWithMaxValue(totalWeights);
            var runningWeight = 0.0;
            foreach (var song in songsWithWeight)
            {
                var newWeight = runningWeight + song.Weight;
                if (randomWeight >= runningWeight && randomWeight <= newWeight)
                {
                    return song.Info.ToDto(song.Like);
                }
                runningWeight = newWeight;
            }

            var errorMessage = "Unable to find random song. This should never happen. Random weight chosen was " + randomWeight.ToString() + ", max weight was " + totalWeights.ToString();
            RavenStore.Log(errorMessage);
            throw new Exception(errorMessage);
        }

        private static double GetSongWeight(Song song, SongLike likeStatus)
        {
            var likeWeightMultiplier = GetSongLikeWeightMultiplier(likeStatus);
            var popularityWeight = GetPopularityWeight(song);

            var proposedFinalWeight = popularityWeight * likeWeightMultiplier;
            return proposedFinalWeight.MinMax(.001, 10);
        }

        private static double GetSongLikeWeightMultiplier(SongLike likeStatus)
        {
            const double normalMultiplier = 1;
            const double likeMultiplier = 1.5;
            const double dislikeMultiplier = 0.01;

            return Match.Value(likeStatus)
                .With(SongLike.Like, likeMultiplier)
                .With(SongLike.Dislike, dislikeMultiplier)
                .DefaultTo(normalMultiplier);
        }

        private static double GetPopularityWeight(Song song)
        {
            const double veryUnpopularWeight = 0.01;
            const double unpopularWeight = 0.1;
            const double normalWeight = 1;
            const double popularWeight = 1.25;
            const double veryPopularWeight = 1.50;
            const double extremelyPopularWeight = 1.75;

            return Match.Value(song.CommunityRank)
                .With(v => v < -5, veryUnpopularWeight)
                .With(v => v < 0, unpopularWeight)
                .With(v => v > 30, extremelyPopularWeight)
                .With(v => v > 20, veryPopularWeight)
                .With(v => v > 10, popularWeight)
                .With(v => v >= 0, normalWeight)
                .DefaultTo(0);
        }

        private static double RandomDoubleWithMaxValue(double maxValueInclusive)
        {
            var randomValue = Random.NextDouble();
            var desiredValue = randomValue * maxValueInclusive;
            var desiredValueTrimmed = Math.Min(maxValueInclusive, desiredValue);
            return desiredValueTrimmed;
        }

        private SongLike GetLikeStatusForSong(Song song, Like[] userSongPreferences)
        {
            var likeDislikeForThisSong = userSongPreferences.FirstOrDefault(l => l.SongId == song.Id);
            return likeDislikeForThisSong.ToSongLikeEnum();
        }

        private void IncrementTotalPlayed(Guid clientId)
        {
            using (var session = RavenStore.Instance.OpenSession())
            {
                var user = session.Query<User>().FirstOrDefault(u => u.ClientIdentifier == clientId);
                if (user != null)
                {
                    user.TotalPlays += 1;

                    var todaysDate = DateTime.Now.Date;
                    var visit = session.Query<Visit>().FirstOrDefault(v => v.UserId == user.Id && v.DateTime == todaysDate);
                    if (visit != null)
                    {
                        visit.TotalPlays += 1;
                    }
                    else
                    {
                        session.Store(new Visit()
                        {
                            TotalPlays = 1,
                            UserId = user.Id,
                            DateTime = todaysDate
                        });
                    }
                }
                else
                {
                    // Should never reach here.
                    RavenStore.Log("Unable to increment total plays because user wasn't in the database.", session);
                }

                session.SaveChanges();
            }
        }

        private static List<Song> InitializeCache()
        {
            FindNewSongsOnDisk();
            var allSongsInDatabase = GetCachedSongs();
            var deletedSongs = WeedOutSongsDeletedFromDatabase(allSongsInDatabase);
            return allSongsInDatabase.Except(deletedSongs).ToList();
        }

        private static List<Song> WeedOutSongsDeletedFromDatabase(List<Song> allSongsInDatabase)
        {
            var songsOnDisk = Directory
                .EnumerateFiles(Constants.MusicDirectory, "*.mp3")
                .Select(Path.GetFileName)
                .ToArray();

            var songsDeletedFromDisk = allSongsInDatabase
                .AsParallel()
                .Where(s => !songsOnDisk.Any(filePath => s.FileName == filePath))
                .ToList(); 

            using (var session = RavenStore.Instance.OpenSession())
            {
                var deleteCommands = songsDeletedFromDisk
                    .Select(s => new Raven.Abstractions.Commands.DeleteCommandData { Key = "songs-" + s.Id.ToString() })
                    .AsEnumerable<Raven.Abstractions.Commands.ICommandData>()
                    .ToArray();
                session.Advanced.Defer(deleteCommands);
                session.SaveChanges();
            }

            return songsDeletedFromDisk;
        }

        private static List<Song> GetCachedSongs()
        {
            var allCachedSongs = new List<Song>(1500);
            var take = 100;
            try
            {
                var skip = 0;
                var lastResultsCount = -1;
                while (lastResultsCount != 0)
                {
                    var nextChunk = GetCachedSongs(skip, take);
                    allCachedSongs.AddRange(nextChunk);
                    lastResultsCount = nextChunk.Count;
                    skip += lastResultsCount;
                }
            }
            catch (Exception error)
            {
                RavenStore.Log("Unable to get cached songs: " + error.ToString());
                throw;
            }


            if (allCachedSongs.Count == 0)
            {
                var errorMessage = "You've got no songs in your /Content/Music directory. Put some in, and you'll be good to go.";
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    System.Diagnostics.Debugger.Break();
                }
                throw new NotSupportedException(errorMessage);
            }

            return allCachedSongs;
        }

        private static List<Song> GetCachedSongs(int skip, int take)
        {
            using (var session = RavenStore.Instance.OpenSession())
            {
                var songsInDatabase = session.Query<Song>()
                    .OrderBy(s => s.Id)
                    .Skip(skip)
                    .Take(take)
                    .ToList();

                return songsInDatabase;
            }
        }

        private static void FindNewSongsOnDisk()
        {
            var songsOnDisk = Directory
                .GetFiles(Constants.MusicDirectory, "*.mp3")
                .Select(Path.GetFileName);

            songsOnDisk.ForEach(EnsureSongExistsInDatabase);
        }

        private static void EnsureSongExistsInDatabase(string filePath)
        {
            using (var session = RavenStore.Instance.OpenSession())
            {
                var song = session.Query<Song>().FirstOrDefault(s => s.FileName == filePath);
                if (song == null)
                {
                    var newSong = new Song(filePath);
                    session.Store(newSong);
                    session.SaveChanges();
                }
            }
        }

        private static void DeleteLikesForSongsAsync(Song[] songs)
        {
            songs.AsParallel().ForAll(DeleteLikesForSong); 
        }

        private static void DeleteLikesForSong(Song song)
        {
            var remainingLikes = int.MaxValue;
            while (remainingLikes > 0)
            {
                using (var session = RavenStore.Instance.OpenSession())
                {
                    var likesForSong = session.Query<Like>().Where(l => l.SongId == song.Id).ToArray();
                    likesForSong.ForEach(session.Delete);
                    session.SaveChanges();

                    remainingLikes = likesForSong.Length;
                }
            }
        }

        private static void UpdateLikeStatus(Guid clientId, int songId, SongLike likeStatus)
        {
            using (var session = RavenStore.Instance.OpenSession())
            {
                var user = session.Query<User>().FirstOrDefault(u => u.ClientIdentifier == clientId);
                if (user != null)
                {
                    var existingLike = session.Query<Like>().FirstOrDefault(l => l.SongId == songId && l.UserId == user.Id);
                    if (existingLike != null)
                    {
                        existingLike.LikeStatus = likeStatus;
                    }
                    else
                    {
                        var newLikeStatus = new Like()
                        {
                            LikeStatus = likeStatus,
                            SongId = songId,
                            UserId = user.Id,
                            Date = DateTime.Now
                        };
                        session.Store(newLikeStatus);
                    }

                    // Update the community rank.
                    var songInDb = session.Query<Song>().FirstOrDefault(s => s.Id == songId);
                    if (songInDb != null)
                    {
                        songInDb.CommunityRank += likeStatus == SongLike.Like ? 1 : -1;
                    }

                    session.SaveChanges();
                }
            }

            Dependency.Get<LikesCache>().OnLikesChanged(clientId);

            // Update the in-memory item.
            CachedSongs
                .Where(s => s.Id == songId)
                .ForEach(s => s.CommunityRank += likeStatus == SongLike.Like ? 1 : -1);
        }
    }
}
