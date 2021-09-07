module.exports = (req, res, next) => {
	if (req.cookies.token)
		res.render("dashboard-admin")
	else
		res.redirect("/login")
}
