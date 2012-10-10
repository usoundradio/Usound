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
    // The current media URI is usually the currentSong.Uri. But for song requests, we play the request announcement URI.
    this.currentSong.subscribe(function (newSong) {
        if (newSong) {
            self.currentMediaUri(newSong.uri);
        }
    });

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
           new chavah.songPlaceHolder("TrendingSongResults", 9),
           new chavah.songPlaceHolder("TrendingSongResults", 10)
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
	                new chavah.songPlaceHolder("RandomLikedSongResults", 9),
	                new chavah.songPlaceHolder("RandomLikedSongResults", 10),
                     new chavah.songPlaceHolder("RandomLikedSongResults", 11),
        new chavah.songPlaceHolder("RandomLikedSongResults", 12),

                    new chavah.songPlaceHolder("RandomLikedSongResults", 13),
                    new chavah.songPlaceHolder("RandomLikedSongResults", 14),
                    new chavah.songPlaceHolder("RandomLikedSongResults", 15),
                    new chavah.songPlaceHolder("RandomLikedSongResults", 16),
                    new chavah.songPlaceHolder("RandomLikedSongResults", 17),
                    new chavah.songPlaceHolder("RandomLikedSongResults", 18),
	                new chavah.songPlaceHolder("RandomLikedSongResults", 19),
	                new chavah.songPlaceHolder("RandomLikedSongResults", 20),
    new chavah.songPlaceHolder("RandomLikedSongResults", 21),
        new chavah.songPlaceHolder("RandomLikedSongResults", 22),

                   new chavah.songPlaceHolder("RandomLikedSongResults", 23),
                   new chavah.songPlaceHolder("RandomLikedSongResults", 24),
                   new chavah.songPlaceHolder("RandomLikedSongResults", 25),
                   new chavah.songPlaceHolder("RandomLikedSongResults", 26),
                   new chavah.songPlaceHolder("RandomLikedSongResults", 27),
                   new chavah.songPlaceHolder("RandomLikedSongResults", 28),
                   new chavah.songPlaceHolder("RandomLikedSongResults", 29),
                   new chavah.songPlaceHolder("RandomLikedSongResults", 30)
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
	    new chavah.songPlaceHolder("TopSongResults", 10),
        new chavah.songPlaceHolder("TopSongResults", 11),
        new chavah.songPlaceHolder("TopSongResults", 12),
        new chavah.songPlaceHolder("TopSongResults", 13),
        new chavah.songPlaceHolder("TopSongResults", 14),
        new chavah.songPlaceHolder("TopSongResults", 15),
        new chavah.songPlaceHolder("TopSongResults", 16),
        new chavah.songPlaceHolder("TopSongResults", 17),
        new chavah.songPlaceHolder("TopSongResults", 18),
	    new chavah.songPlaceHolder("TopSongResults", 19),
	    new chavah.songPlaceHolder("TopSongResults", 20),
        new chavah.songPlaceHolder("TopSongResults", 21),
        new chavah.songPlaceHolder("TopSongResults", 22),
        new chavah.songPlaceHolder("TopSongResults", 23),
        new chavah.songPlaceHolder("TopSongResults", 24),
        new chavah.songPlaceHolder("TopSongResults", 25),
        new chavah.songPlaceHolder("TopSongResults", 26),
        new chavah.songPlaceHolder("TopSongResults", 27),
        new chavah.songPlaceHolder("TopSongResults", 28),
        new chavah.songPlaceHolder("TopSongResults", 29),
        new chavah.songPlaceHolder("TopSongResults", 30)
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
         new chavah.songPlaceHolder(),
            new chavah.songPlaceHolder(),
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
        var eleventhSong = self.recentSongs()[10].song();
        var secondteenthSong = self.recentSongs()[11].song();
        var thirdteenthSong = self.recentSongs()[12].song();
        var fourthteenthSong = self.recentSongs()[13].song();
        var fifthteenthSong = self.recentSongs()[14].song();
        var sixthteenthSong = self.recentSongs()[15].song();
        var seventeenthSong = self.recentSongs()[16].song();
        var eighteenthSong = self.recentSongs()[17].song();
        var nineteenthSong = self.recentSongs()[18].song();

        setTimeout(function () { self.recentSongs()[0].enqueuedSong(song); }, 1000);
        setTimeout(function () { self.recentSongs()[1].enqueuedSong(firstSong) }, 1500);
        setTimeout(function () { self.recentSongs()[2].enqueuedSong(secondSong) }, 1750);
        setTimeout(function () { self.recentSongs()[3].enqueuedSong(thirdSong) }, 1750);
        setTimeout(function () { self.recentSongs()[4].enqueuedSong(fourthSong) }, 2000);
        setTimeout(function () { self.recentSongs()[5].enqueuedSong(fifthSong) }, 2250);
        setTimeout(function () { self.recentSongs()[6].enqueuedSong(sixthSong) }, 2500);
        setTimeout(function () { self.recentSongs()[7].enqueuedSong(seventhSong) }, 1750);
        setTimeout(function () { self.recentSongs()[8].enqueuedSong(eighthSong) }, 1750);
        setTimeout(function () { self.recentSongs()[9].enqueuedSong(ninethSong) }, 1750);
        setTimeout(function () { self.recentSongs()[10].enqueuedSong(tenthSong); }, 1000);
        setTimeout(function () { self.recentSongs()[11].enqueuedSong(eleventhSong) }, 1500);
        setTimeout(function () { self.recentSongs()[12].enqueuedSong(secondteenthSong) }, 1750);
        setTimeout(function () { self.recentSongs()[13].enqueuedSong(thirdteenthSong) }, 1750);
        setTimeout(function () { self.recentSongs()[14].enqueuedSong(fourthteenthSong) }, 2000);
        setTimeout(function () { self.recentSongs()[15].enqueuedSong(fifthteenthSong) }, 2250);
        setTimeout(function () { self.recentSongs()[16].enqueuedSong(sixthteenthSong) }, 2500);
        setTimeout(function () { self.recentSongs()[17].enqueuedSong(seventeenthSong) }, 1750);
        setTimeout(function () { self.recentSongs()[18].enqueuedSong(eighteenthSong) }, 1750);
        setTimeout(function () { self.recentSongs()[19].enqueuedSong(nineteenthSong) }, 1750);


    }

    this.toggleSongRequestDialogVisible = function () {
        self.songRequestVisible(!self.songRequestVisible());
    }

    //fadeOut: function(element) {
    //    if (element.nodeType === 1) {
    //        $(element).fadeOut("slow", function() { $(element).remove(); });
    //    }
    //},

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
        PubSub.publish("JsonGet", { url: "/GetSongMatches", data: { searchText: requestText }, responseMessage: "SongMatchesResults" });
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
            PubSub.publish("JsonGet", {
                url: "/GetSongForClient",
                data: { clientId: chavah.localstorage.getOrCreateUserId() },
                responseMessage: "SongFetched"
            });
        }
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