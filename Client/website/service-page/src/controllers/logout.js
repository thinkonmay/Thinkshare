module.exports = (req, res, next) => {
	res.clearCookie("token")
	res.redirect("/")
}
