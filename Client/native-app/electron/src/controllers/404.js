module.exports = (req, res, next) => {
	res.status(404).render("404", {
		status: 404,
		stack: "Page not found"
	})
}