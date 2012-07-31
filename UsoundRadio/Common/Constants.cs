using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace UsoundRadio.Common
{
    public class Constants
    {
        public static readonly string AlbumArtDirectory = HostingEnvironment.MapPath("~/Content/AlbumArt");
        public static readonly string MusicDirectory = HostingEnvironment.MapPath("~/Content/Music");
        
        public const string RelativeMusicDirectory = "/Content/Music";
    }
}