using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UsoundRadio.Models
{
    public class Like
    {
        public int Id { get; set; }
        public SongLike LikeStatus { get; set; }
        public int UserId { get; set; }
        public int SongId { get; set; }
        public DateTime Date { get; set; }
    }
}