module.exports = (req, res, next) => {
	res.status(500).render("500", {
		status: 500,
		stack: "An error occured"
	})
}