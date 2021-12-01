module.exports = (req, res, next) => {
	if (req.cookies.token) return next()

	return res.redirect("/login")
}