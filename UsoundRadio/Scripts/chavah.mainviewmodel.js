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
		new chavah.songPlaceHolder("TrendingSongResults", 2)

    ]);

	this.myLikedSongs = ko.observableArray([
        new chavah.songPlaceHolder("RandomLikedSongResults", 0),
        new chavah.songPlaceHolder("RandomLikedSongResults", 1),
        new chavah.songPlaceHolder("RandomLikedSongResults", 2)
    ]);

	this.topSongs = ko.observableArray([
        new chavah.songPlaceHolder("TopSongResults", 0),
        new chavah.songPlaceHolder("TopSongResults", 1),
        new chavah.songPlaceHolder("TopSongResults", 2)
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
	    setTimeout(function () { self.recentSongs()[0].enqueuedSong(song); }, 1000);
	    setTimeout(function () { self.recentSongs()[1].enqueuedSong(firstSong) }, 1500);
	    setTimeout(function () { self.recentSongs()[2].enqueuedSong(secondSong) }, 1750);
	    setTimeout(function () { self.recentSongs()[3].enqueuedSong(secondSong) }, 1750);
	    setTimeout(function () { self.recentSongs()[4].enqueuedSong(secondSong) }, 1750);
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

	this.nextSong = function () {
	    PubSub.publish("NextSong");
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
	var songRequestTextThrottled =  ko.computed(function() {
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
	chavah.dataServices.recurringFetch("/GetRandomLikedSongs", { clientId: this.userId, count: 3 }, 0, 15000, "RandomLikedSongResults");
	chavah.dataServices.recurringFetch("/GetTrendingSongs", { count: 3 }, 300, 3000, "TrendingSongResults");
	chavah.dataServices.recurringFetch("/GetTopSongs", { count: 3 }, 600, 60000, "TopSongResults");
}