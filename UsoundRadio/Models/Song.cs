
    
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics.Contracts;
using UsoundRadio.Models;

namespace PrairieAsunder.Models
{
    public class Song
    {
        public Song()
        {
        }

        public Song(string fileName)
        {
            Contract.Requires(fileName != null);

            this.FileName = fileName;
            this.Number = 0;

            var fileNameWithouExtension = System.IO.Path.GetFileNameWithoutExtension(fileName);
            var indexOfLastDash = fileNameWithouExtension.LastIndexOf(" - ");
            if (indexOfLastDash != -1)
            {
                this.Name = fileNameWithouExtension
                    .Substring(indexOfLastDash)
                    .Replace('_', ':')
                    .Trim('-', ' ');
            }

            var indexOfFirstDash = fileNameWithouExtension.IndexOf(" - ");
            if (indexOfFirstDash != -1)
            {
                this.Artist = fileNameWithouExtension
                    .Substring(0, indexOfFirstDash)
                    .Trim('-', ' ');
            }
            var dashCount = fileNameWithouExtension.Count(c => c == '-');
            if (dashCount >= 2)
            {
                var indexOfSecondDash = fileNameWithouExtension.IndexOf(" - ", indexOfFirstDash + 2);
                if (indexOfSecondDash != -1)
                {
                    this.Album = fileNameWithouExtension
                        .Substring(indexOfFirstDash, indexOfSecondDash - indexOfFirstDash)
                        .Trim('-', ' ');
                }
            }

            var songNumberMatch = Regex.Match(fileNameWithouExtension, " - (\\d{2}) - ");
            if (songNumberMatch.Success && songNumberMatch.Groups.Count == 2 && songNumberMatch.Groups[1].Success)
            {
                this.Number = int.Parse(songNumberMatch.Groups[1].Value);
            }
        }

        public string Name { get; set; }

        public string Country { get; set; }

        public string Genre { get; set; }
        public int Number { get; set; }

        public string Album { get; set; }

        public string Artist { get; set; }
        public string Description { get; set; }
        public Uri AlbumArtUri { get; set; }

        public Uri PurchaseUri { get; set; }

        public Uri Uri { get; set; }

        public SongLike SongLike { get; set; }

        public int CommunityRank { get; set; }

        public string DebugInfo { get; set; }

        public string FileName { get; set; }

        public int Id { get; set; }

        /// <summary>
        /// Creates a new song object that's ready to be sent as a data transfer object over to the client.
        /// </summary>
        /// <param name="likeStatus">The like status for the song.</param>
        /// <returns></returns>
        public Song ToDto(SongLike likeStatus)
        {
            return new Song
            {
                Album = this.Album,
                Artist = this.Artist,
                CommunityRank = this.CommunityRank,
                Id = this.Id,
                SongLike = likeStatus,
                Name = this.Name,
                 Description = this.Description,
                Number = this.Number,
                Genre = this.Genre,
                Country = this.Country,

                AlbumArtUri = GetAlbumArtUri(),
                Uri = GetSongUri()
            };
        }

        /// <summary>
        /// Creates a new song object that's ready to be sent as a data transfer object over to the client.
        /// </summary>
        /// <param name="likeStatus">The like status for the song.</param>
        /// <returns></returns>
        public Song ToDto()
        {
            return ToDto(SongLike.None);
        }

        private Uri GetAlbumArtUri()
        {
            var relativeUri = "/Songs/GetSongAlbumArt?songId=" + this.Id.ToString();
            return new Uri(relativeUri, UriKind.Relative);
        }

        public Uri GetSongUri()
        {
            var relativeUri = "/Songs/GetSongFile?songId=" + this.Id.ToString();
            return new Uri(relativeUri, UriKind.Relative);
        }

        public static Song GetErrorSong(string error)
        {
            return new Song
            {
                Album = "Arise O Lord",
                Artist = "Israel's Hope",
                CommunityRank = 0,
                DebugInfo = error,
                Id = 0,
                Name = "Let Us Adore",
                Number = 6,
                  Genre = "Jewish",
                Country = "UnitedStates",
                PurchaseUri = new Uri("http://lmgtfy.com/?q=%22" + Uri.EscapeDataString("Israel's Hope") + "%22+messianic+music+purchase"),
                SongLike = SongLike.None,
                Uri = new Uri("http://judahhimango.com/music/messianic/Israel's Hope - Arise O Lord - 06 - Let Us Adore.mp3")
            };
        }
    }
}