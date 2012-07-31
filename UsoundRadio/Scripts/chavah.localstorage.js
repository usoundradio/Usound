chavah.localstorage = {
	getOrCreateUserId: function() {
		var cookieValue = chavah.localstorage.getCookie("userId");
		if (cookieValue == null) {
			var id = guid();
			chavah.localstorage.createCookie("userId", id, 999);
			return id;
		}
		return cookieValue;
	},

	createCookie: function (cookieName, value, expirationInDays) {
		var exdate = new Date();
		exdate.setDate(exdate.getDate() + expirationInDays);
		var c_value = escape(value) + ((expirationInDays == null) ? "" : "; expires = " + exdate.toUTCString());
		document.cookie = cookieName + "=" + c_value;
	},

	getCookie: function (cookieName)
	{
		var cookies = document.cookie.split(";");
		for (var i=0; i < cookies.length; i++)
		{
			var x = cookies[i].substr(0, cookies[i].indexOf("="));
			var y = cookies[i].substr(cookies[i].indexOf("=") + 1);
			x = x.replace(/^\s+|\s+$/g, "");
			if (x==cookieName) {
				return unescape(y);
			}
		}
		return null;
	}
}