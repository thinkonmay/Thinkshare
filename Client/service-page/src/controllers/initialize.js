const fetch = require("node-fetch")

module.exports = async (req, res, next) => {
	try {
		const query = Object.assign({
				cap: {}
			},
			req.query
		)
		const body = {
			slaveId: Number(query.SlaveID),
			cap: {
				videoCodec: 1 || Number(query.cap.videoCodec),
				audioCodec: 4 || Number(query.cap.audioCodec),
				mode: 4 || Number(query.cap.mode),
				screenWidth: 2560 || Number(query.cap.screenWidth),
				screenHeight: 1440 || Number(query.cap.screenHeight)
			}
		}
		const data = await fetch(
			process.env.THINKMAY_HOST + "/session/initialize", {
				headers: {
					Authorization: "Bearer " + atob(req.cookies.token),
					"Content-Type": "application/json"
				},
				method: "POST",
				body: JSON.stringify(body)
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
