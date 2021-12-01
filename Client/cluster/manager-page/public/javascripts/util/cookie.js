export function getCookie(cname) {
	let name = cname + "="
	let decodedCookie = decodeURIComponent(document.cookie)
	let ca = decodedCookie.split(";")
	for (let i = 0; i < ca.length; i++) {
		let c = ca[i]
		while (c.charAt(0) == " ") {
			c = c.substring(1)
		}
		if (c.indexOf(name) == 0) {
			return atob(c.substring(name.length, c.length))
		}
	}
	return ""
}

export function setCookie(name, value, milis) {
	var expires = ""
	if (milis) {
		var date = new Date(Date.now() + milis)
		expires = "; expires=" + date.toUTCString()
	}
	document.cookie = name + "=" + (btoa(value) || "") + expires + "; path=/"
}

export function deleteCookie(name, path, domain) {
    if (getCookie(name)) document.cookie = name + '=' +
        ((path) ? ';path=' + path : '') +
        ((domain) ? ';domain=' + domain : '') +
        ';expires=Thu, 01-Jan-1970 00:00:01 GMT';
}