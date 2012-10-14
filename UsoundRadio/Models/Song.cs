using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Diagnostics.Contracts;
using UsoundRadio.Common;
using System.Web.Hosting;
using System.IO;

namespace UsoundRadio.Models
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
            else
            {
                // No hyphens; the file name isn't formatted as expected.
                // Use the file name as the song name, put "Unknown" in everything else.
                this.Name = Path.GetFileNameWithoutExtension(fileName);
                this.Artist = "Unknown Artist";
                this.Album = "Unknown Album";
                this.Country = "Unknown Country";
                this.Genre = "Unknown Genre";
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
        public int Number { get; set; }
        public string Album { get; set; }
        public string Artist { get; set; }
        public SongLike SongLike { get; set; }
        public int CommunityRank { get; set; }
        public CommunityRankStanding CommunityRankStanding { get; set; }
        public string Genre { get; set; }
        public string Country { get; set; }
        public string Description { get; set; }
        public string FileName { get; set; }
        public string Id { get; set; }
        public Uri PurchaseUri { get; set; }
        public Uri AlbumArtUri { get; set; }
        public Uri Uri { get; set; }

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
                Number = this.Number,
                Genre = this.Genre,
                Description = this.Description,
                Country = this.Country,

                PurchaseUri = this.PurchaseUri,
                AlbumArtUri = GetAlbumArtUri(),
                Uri = GetSongUri()
            };
        }

        /// <summary>
        /// Creates a new song object that's ready to be sent as a data transfer object over to the client.
        /// </summary>
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
    }
}