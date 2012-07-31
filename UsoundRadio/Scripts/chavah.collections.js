chavah.observableArrayFrom = function (address, args, dataTransformer, refreshTime, callback) {
    var array = ko.observableArray();

    var fetch = function () {
        $.getJSON(address, args).success(function (results) {
            var transformedResults = dataTransformer ? results.map(function (r) { return dataTransformer(r) }) : results;

            if (callback) {
                callback(transformedResults);
            }
            else {
                array(transformedResults);
            }

            if (refreshTime) {
                setTimeout(fetch, refreshTime);
            }
        });
    }
    
    fetch();
    return array;
}

chavah.observableSongsFrom = function (address, args, refreshTime, callback) {
    return chavah.observableArrayFrom(address, args, function (s) { return new Song(s) }, refreshTime, callback);
}