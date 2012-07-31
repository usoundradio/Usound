chavah.songRequestManager = function () {
    var self = this;
    this.pendingRequestedSongId = 0;
    this.hasPlayedRequestAnnouncement = false;

    this.hasPendingRequest = function() {
        return this.pendingRequestedSongId > 0;
    }

    this.requestSong = function (song) {
        PubSub.publish("JsonGet", { url: "/GetSongForSongRequest", data: { clientId: chavah.localstorage.getOrCreateUserId(), songId: song.id } });
        PubSub.publish("Pause");
        this.pendingRequestedSongId = song.id;
        this.hasPlayedRequestAnnouncement = false;
        PubSub.publish("NextSong");
    }

    this.playSongRequest = function () {
        if (!this.hasPendingRequest()) {
            throw "There was no pending song request.";
        } 
        
    
        else {
            PubSub.publish("PlaySongById", { songId: self.pendingRequestedSongId });
            this.pendingRequestedSongId = 0;
            this.hasPlayedRequestAnnouncement = false;
        }
    }
    

    // When we get the results back for a pending song request, check if there's 
    PubSub.subscribe("PendingSongRequestResults", function (message, id) {
        if (!self.hasPendingRequest() && id > 0) {
            self.pendingRequestedSongId = id;
            self.hasPlayedRequestAnnouncement = false;
        }
    });

    chavah.dataServices.recurringFetch("/GetRequestedSongId", { clientId: chavah.localstorage.getOrCreateUserId() }, 10000, 30000, "PendingSongRequestResults");
}