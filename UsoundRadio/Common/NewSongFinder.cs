using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Raven.Client;
using Raven.Client.Linq;
using UsoundRadio.Models;
using System.Threading.Tasks;

namespace UsoundRadio.Common
{
    /// <summary>
    /// Syncs the songs on disk with the songs in the database.
    /// </summary>
    public class NewSongFinder
    {
        public Task FindSongsAsync()
        {
            // Asynchronously find new songs on disk and delete missing ones from the database.
            var findNewSongs = Task.Factory.StartNew(FindNewSongsOnDisk);
            var deleteMissingSongs = findNewSongs.ContinueWith(_ => RemoveSongsDeletedFromDisk());
            var deleteLikes = deleteMissingSongs.ContinueWith(t => RemoveLikesForRemovedSongs(t.Result));
            return deleteMissingSongs;
        }

        private void FindNewSongsOnDisk()
        {
            var songsOnDiskChunks = Directory
                .GetFiles(Constants.MusicDirectory, "*.mp3")
                .Select(Path.GetFileName)
                .Chunk(25); 

            foreach (var songChunk in songsOnDiskChunks)
            {
                using (var session = Get.A<IDocumentStore>().OpenSession())
                {
                    foreach (var fileName in songChunk)
                    {
                        var songInDb = session
                            .Query<Song>()
                            .FirstOrDefault(s => s.FileName == fileName);
                        if (songInDb == null)
                        {
                            session.Store(new Song(fileName));                            
                        }
                    }

                    session.SaveChanges();
                }
            }
        }

        private IReadOnlyCollection<string> RemoveSongsDeletedFromDisk()
        {
            var allDeletedSongIds = new List<string>();
            var songsOnDisk = Directory
                .EnumerateFiles(Constants.MusicDirectory, "*.mp3")
                .Select(Path.GetFileName)
                .ToArray();

            var skip = 0;
            var take = 25;
            while (true)
            {
                using (var session = Get.A<IDocumentStore>().OpenSession())
                {
                    var songs = session
                        .Query<Song>()
                        .Customize(x => x.WaitForNonStaleResultsAsOfNow())
                        .Skip(skip)
                        .Take(take)
                        .ToArray();

                    if (songs.Length == 0)
                    {
                        break;
                    }
                    
                    var deletedSongIds = songs
                        .Where(s => !songsOnDisk.Contains(s.FileName, StringComparer.InvariantCultureIgnoreCase))
                        .Select(s => s.Id);
                    allDeletedSongIds.AddRange(deletedSongIds);

                    var deleteSongCommands = deletedSongIds
                        .Select(id => new Raven.Abstractions.Commands.DeleteCommandData { Key = "songs/" + id.ToString() })
                        .AsEnumerable<Raven.Abstractions.Commands.ICommandData>()
                        .ToArray();

                    session.Advanced.Defer(deleteSongCommands.ToArray());
                    session.SaveChanges();

                    skip += songs.Length;
                }
            }

            return allDeletedSongIds.AsReadOnly();
        }

        private void RemoveLikesForRemovedSongs(IReadOnlyCollection<string> removedSongIds)
        {
            using (var session = Get.A<IDocumentStore>().OpenSession())
            {
                var deleteCommands = session.Query<Like>()
                    .Where(l => l.SongId.In(removedSongIds.ToArray()))
                    .AsEnumerable()
                    .Select(id => new Raven.Abstractions.Commands.DeleteCommandData { Key = "likes/" + id.ToString() })
                    .AsEnumerable<Raven.Abstractions.Commands.ICommandData>();
                session.Advanced.Defer(deleteCommands.ToArray());
                session.SaveChanges();
            }
        }
    }
}