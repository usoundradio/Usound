using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UsoundRadio.Models
{
    public class SongRequest
    {
        public SongRequest()
        {
            PlayedForUserIds = new List<string>();
        }
        public string UserId { get; set; }
        public string SongId { get; set; }
        public DateTime DateTime { get; set; }
        public List<string> PlayedForUserIds { get; set; }
    }
}