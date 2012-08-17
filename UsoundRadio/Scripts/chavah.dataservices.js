chavah.dataServices = {

    start: function() {
        PubSub.subscribe("JsonGet", function (message, args) {
            $.ajax({
                url: "/Songs" + args.url,
                data: args.data,
                dataType: 'json',
                cache: false
            })
            .success(function (results) {
                if (args.responseMessage) {
                    PubSub.publish(args.responseMessage, results);
                }
            })
            .fail(function(jqXHR, textStatus, errorThrown) {
                console.log("AJAX call failed.", message, args, jqXHR, textStatus, errorThrown);
            });
        });

        PubSub.subscribe("Post", function (message, args) {
            $.post("/Songs" + args.url, args.data).success(function (result) {
                if (args.responseMessage) {
                    PubSub.publish(args.responseMessage, results);
                }
            });
        });

        // When the the PlaySongById message is sent, we do an AJAX call to fetch
        // the song, then publish the SongFetched message when done.
        PubSub.subscribe("PlaySongById", function (message, args) {
            PubSub.publish("Pause");
            var fetchArgs = { clientId: chavah.localstorage.getOrCreateUserId(), songId: args.songId };
            PubSub.publish("JsonGet", { url: "/GetSongById", responseMessage: "SongFetched", data: fetchArgs });
        });

        // When a song is fetched, queue up playing that song.
        PubSub.subscribe("SongFetched", function (message, args) {
            var song = new Song(args);
            PubSub.publish("PlaySong", { song: song });
        });
    },

	recurringFetch: function (url, args, initialDelay, repeatDelay, responseMessage) {

	    var fetch = function () {
	        $.ajax({
	            url: "/Songs" + url,
	            data: args,
	            dataType: 'json',
                cache: false
            }).success(function (results) {
				PubSub.publish(responseMessage, results);
			}).complete(function () {
				setTimeout(fetch, repeatDelay);
			});
		}

		setTimeout(fetch, initialDelay);
	}
}