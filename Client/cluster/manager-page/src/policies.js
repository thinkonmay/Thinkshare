module.exports = {
	"*": true,
	"/dashboard": "is-logged-in",
	"/setting": "is-logged-in",
	"/initialize": "is-logged-in",
	"/register-cluster": "is-logged-in"
}