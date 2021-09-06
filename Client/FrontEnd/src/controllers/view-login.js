const path = require("path")

module.exports = (req, res, next) => {
	// console.log(req.cookies)
	if (!req.cookies.token)
		res.render('login')
	else
		res.redirect('/dashboard')
}
