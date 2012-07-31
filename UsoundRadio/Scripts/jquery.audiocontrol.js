(function ($) {
    $.fn.audiocontrol = function (options) {
        /********************
        ** Provide default settings and a mechanism for overriding with options object
        ********************/
        var defaults = {
            audioButtonClass: "audioButton",  			//class applied to DOM elements used as buttons
            flashAudioPlayerPath: "swf/player.swf", 	//path to flash version of audio player
            flashObjectID: "audioPlayer", 			//id of flash object
            loadingClass: "loading", 				//class applied to buttons while media loads
            playerContainer: "player", 				//id of DOM element that contains audio element (or is replaced by flash)
            playingClass: "playing", 				//class applied to buttons while media plays
            swfobjectPath: "js/swfobject.js",			//path to swfobject.js used to embed flash movie in page
            audioSelector: "#audioElement"
        };

        var currentTrack; 						//reference to the current(or last played) track
        var isPlaying = false; 					//track play state of track
        var forceFlash = false; 					//force player into flash mode for testing
        var audio; 								//audio element or id of embedded flash player
        var mediaState = ko.observable();
        var playOnFlashLoad;
        var isFirstPlay = true;

        //vars for caching jQuery wrapped sets
        var $player;
        var $buttons;

        /********************
        ** The play, resume and pause methods differ depending on the playback mode (html5 or flash) of the player.
        ** Instead of repeatedly testing forceFlash every time one of those methods is called, I test once
        ** in init() and use playTrack, resumeTrack and pauseTrack to hold references to the functions that are properties
        ** of the htmlFunctions or flashFunctions object.
        **
        ** I create a new instance of the Audio element for every new track in html5 mode. This is because Audio events (canplay, ended, etc)
        ** don't fire consistently across all browsers when resetting the src attribute of a previously created Audio element.
        ********************/
        var playTrack;
        var resumeTrack;
        var pauseTrack;
        var html5AudioElement;
        /********************
        ** Some actions such as updating UI and vars are necessary on change of track state regardless of playback mode.
        ** These common functions are called from both htmlFunctions and flashFunctions.
        ********************/
        var common = {
            playTrack: function () {
                currentTrack = defaults.currentMedia();
                isPlaying = true;
                mediaState("playing");
            },
            pauseTrack: function () {
                isPlaying = false;
                mediaState("paused");
            },
            resumeTrack: function () {
                isPlaying = true;
                mediaState("playing");
            }
        }

        var htmlFunctions = {
            playTrack: function () {
                currentTrack = defaults.currentMedia();
                if (audio) {
                    audio.pause();
                }

                audio = html5AudioElement;
                if (!audio) {
                    audio = $(defaults.audioSelector)[0];
                    addListeners(audio);
                    audio.addEventListener("pause", function () { mediaState("paused") });
                    audio.addEventListener("playing", function () { mediaState("playing") });
                    html5AudioElement = audio;
                }

                // Yes, duplicated. https://groups.google.com/group/jplayer/browse_thread/thread/a91bc1e0dfece085
                audio.src = currentTrack;
                audio.src = currentTrack;
                audio.load();

                // iOS has some painful policies when it comes to playing HTML5 audio:
                // Audio can't play until the page has been activated.
                // If we detect this condition, show the play button (thus forcing the user to activate the UI before music starts playing.)
                var isIOS = navigator.userAgent.indexOf("iPad") != -1 || navigator.userAgent.indexOf("iPhone") != -1;
                if (isIOS && isFirstPlay) {
                    isFirstPlay = false;
                    mediaState("paused");
                }
                else {
                    audio.play();
                }
            },
            pauseTrack: function () {
                audio.pause();
                common.pauseTrack();
            },
            resumeTrack: function () {
                audio.play();
                common.resumeTrack();
            }
        };

        var flashFunctions = {
            playTrack: function () {
                audio = document.getElementById(defaults.flashObjectID);
                if (audio) {
                    common.playTrack();
                    addListeners(window);
                    audio.playFlash(currentTrack);
                }
                else {
                    playOnFlashLoad = true;
                }
            },
            pauseTrack: function () {
                audio.pauseFlash();
                common.pauseTrack();
            },
            resumeTrack: function () {
                audio.playFlash();
                common.resumeTrack();
            }
        };

        if (options) {
            $.extend(defaults, options);
        }

        $player = $("#" + defaults.playerContainer);
        $buttons = $("." + defaults.audioButtonClass);

        playTrack = htmlFunctions.playTrack;
        resumeTrack = htmlFunctions.resumeTrack;
        pauseTrack = htmlFunctions.pauseTrack;

        defaults.currentMedia.subscribe(function () {
            updateTrackState();
        }); 

        if (forceFlash || !document.createElement('audio').canPlayType) {
            useFlash();
        }

        var canPlayNativeMp3 = false;
        try {
            canPlayNativeMp3 = canPlay("MP3");
        }
        catch (err) {
        }

        if (!canPlayNativeMp3) {
            useFlash();
        }

        function useFlash() {
            playTrack = flashFunctions.playTrack;
            resumeTrack = flashFunctions.resumeTrack;
            pauseTrack = flashFunctions.pauseTrack;
            $.getScript(defaults.swfobjectPath, loadFlash);
        }

        function loadFlash() {
            var flashLoadedCallback = function (success, id, ref) {
                if (playOnFlashLoad) {
                    setTimeout(playTrack, 250);
                }
            }

            swfobject.embedSWF(defaults.flashAudioPlayerPath, defaults.playerContainer, "2px", "2px", "9.0.0", "swf/expressInstall.swf", false, false, { id: defaults.flashObjectID }, flashLoadedCallback);
        }

        function updateTrackState() {

            if (!audio || (currentTrack !== defaults.currentMedia())) {
                playTrack();
            }
            else if (!isPlaying) {
                resumeTrack();
            }
            else {
                pauseTrack();
            }
        }

        /********************
        ** These methods exist because I must add/remove listeners every time a new Audio element is created.
        ** It's necessary to add event listeners to different elements depending on playback mode.
        ** Flash dispatches events from the window and html5 from the audio element so we bind to those
        ** elements respectively. It was primarily an issue in IE8 and below because the audio events being
        ** dispatched from the Flash are custom events. I need to investigate further.
        ********************/
        function addListeners(elem) {
            $(elem).bind({
                "canplay": onLoaded,
                "error": onError,
                "ended": onEnded
            });
        }

        function removeListeners(elem) {
            $(elem).unbind({
                "canplay": onLoaded,
                "error": onError,
                "ended": onEnded
            });
        }

        /********************
        ** event handlers
        ********************/

        function onLoaded() {
            audio.play();
            $buttons.removeClass(defaults.loadingClass);
        }

        function onError(e) {
            console.log("Error loading audio", e);
            mediaState("error");
            removeListeners(audio);
            PubSub.publish("AudioError", e);
        }

        function onEnded() {
            isPlaying = false;
            mediaState("ended");
            currentTrack = "";
            //removeListeners(audio);
        }

        /********************
        ** canPlay is a utility method used to test if we have browser support for an audio codec
        ********************/

        function canPlay(type) {
            var fmt;
            switch (type) {
                case "OGG":
                    fmt = 'audio/ogg; codecs="vorbis"';
                    break;
                case "MP3":
                    fmt = 'audio/mpeg';
                    break;
            }
            return document.createElement('audio').canPlayType(fmt).match(/maybe|probably/i) ? true : false;
        }

        var player = {
            mediaState: mediaState,
            playPause: function () {
                if (mediaState() === "playing") {
                    pauseTrack();
                } else if (mediaState() === "paused") {
                    resumeTrack();
                } else {
                    playTrack();
                }
            },
            play: function () {
                playTrack();
            },
            pause: function () {
                pauseTrack();
            }
        };

        return player;
    }
})(jQuery);