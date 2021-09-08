module.exports = (req, res, next) => {
	console.log(req.query)
	res.send(req.query.html)
}
