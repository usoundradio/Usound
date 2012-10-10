using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Raven.Abstractions.Extensions;
using UsoundRadio.Data;
using UsoundRadio.Models;
using UsoundRadio.Common;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;
using Raven.Client.Linq;
using UsoundRadio.Data;
using Raven.Client;

namespace UsoundRadio.Controllers
{
    public class SongsController : RavenController
    {
        private static readonly Random random = new Random();

        public JsonResult GetSongMatches(string searchText)
        {
            var songMatches = this.RavenSession
                .Query<Song>()
                .Where(s =>
                    s.Name.StartsWith(searchText) ||
                    s.Artist.StartsWith(searchText) ||
                    s.Album.StartsWith(searchText))
                .Take(50)
                .AsEnumerable()
                .Select(s => s.ToDto());
            return Json(songMatches, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSongForClient(Guid clientId)
        {
            try
            {
                var user = OnSongRequestedByUser(clientId);
                var song = PickSongForUser(user);
                return Json(song, JsonRequestBehavior.AllowGet);
            }
            catch (Exception error)
            {
                this.Log(error.ToString());
                throw;
            }
        }

        public FileResult FacebookGetSongAlbumArt(string songNumber)
        {
            // The Facebook app doesn't like Raven IDs (e.g. songs/42),
            // even if the URL is encoded. Instead, we have to pass the song number (e.g. 42)
            // then get the album art from that.
            return GetSongAlbumArt("songs/" + songNumber);
        }

        public FileResult GetSongAlbumArt(string songId)
        {
            var song = this.RavenSession.Query<Song>().First(s => s.Id == songId);
            var albumArtFile = Directory.EnumerateFiles(Constants.AlbumArtDirectory, string.Format("{0} - {1}*", song.Artist, song.Album)).FirstOrDefault();
            var artFileOrDefaultPath = albumArtFile != null ? albumArtFile : Path.Combine(Constants.AlbumArtDirectory, "default.jpg");
            return File(artFileOrDefaultPath, "image/jpeg");
        }

        public FileResult GetSongFile(string songId)
        {
            var song = this.RavenSession.Query<Song>().First(s => s.Id == songId);
            var filePath = Path.Combine(Constants.MusicDirectory, song.FileName);
            return File(filePath, "audio/mpeg");
        }

        public JsonResult GetSongById(Guid clientId, string songId)
        {
            var user = OnSongRequestedByUser(clientId);
            var songLike = this.RavenSession
                .Query<Like>()
                .Customize(c => c.Include<Like>(l => l.SongId))
                .FirstOrDefault(s => s.UserId == user.Id && s.SongId == songId);

            if (songLike != null)
            {
                var song = this.RavenSession.Load<Song>(songId.ToString());
                return Json(song.ToDto(songLike.ToSongLikeEnum()), JsonRequestBehavior.AllowGet);
            }
            else
            {
                var song = this.RavenSession
                    .Query<Song>()
                    .FirstOrDefault(s => s.Id == songId);
                if (song != null)
                {
                    return Json(song.ToDto(), JsonRequestBehavior.AllowGet);
                }

                // This should never happen: a client requets a song ID that doesn't exist.
                var errorMessage = "Unable to find song with ID = " + songId.ToString();
                this.Log(errorMessage);
                throw new Exception(errorMessage);
            }
        }

        public JsonResult GetRequestedSongId(Guid clientId)
        {
            var user = RavenSession.Query<User>().FirstOrDefault(u => u.ClientIdentifier == clientId);
            if (user == null)
            {
                return null;
            }

            var recentSongRequests = RavenSession
                .Query<SongRequest>()
                .Take(10)
                .ToArray();

            // Delete old song requests.
            var recent = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(15));
            recentSongRequests
                .Where(s => s.DateTime < recent)
                .ForEach(RavenSession.Delete);

            var validSongRequest = recentSongRequests.FirstOrDefault(s => s.DateTime >= recent);
            if (validSongRequest != null)
            {
                validSongRequest.PlayedForUserIds.Add(user.Id);
                return Json(validSongRequest.SongId, JsonRequestBehavior.AllowGet);
            }

            return Json(null, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSongForSongRequest(Guid clientId, string songId)
        {
            var user = RavenSession.Query<User>().FirstOrDefault(u => u.ClientIdentifier == clientId);
            var userId = user != null ? user.Id : "0";
            var songRequest = new SongRequest
            {
                DateTime = DateTime.UtcNow,
                SongId = songId,
                UserId = userId
            };
            RavenSession.Store(songRequest);
            return GetSongById(clientId, songId);
        }

        public void LikeById(Guid clientId, string songId)
        {
            UpdateLikeStatus(clientId, songId, SongLike.Like);
        }

        public void DislikeById(Guid clientId, string songId)
        {
            UpdateLikeStatus(clientId, songId, SongLike.Dislike);
        }

        public JsonResult GetSongByAlbum(Guid clientId, string album, string artist)
        {
            var song = this.RavenSession
                .Query<Song>()
                .Customize(c => c.RandomOrdering())
                .FirstOrDefault(s => s.Album == album);
            if (song != null)
            {
                return GetSongById(clientId, song.Id);
            }
            else
            {
                var errorMessage = "Unable to find an album matching name " + album;
                this.Log(errorMessage);
                throw new Exception(errorMessage);
            }
        }

        public JsonResult GetSongByArtist(Guid clientId, string artist)
        {
            var song = this.RavenSession
                .Query<Song>()
                .Customize(c => c.RandomOrdering())
                .FirstOrDefault(s => s.Artist == artist);
            if (song != null)
            {
                return GetSongById(clientId, song.Id);
            }
            else
            {
                var errorMessage = "Unable to find a song name starting with " + artist;
                this.Log(errorMessage);
                throw new Exception(errorMessage);
            }
        }

        public JsonResult GetTrendingSongs(int count)
        {
            var recentLikedSongIds = this.RavenSession
                .Query<Like>()
                .Customize(c => c.Include<Like>(l => l.SongId))
                .Where(l => l.LikeStatus == SongLike.Like)
                .OrderByDescending(l => l.Date)
                .Select(l => l.SongId)
                .Take(count)
                .ToArray();

            var songs = this.RavenSession.Load<Song>(recentLikedSongIds)
                .Where(s => s != null)
                .Select(s => s.ToDto());
                
            return Json(songs, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetRandomLikedSongs(Guid clientId, int count)
        {
            var user = this.RavenSession.Query<User>().FirstOrDefault(u => u.ClientIdentifier == clientId);
            if (user != null)
            {
                var likedSongIds = this.RavenSession
                    .Query<Like>()
                    .Customize(x => x.RandomOrdering())
                    .Customize(x => x.Include<Like>(l => l.SongId))
                    .Where(l => l.LikeStatus == SongLike.Like && l.UserId == user.Id)
                    .Select(l => l.SongId)
                    .Take(count)
                    .ToArray();

                var songs = this.RavenSession
                    .Load<Song>(likedSongIds)
                    .Where(s => s != null)
                    .AsEnumerable()
                    .Select(s => s.ToDto());

                return Json(songs, JsonRequestBehavior.AllowGet);
            }

            return Json(new Song[0], JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetTopSongs(int count)
        {
            var results = this.RavenSession
                .Query<Song>()
                .OrderByDescending(s => s.CommunityRank)
                .Take(count)
                .ToArray()
                .Select(s => s.ToDto(SongLike.None));

            return Json(results, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateSongs(IEnumerable<Song> songs)
        {
            foreach (var song in songs)
            {
                this.RavenSession.Store(song);
            }

            return Json(songs, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UploadSong(Uri address, string fileName)
        {
            var downloader = new System.Net.WebClient();
            var filePath = Path.Combine(Constants.MusicDirectory, fileName);
            
            using (var webClient = new System.Net.WebClient())
            {
                webClient.DownloadFile(address, filePath);
            }

            // Find the song with the file name and replace it.
            // If no such song exists, create a new one.
            var song = RavenSession.Query<Song>().FirstOrDefault(s => s.FileName == fileName) ?? new Song(fileName);
            RavenSession.Store(song);
            return Json(song.ToDto(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSongs()
        {
            const int maxSongs = 25;
            var songs = RavenSession
                .Query<Song>()
                .Customize(c => c.RandomOrdering())
                .Take(maxSongs);
            return Json(songs, JsonRequestBehavior.AllowGet);
        }

        private User OnSongRequestedByUser(Guid clientId)
        {
            Contract.Ensures(Contract.Result<User>() != null);

            var user = CreateUserIfNecessary(clientId);
            IncrementTotalPlayed(user);
            return user;
        }

        private User CreateUserIfNecessary(Guid clientId)
        {
            var user = this.RavenSession
                .Query<User>()
                .FirstOrDefault(u => u.ClientIdentifier == clientId);
            if (user == null)
            {
                user = new User { ClientIdentifier = clientId };
                this.RavenSession.Store(user);
            }
            return user;
        }

        private Song PickSongForUser(User user)
        {
            // This is NOT an unbounded result set:
            // This queries the Songs_RankStandings index, which will reduce the results. Max number of results will be the number of CommunityRankStanding enum constants.
            var songRankStandings = this.RavenSession
                .Query<Song, Songs_RankStandings>()
                .As<Songs_RankStandings.Results>()
                .ToArray();

            var veryPoorRankSongCount = songRankStandings.Where(s => s.Standing == CommunityRankStanding.VeryPoor).Select(s => s.Count).FirstOrDefault();
            var poorRankSongCount = songRankStandings.Where(s => s.Standing == CommunityRankStanding.Poor).Select(s => s.Count).FirstOrDefault();
            var normalRankSongCount = songRankStandings.Where(s => s.Standing == CommunityRankStanding.Normal).Select(s => s.Count).FirstOrDefault();
            var goodRankSongCount = songRankStandings.Where(s => s.Standing == CommunityRankStanding.Good).Select(s => s.Count).FirstOrDefault();
            var greatRankSongCount = songRankStandings.Where(s => s.Standing == CommunityRankStanding.Great).Select(s => s.Count).FirstOrDefault();
            var bestRankSongCount = songRankStandings.Where(s => s.Standing == CommunityRankStanding.Best).Select(s => s.Count).FirstOrDefault();

            var pickRankedSong = new Func<CommunityRankStanding, Func<Song>>(standing => new Func<Song>(() => PickRankedSongForUser(standing, user)));
            var songPick = user.Preferences.PickSong(veryPoorRankSongCount, poorRankSongCount, normalRankSongCount, goodRankSongCount, greatRankSongCount, bestRankSongCount);
            var songPicker = Match.Value(songPick)
                .With(SongPick.VeryPoorRank, pickRankedSong(CommunityRankStanding.VeryPoor))
                .With(SongPick.PoorRank, pickRankedSong(CommunityRankStanding.Poor))
                .With(SongPick.NormalRank, pickRankedSong(CommunityRankStanding.Normal))
                .With(SongPick.GoodRank, pickRankedSong(CommunityRankStanding.Good))
                .With(SongPick.GreatRank, pickRankedSong(CommunityRankStanding.Great))
                .With(SongPick.BestRank, pickRankedSong(CommunityRankStanding.Best))
                .With(SongPick.LikedAlbum, () => PickLikedAlbumForUser(user))
                .With(SongPick.LikedArtist, () => PickLikedArtistForUser(user))
                .With(SongPick.LikedSong, () => PickLikedSongForUser(user))
                .With(SongPick.RandomSong, PickRandomSong)
                .Evaluate();
            var song = songPicker();
            if (song == null)
            {
                song = PickRandomSong();
            }

            return song.ToDto(user.Preferences.GetLikeStatus(song));
        }

        private Song PickRandomSong()
        {
            return RavenSession.Query<Song>()
                .Customize(c => c.RandomOrdering())
                .First();
        }

        private Song PickLikedSongForUser(User user)
        {
            var randomLikedSong = user.Preferences.Songs.Where(s => s.LikeCount == 1).RandomElement();
            if (randomLikedSong != null)
            {
                return RavenSession.Query<Song>().FirstOrDefault(s => s.Id == randomLikedSong.Name);
            }
            return null;
        }

        private Song PickLikedArtistForUser(User user)
        {
            var randomLikedArtist = user.Preferences.GetLikedArtists().RandomElement();
            if (randomLikedArtist != null)
            {
                return this.RavenSession.Query<Song>()
                    .Where(s => s.Artist == randomLikedArtist.Name)
                    .Customize(c => c.RandomOrdering())
                    .FirstOrDefault();
            }

            return null;
        }

        private Song PickLikedAlbumForUser(User user)
        {
            var randomLikedAlbum = user.Preferences.GetLikedAlbums().RandomElement();
            if (randomLikedAlbum != null)
            {
                return this.RavenSession.Query<Song>()
                    .Where(s => s.Album == randomLikedAlbum.Name)
                    .Customize(c => c.RandomOrdering())
                    .FirstOrDefault();
            }

            return null;
        }

        private Song PickRankedSongForUser(CommunityRankStanding rank, User user)
        {
            var dislikedSongIds = user.Preferences.GetDislikedSongs().Select(s => s.Name).ToArray();
            return this.RavenSession.Query<Song>()
                .Where(s => s.CommunityRankStanding == rank && !s.Id.In(dislikedSongIds))
                .Customize(x => x.RandomOrdering())
                .FirstOrDefault();
        }

        private void IncrementTotalPlayed(User user)
        {
            user.TotalPlays += 1;

            var todaysDate = DateTime.Now.Date;
            var visit = this.RavenSession.Query<Visit>().FirstOrDefault(v => v.UserId == user.Id && v.DateTime == todaysDate);
            if (visit != null)
            {
                visit.TotalPlays += 1;
            }
            else
            {
                this.RavenSession.Store(new Visit()
                {
                    TotalPlays = 1,
                    UserId = user.Id,
                    DateTime = todaysDate
                });
            }
        }

        private void UpdateLikeStatus(Guid clientId, string songId, SongLike likeStatus)
        {
            var user = this.RavenSession
                .Query<User>()
                .FirstOrDefault(u => u.ClientIdentifier == clientId);
            if (user != null)
            {
                var existingLike = this.RavenSession
                    .Query<Like>()
                    .FirstOrDefault(l => l.SongId == songId && l.UserId == user.Id);
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
                    this.RavenSession.Store(newLikeStatus);
                }

                // Update the community rank.
                var songInDb = this.RavenSession
                    .Query<Song>()
                    .FirstOrDefault(s => s.Id == songId);
                if (songInDb != null)
                {
                    songInDb.CommunityRank += likeStatus == SongLike.Like ? 1 : -1;
                    user.Preferences.Update(songInDb, likeStatus);

                    // Update song.CommunityRankStanding
                    var communityRankStats = this.RavenSession
                        .Query<Song, Songs_CommunityRankIndex>()
                        .As<Songs_CommunityRankIndex.Results>()
                        .FirstOrDefault();
                    if (communityRankStats != null)
                    {
                        var averageSongRank = Math.Max(0, (double)communityRankStats.RankSum / communityRankStats.SongCount);
                        var newStanding = Match.Value(songInDb.CommunityRank)
                            .With(v => v <= -5, CommunityRankStanding.VeryPoor)
                            .With(v => v <= -1, CommunityRankStanding.Poor)
                            .With(v => v <= averageSongRank, CommunityRankStanding.Normal)
                            .With(v => v <= averageSongRank * 2, CommunityRankStanding.Good)
                            .With(v => v <= averageSongRank * 3, CommunityRankStanding.Great)
                            .With(v => v <= averageSongRank * 4, CommunityRankStanding.Best)
                            .Evaluate();
                        songInDb.CommunityRankStanding = newStanding;
                    }
                }
            }
        }
    }
}
