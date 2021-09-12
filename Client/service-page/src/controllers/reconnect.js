const fetch = require("node-fetch")

module.exports = async (req, res, next) => {
	try {
		const data = await fetch(
			process.env.THINKMAY_HOST + "/session/Reconnect", {
				headers: {
					Authorization: "Bearer " + atob(req.cookies.token),
					"Content-Type": "application/json"
				},
				method: "POST",
				body: JSON.stringify({
					SlaveID: Number(req.query.SlaveID)
				})
			}
		)
		const html = await data.text()
		res.send(html)
	} catch (error) {
		res.status(500).render("error", {
			status: 500,
			stack: process.env.NODE_ENV == "development" ? error.stack : "Page error"
		})
	}
}
