using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UsoundRadio.Models
{
    public class User
    {
        public int Id { get; set; }
        public Guid ClientIdentifier { get; set; }
        public int TotalPlays { get; set; }
    }
}