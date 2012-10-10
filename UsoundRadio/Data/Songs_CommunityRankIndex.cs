using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UsoundRadio.Models;

namespace UsoundRadio.Data
{
    public class Songs_CommunityRankIndex : Raven.Client.Indexes.AbstractIndexCreationTask<Song, Songs_CommunityRankIndex.Results>
    {
        public Songs_CommunityRankIndex()
        {
            Map = songs => from song in songs
                           select new 
                           { 
                               SongCount = 1, 
                               RankSum = song.CommunityRank
                           };

            Reduce = results => from result in results
                                group result by result.SongCount into g
                                select new 
                                {
                                    SongCount = g.Sum(x => x.SongCount), 
                                    RankSum = g.Sum(x => x.RankSum)
                                };
        }

        public class Results
        {
            public int SongCount { get; set; }
            public int RankSum { get; set; }
        }
    }
}