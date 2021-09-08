module.exports = (req, res, next) => {
	res.clearCookie("token")
	req.logout()
	res.redirect("/")
}
