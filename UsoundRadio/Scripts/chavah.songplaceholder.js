chavah.songPlaceHolder = function(resultsMessage, resultsIndex) {
	var self = this;
	this.song = ko.observable();
	this.hasSong = ko.computed(function() { return self.song() != null; });
    this.albumArtUri = ko.computed(function () {
	    var song = self.song();
		return song ? song.albumArtUri : null;
	});

	this.communityRank = ko.computed(function () {
		return self.song() ? self.song().communityRank : 0;
	});

	this.name = ko.computed(function () {
		return self.song() ? self.song().name : "";
    });

	this.artist = ko.computed(function() {
	    return self.song() ? self.song().artist : "";
	});

	this.album = ko.computed(function () {
	    return self.song() ? self.song().album : "";
	});

	this.play = function () {
	    var song = this.song();
	    if (song) {
	        song.play();
	    }
	};
    this.updateSong = function (newSong) {
	    if (!self.song() || self.song().id !== newSong.id && !self.preventSongSwitch) {
	        self.song(newSong);
	    }
	};
    this.enqueuedSong = ko.observable();
	this.preventSongSwitch = false;
	this.mouseEnter = function () { self.preventSongSwitch = true; };
    this.mouseLeave = function () { self.preventSongSwitch = false; };
    if (resultsMessage) {
	    PubSub.subscribe(resultsMessage, function (message, args) {
	        var songDataContract = args[resultsIndex];
	        if (songDataContract != null) {
	            var song = new Song(songDataContract);
	            if (!self.enqueuedSong() || self.enqueuedSong().id !== song.id) {
	                setTimeout(function () { self.enqueuedSong(song); }, 150 * resultsIndex);
	            }
	        }
	    });
	}
}