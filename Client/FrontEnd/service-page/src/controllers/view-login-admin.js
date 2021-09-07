const path = require("path")

module.exports = (req, res, next) => {
	if (!req.cookies.token)
		res.render('login-admin')
	else
		res.redirect('/dashboard')
}
