function Song(dataContract) {
    var song = this;
    var unranked = 0;
    var liked = 1;
    var disliked = 2;
    this.name = dataContract.Name;
    this.description = dataContract.Description;
    this.number = dataContract.Number;
    this.album = dataContract.Album;
    this.artist = dataContract.Artist;
    this.genre = dataContract.Genre;
    this.country = dataContract.Country;
    this.albumArtUri = dataContract.AlbumArtUri;
    this.uri = dataContract.Uri;
    this.likeStatus = ko.observable(dataContract.SongLike);
    this.communityRank = ko.observable(dataContract.CommunityRank);
    this.id = dataContract.Id;
    this.isLiked = ko.computed(function () {
        return song.likeStatus() === liked;
    });
    this.isDisliked = ko.computed(function () {
        return song.likeStatus() === disliked;
    });
    this.thumbUpImage = ko.computed(function () {
        return chavah.constants.contentDirectory + "/images/up" + (song.isLiked() ? "Checked.png" : ".png");
    });
    this.thumbDownImage = ko.computed(function () {
        return chavah.constants.contentDirectory + "/images/down" + (song.isDisliked() ? "Checked.png" : ".png");
    });
    this.communityRankColor = ko.computed(function () {
        return chavah.converters.rankToColor(song.communityRank());
    });
    this.likeHoverText = ko.computed(function () {
        return song.likeStatus() === liked ? "You've already liked this song. Chavah is playing more songs like it" : "I like this song, play more songs like it";
    });
    this.dislikeHoverText = ko.computed(function () {
        return song.likeStatus() === disliked ? "You already disliked this song. Chavah will keep it on the back shelf, and rarely play it for you" : "I don't like this song, don't play it again";
    });

    this.play = function () {
        // Play by ID causes fetch to the server to get the song.
        // That fetch is necessary to grab information like the like status of a song.
        PubSub.publish("PlaySongById", { songId: song.id });
    }

    this.playSongFromArtist = function () {
        PubSub.publish("Pause");
        PubSub.publish("Get", {
            url: "/GetSongByArtist",
            data: { clientId: chavah.localstorage.getOrCreateUserId(), artist: song.artist },
            responseMessage: "SongFetched"
        });
    }

    this.playSongFromAlbum = function () {
        PubSub.publish("Pause");
        PubSub.publish("Get", {
            url: "/GetSongByAlbum",
            data: { clientId: chavah.localstorage.getOrCreateUserId(), album: song.album },
            responseMessage: "SongFetched"
        });
    }

    //this.songGrid = function () {
    //    PubSub.publish("Get", {
    //        url: "/GetSongMatches?searchText=",
    //        data: { album: song.album }
    //  });
    //}

    this.dislike = function () {
        if (!song.isDisliked()) {
            var incrementAmount = song.isLiked() ? -2 : -1;
            likeOrDislike(incrementAmount, "/DislikeById");
            PubSub.publish("NextSong");
        }
    }

    this.like = function () {
        if (!song.isLiked()) {
            var incrementAmount = song.isDisliked() ? 2 : 1;
            likeOrDislike(incrementAmount, "/LikeById");
        }
    }

    function likeOrDislike(increment, actionName) {
        $.getJSON("/Songs" + actionName, {
            clientId: chavah.viewModel.userId,
            songId: song.id
        });
        song.communityRank(song.communityRank() + increment);
        song.likeStatus(increment > 0 ? liked : disliked);
    }
}