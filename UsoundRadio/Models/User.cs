using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UsoundRadio.Models
{
    public class User
    {
        public User()
        {
            this.Preferences = new UserSongPreferences();
        }

        public string Id { get; set; }
        public Guid ClientIdentifier { get; set; }
        public int TotalPlays { get; set; }
        public UserSongPreferences Preferences { get; set; }
    }
}