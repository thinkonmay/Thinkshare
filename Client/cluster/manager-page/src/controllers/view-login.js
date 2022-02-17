module.exports = (req, res, next) => {
	if (!req.cookies.token) res.render("login")
	else res.redirect("dashboard")
}