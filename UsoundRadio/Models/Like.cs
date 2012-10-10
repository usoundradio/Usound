using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UsoundRadio.Models
{
    public class Like
    {
        public string Id { get; set; }
        public SongLike LikeStatus { get; set; }
        public string UserId { get; set; }
        public string SongId { get; set; }
        public DateTime Date { get; set; }
    }
}