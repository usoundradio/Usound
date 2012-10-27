chavah.mainViewModel = function (ui) {
    var self = this;
    this.songRequestManager = new chavah.songRequestManager();
    this.userId = chavah.localstorage.getOrCreateUserId();
    this.currentSong = ko.observable();
    this.songs = ko.observableArray();
    this.songRequestVisible = ko.observable(false);
    this.selectedSongRequest = ko.observable();
    this.closeSongRequestWindow = function () { self.songRequestVisible(false) }
    this.songRequestMatches = ko.observableArray();
    this.songRequestText = ko.observable('');
    this.mediaState = ko.observable();
    this.currentMediaUri = ko.observable();
    this.playlist = ko.observableArray();
    var Song = function (Name) {
        this.Name = ko.observable(Name);
    };
    var PlaylistItem = function (song) {
        var self = this; // Scope Trick

        self.song = ko.observable(song);

       
    };

    // The current media URI is usually the currentSong.Uri. But for song requests, we play the request announcement URI.
    this.currentSong.subscribe(function (newSong) {
        if (newSong) {
            self.currentMediaUri(newSong.uri);
        }
    });
    //PLAYLIST Starts Here!!!
    //add song to Playlist
    self.addtoPlaylist = function (song, event) {
        var playlist_item = new PlaylistItem(song, 1);

        self.playlist.push(playlist_item);
    };
    self.removeFromPlaylist = function (playlist_item, event) {
        self.playlist.remove(playlist_item);
    };
    self.playPlaylist = function (playlist_item, event) {
        var selectedSong = playlist_item;
        if (selectedSong) {
            self.songRequestManager.requestSong(selectedSong);
        }
   
    };
    $.getJSON("/Songs/getsongmatches?searchText=", function (songs) {
        
        self.songs(songs);

    });
    //END OF PLAYLIST

    this.trendingSongs = ko.observableArray([
       new chavah.songPlaceHolder("TrendingSongResults", 0),
       new chavah.songPlaceHolder("TrendingSongResults", 1),
       new chavah.songPlaceHolder("TrendingSongResults", 2),
               new chavah.songPlaceHolder("TrendingSongResults", 3),
           new chavah.songPlaceHolder("TrendingSongResults", 4),
           new chavah.songPlaceHolder("TrendingSongResults", 5),
           new chavah.songPlaceHolder("TrendingSongResults", 6),
           new chavah.songPlaceHolder("TrendingSongResults", 7),
           new chavah.songPlaceHolder("TrendingSongResults", 8),
           new chavah.songPlaceHolder("TrendingSongResults", 9)
    ]);

    this.myLikedSongs = ko.observableArray([
        new chavah.songPlaceHolder("RandomLikedSongResults", 0),
        new chavah.songPlaceHolder("RandomLikedSongResults", 1),
        new chavah.songPlaceHolder("RandomLikedSongResults", 2),

                    new chavah.songPlaceHolder("RandomLikedSongResults", 3),
                    new chavah.songPlaceHolder("RandomLikedSongResults", 4),
                    new chavah.songPlaceHolder("RandomLikedSongResults", 5),
                    new chavah.songPlaceHolder("RandomLikedSongResults", 6),
                    new chavah.songPlaceHolder("RandomLikedSongResults", 7),
                    new chavah.songPlaceHolder("RandomLikedSongResults", 8),
	                new chavah.songPlaceHolder("RandomLikedSongResults", 9)
	            
            
    ]);

    this.topSongs = ko.observableArray([
        new chavah.songPlaceHolder("TopSongResults", 0),
        new chavah.songPlaceHolder("TopSongResults", 1),
        new chavah.songPlaceHolder("TopSongResults", 2),
        new chavah.songPlaceHolder("TopSongResults", 3),
        new chavah.songPlaceHolder("TopSongResults", 4),
        new chavah.songPlaceHolder("TopSongResults", 5),
        new chavah.songPlaceHolder("TopSongResults", 6),
        new chavah.songPlaceHolder("TopSongResults", 7),
        new chavah.songPlaceHolder("TopSongResults", 8),
	    new chavah.songPlaceHolder("TopSongResults", 9),
	    new chavah.songPlaceHolder("TopSongResults", 10)
     
    ]);

    this.recentSongs = ko.observableArray([
        new chavah.songPlaceHolder(),
        new chavah.songPlaceHolder(),
        new chavah.songPlaceHolder(),
	            new chavah.songPlaceHolder(),
	            new chavah.songPlaceHolder(),
	            new chavah.songPlaceHolder(),
	            new chavah.songPlaceHolder(),
	            new chavah.songPlaceHolder(),
	            new chavah.songPlaceHolder(),
         new chavah.songPlaceHolder()
            

    ]);

    this.hasLikedAnySongs = ko.computed(function () {
        return self.myLikedSongs().some(function (s) { return s.hasSong() });
    });

    this.hasAnyRecentSongs = ko.computed(function () {
        return self.recentSongs().some(function (s) { return s.hasSong() });
    });

    this.playPause = function () {
        // PublishSync is necessary because iPhone has some really screwed up <audio> play policies:
        // - You can't play HTML5 audio until the UI has been activated.
        // - The activation of the page must immediately (e.g. synchronously) start the audio.
        // Thanks, Apple. You suck.
        PubSub.publishSync("TogglePlayPause");
    }

    this.pushRecentSong = function (song) {
        var firstSong = self.recentSongs()[0].song();
        var secondSong = self.recentSongs()[1].song();
        var thirdSong = self.recentSongs()[2].song();
        var fourthSong = self.recentSongs()[3].song();
        var fifthSong = self.recentSongs()[4].song();
        var sixthSong = self.recentSongs()[5].song();
        var seventhSong = self.recentSongs()[6].song();
        var eighthSong = self.recentSongs()[7].song();
        var ninethSong = self.recentSongs()[8].song();
        var tenthSong = self.recentSongs()[9].song();
        

        setTimeout(function () { self.recentSongs()[0].enqueuedSong(firstSong) }, 1500);
        setTimeout(function () { self.recentSongs()[1].enqueuedSong(secondSong) }, 1750);
        setTimeout(function () { self.recentSongs()[2].enqueuedSong(thirdSong) }, 1750);
        setTimeout(function () { self.recentSongs()[3].enqueuedSong(fourthSong) }, 2000);
        setTimeout(function () { self.recentSongs()[4].enqueuedSong(fifthSong) }, 2250);
        setTimeout(function () { self.recentSongs()[5].enqueuedSong(sixthSong) }, 2500);
        setTimeout(function () { self.recentSongs()[6].enqueuedSong(seventhSong) }, 1750);
        setTimeout(function () { self.recentSongs()[7].enqueuedSong(eighthSong) }, 1750);
        setTimeout(function () { self.recentSongs()[8].enqueuedSong(ninethSong) }, 1750);
        setTimeout(function () { self.recentSongs()[9].enqueuedSong(tenthSong) }, 2000);



    }

    this.toggleSongRequestDialogVisible = function () {
        self.songRequestVisible(!self.songRequestVisible());
    }


    this.fadeIn = function (element) {
        if (element.nodeType === 1) {
            $(element).hide().fadeIn("slow");
        }
    }
    this.twitterShareLink = ko.computed(function () {
        var songToShare = self.currentSong();
        if (songToShare) {
            var tweetText = 'Listening to "' + songToShare.artist + " - " + songToShare.name + '"';
            var url = "http://usoundradio.com/Home/Radio?song=" + songToShare.id;
            var via = "usoundradio";
            return "https://twitter.com/share?text=" + escape(tweetText) + "&url=" + escape(url) + "&via=" + escape(via);
        }
        return "#";
    });

    this.facebookShareLink = ko.computed(function () {
        var songToShare = self.currentSong();
        if (songToShare) {
            var name = songToShare.artist + " - " + songToShare.name;
            var albumurl = "usoundradio.com/Content/AlbumArt/" + songToShare.artist + " - " + songToShare.album + ".jpg";
            var url = "http://usoundradio.com/Home/Radio?song=" + songToShare.id;
            return "https://www.facebook.com/dialog/feed?app_id=454524694568019" +
                "&link=" + url +
                "&picture=" + escape(albumurl) +
                "&name=" + escape(name) +
                "&caption=" + escape("On " + songToShare.album) +
                "&description=" + escape("Usound Radio - Built by Musicians, Driven by Listeners.") +
                "&redirect_uri=" + escape("http://www.usoundradio.com")
        }
        return "#";
    });
    this.nextSong = function () {
        PubSub.publish("NextSong");
    }

    this.uploadMusic = function () {
        filepicker.getFile(['audio/mpeg'], {
            'multiple': true,
            'container': 'window',
            'services': [
                filepicker.SERVICES.COMPUTER,
                filepicker.SERVICES.URL,
                filepicker.SERVICES.DROPBOX,
                filepicker.SERVICES.GOOGLE_DRIVE,
                filepicker.SERVICES.GMAIL
            ]
        }, function (uploadedFiles) {
            // This function is invoked when the files finish uploading.
            // We're passed uploadedFiles, which is an array of objects that look like this:
            // { url: "http://filepickerserver/asdfqw35134", data: { size: 12342134, type: "audio/mpeg", filename: "foo.mp3" } }
            uploadedFiles.forEach(function (file) {
                var postArgs = { url: "/UploadSong", responseMessage: "SongUploadCompleted", data: { address: file.url, fileName: file.data.filename } };
                PubSub.publish("Post", postArgs);
            });
        });
    }

    this.requestSelectedSong = function () {
        var selectedSong = self.selectedSongRequest();
        if (selectedSong) {
            self.songRequestManager.requestSong(selectedSong);
        }
        self.selectedSongRequest(null);
        self.songRequestText("");
        self.closeSongRequestWindow();
    }

    this.songRequestMatchClicked = function (song) {
        self.selectedSongRequest(song);
        self.songRequestText(song.artist + " - " + song.name);
        self.songRequestMatches.removeAll();
    }

    var playerOptions = {
        flashAudioPlayerPath: chavah.constants.contentDirectory + "/swf/player.swf",
        swfobjectPath: chavah.constants.scriptsDirectory + "/swfobject.js",
        currentMedia: self.currentMediaUri
    };
    this.player = ui.audiocontrol(playerOptions);
    this.player.mediaState.subscribe(function (mediaState) {
        if (mediaState == "ended") {
            self.nextSong();
        }
    });

    this.playImage = ko.computed(function () {
        var relativeImagePath = self.player.mediaState() === "paused" ? "/images/play.png" : "/images/pause.png";
        return chavah.constants.contentDirectory + relativeImagePath;
    });

    // Song request fetching
    var songRequestTextThrottled = ko.computed(function () {
        return self.songRequestText();
    }).extend({ throttle: 500 });
    this.songRequestText.subscribe(function (requestText) {
        PubSub.publish("Get", { url: "/GetSongMatches", data: { searchText: requestText }, responseMessage: "SongMatchesResults" });
    });

    // When we click next song, it should pause the current song, then
    // do a fetch for another song.
    PubSub.subscribe("NextSong", function (message, args) {
        PubSub.publish("Pause");

        // When a song ends, play the next song.
        // Or, if there's a pending song request, play that.
        if (self.songRequestManager.hasPendingRequest()) {
            self.songRequestManager.playSongRequest();
        }
        else {
            // No song requests pending. Just grab the next song.
            PubSub.publish("Get", {
                url: "/GetSongForClient",
                data: { clientId: chavah.localstorage.getOrCreateUserId() },
                responseMessage: "SongFetched"
            });
        }
    });

    PubSub.subscribe("SongUploadCompleted", function (message, args) {
        // When a song upload finishes, refetch the top songs immediately.
        PubSub.publish("Get", {
            url: "/GetTopSongs",
            data: { count: 31 },
            responseMessage: "TopSongResults"
        });

        PubSub.publish("RefreshSongsGrid");
    });

    PubSub.subscribe("SetCurrentMediaUri", function (message, args) {
        self.currentMediaUri(args.uri);
    });

    PubSub.subscribe("SongMatchesResults", function (message, args) {
        var songs = args.map(function (s) { return new Song(s) });
        self.songRequestMatches(songs);
    });


    // Start fetching the data for the song columns.
    chavah.dataServices.recurringFetch("/GetRandomLikedSongs", { clientId: this.userId, count: 31 }, 0, 15000, "RandomLikedSongResults");
    chavah.dataServices.recurringFetch("/GetTrendingSongs", { count: 10 }, 300, 3000, "TrendingSongResults");
    chavah.dataServices.recurringFetch("/GetTopSongs", { count: 31 }, 600, 60000, "TopSongResults");
}