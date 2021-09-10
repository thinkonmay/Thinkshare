module.exports = {
	"*": true,
	"/dashboard": "is-logged-in",
	"/dashboard-admin": "is-logged-in",
	"/initialize": "is-logged-in"
}
