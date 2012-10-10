using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UsoundRadio.Common;
using UsoundRadio.Data;

namespace UsoundRadio.Models
{
    public class UserSongPreferences
    {
        public UserSongPreferences()
        {
            Artists = new List<LikeDislikeCount>();
            Albums = new List<LikeDislikeCount>();
            Songs = new List<LikeDislikeCount>();
        }

        public List<LikeDislikeCount> Artists { get; set; }
        public List<LikeDislikeCount> Albums { get; set; }
        public List<LikeDislikeCount> Songs { get; set; }

        /// <summary>
        /// Picks a song for the user based his music preferences.
        /// </summary>
        /// <param name="totalSongCount">The total number of songs.</param>
        /// <returns></returns>
        public SongPick PickSong(int veryPoorRankedSongCount, int poorRankedSongCount, int normalRankedSongCount, int goodRankedSongCount, int greatRankedSongCount, int bestRankedSongCount)
        {
            // Based off of the song weights algorithm described here:
            // http://stackoverflow.com/questions/3345788/algorithm-for-picking-thumbed-up-items/3345838#3345838

            var veryPoorRange = new Range(0, .001 * veryPoorRankedSongCount);
            var poorRange = new Range(veryPoorRange.End, .01 * poorRankedSongCount);
            var normalRange = new Range(poorRange.End, normalRankedSongCount);
            var goodRange = new Range(normalRange.End, 2 * goodRankedSongCount);
            var greatRange = new Range(goodRange.End, 3 * greatRankedSongCount);
            var bestRange = new Range(greatRange.End, 4 * bestRankedSongCount);
            
            var likedSongRange = new Range(bestRange.End, 4 * this.Songs.Count(s => s.LikeCount == 1));
            var likedAlbumRange = new Range(likedSongRange.End, 3 * GetLikedAlbums().Count());
            var likedArtistRange = new Range(likedAlbumRange.End, 3 * GetLikedArtists().Count());

            var totalRange = likedArtistRange.End;
            var randomRange = new Random().Range(totalRange);

            return Match.Value(randomRange)
                .With(0, SongPick.RandomSong)
                .With(veryPoorRange.IsWithinRange, SongPick.VeryPoorRank)
                .With(poorRange.IsWithinRange, SongPick.PoorRank)
                .With(normalRange.IsWithinRange, SongPick.NormalRank)
                .With(goodRange.IsWithinRange, SongPick.GoodRank)
                .With(greatRange.IsWithinRange, SongPick.GreatRank)
                .With(bestRange.IsWithinRange, SongPick.BestRank)
                .With(likedSongRange.IsWithinRange, SongPick.LikedSong)
                .With(likedAlbumRange.IsWithinRange, SongPick.LikedAlbum)
                .With(likedArtistRange.IsWithinRange, SongPick.LikedArtist)
                .DefaultTo(SongPick.RandomSong);
        }

        public void Update(Song song, SongLike likeStatus)
        {
            UpdateSongs(song, likeStatus);
            UpdateArtists(song, likeStatus);
            UpdateAlbums(song, likeStatus);
        }

        public SongLike GetLikeStatus(Song song)
        {
            var songPreference = this.Songs.FirstOrDefault(s => s.Name == song.Id);
            return Match
                .Value(songPreference)
                .With(default(LikeDislikeCount), SongLike.None)
                .With(l => l.LikeCount == 1, SongLike.Like)
                .With(l => l.DislikeCount == 1, SongLike.Dislike);
        }

        public IEnumerable<LikeDislikeCount> GetLikedAlbums()
        {
            return this.Albums.Where(a => a.LikeCount > 5 && a.DislikeCount <= 1);
        }

        public IEnumerable<LikeDislikeCount> GetLikedArtists()
        {
            return this.Artists.Where(a => a.LikeCount > 10 && a.DislikeCount <= 3);
        }

        public IEnumerable<LikeDislikeCount> GetDislikedSongs()
        {
            return this.Songs.Where(a => a.DislikeCount == 1);
        }

        private void UpdateSongs(Song song, SongLike likeStatus)
        {
            var existingLike = this.Songs.FirstOrDefault(s => s.Name == song.Id);
            if (existingLike != null)
            {
                existingLike.LikeCount = likeStatus == SongLike.Like ? 1 : 0;
                existingLike.DislikeCount = likeStatus == SongLike.Dislike ? 1 : 0;
            }
            else
            {
                this.Songs.Add(new LikeDislikeCount
                {
                    Name = song.Id,
                    LikeCount = likeStatus == SongLike.Like ? 1 : 0,
                    DislikeCount = likeStatus == SongLike.Dislike ? 1 : 0
                });
            }
        }

        private void UpdateArtists(Song song, SongLike likeStatus)
        {
            var existing = this.Artists.FirstOrDefault(s => s.Name == song.Artist);
            if (existing != null)
            {
                existing.LikeCount = likeStatus == SongLike.Like ? existing.LikeCount + 1 : existing.LikeCount;
                existing.DislikeCount = likeStatus == SongLike.Dislike ? existing.DislikeCount + 1 : existing.DislikeCount;
            }
            else
            {
                this.Artists.Add(new LikeDislikeCount
                {
                    Name = song.Artist,
                    LikeCount = likeStatus == SongLike.Like ? 1 : 0,
                    DislikeCount = likeStatus == SongLike.Dislike ? 1 : 0
                });
            }
        }

        private void UpdateAlbums(Song song, SongLike likeStatus)
        {
            var existing = this.Albums.FirstOrDefault(s => s.Name == song.Album);
            if (existing != null)
            {
                existing.LikeCount = likeStatus == SongLike.Like ? existing.LikeCount + 1 : existing.LikeCount;
                existing.DislikeCount = likeStatus == SongLike.Dislike ? existing.DislikeCount + 1 : existing.DislikeCount;
            }
            else
            {
                this.Albums.Add(new LikeDislikeCount
                {
                    Name = song.Album,
                    LikeCount = likeStatus == SongLike.Like ? 1 : 0,
                    DislikeCount = likeStatus == SongLike.Dislike ? 1 : 0
                });
            }
        }
    }
}