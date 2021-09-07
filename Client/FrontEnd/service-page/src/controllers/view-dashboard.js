module.exports = (req, res, next) => {
	if (req.cookies.token)
		res.render("dashboard")
	else
		res.redirect("/login")
}
