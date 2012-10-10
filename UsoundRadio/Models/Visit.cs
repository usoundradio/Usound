using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UsoundRadio.Models
{
    public class Visit
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public DateTime DateTime { get; set; }
        public int TotalPlays { get; set; }
    }
}