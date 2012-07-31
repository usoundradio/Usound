ko.bindingHandlers.fadeInOnSongChange = {
    init: function (element, valueAccessor, allBindingsAccessor, viewModel) {
        // This will be called once when the binding is first applied to an element,
        // and again whenever the associated observable changes value.
        // Update the DOM element based on the supplied values here.

        var data = ko.dataFor(element);
        if (data.enqueuedSong && ko.isObservable(data.enqueuedSong)) {
            data.enqueuedSong.subscribe(function (newSong) {
                $(element).fadeOut(function () {
                    data.updateSong(newSong);
                    $(element).hide().fadeIn();
                });
            });
        }
    }
};