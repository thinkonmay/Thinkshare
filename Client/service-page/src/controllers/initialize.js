const fetch = require("node-fetch")

module.exports = async (req, res, next) => {
	try {
		const query = Object.assign(
			{
				cap: {}
			},
			req.query
		)
		const data = await fetch(
			process.env.THINKMAY_HOST + "/session/initialize",
			{
				headers: {
					Authorization: "Bearer " + atob(req.cookies.token),
					"Content-Type": "application/json"
				},
				method: "POST",
				body: JSON.stringify({
					slaveId: Number(query.slaveId),
					cap: {
						videoCodec: Number(query.cap.videoCodec),
						audioCodec: Number(query.cap.audioCodec),
						mode: Number(query.cap.mode),
						screenWidth: Number(query.cap.screenWidth),
						screenHeight: Number(query.cap.screenHeight)
					}
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
