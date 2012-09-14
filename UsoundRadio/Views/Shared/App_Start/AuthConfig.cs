using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.WebPages.OAuth;
using UsoundRadio.Models;

namespace UsoundRadio
{
    public static class AuthConfig
    {

        public static void RegisterAuth()
        {
            // To let users of this site log in using their accounts from other sites such as Microsoft, Facebook, and Twitter,
            // you must update this site. For more information visit http://go.microsoft.com/fwlink/?LinkID=252166

            //OAuthWebSecurity.RegisterMicrosoftClient(
            //    clientId: "",
            //    clientSecret: "");

           OAuthWebSecurity.RegisterTwitterClient(
               consumerKey: "JvyS7DO2qd6NNTsXJ4E7zA",
                consumerSecret: "9z6157pUbOBqtbm0A0q4r29Y2EYzIHlUwbF4Cl9c");

            OAuthWebSecurity.RegisterFacebookClient(
                appId: "454524694568019",
                appSecret: "fc825e254d05279469c22de6a86a8715");

            OAuthWebSecurity.RegisterGoogleClient();
        }
    }
}
