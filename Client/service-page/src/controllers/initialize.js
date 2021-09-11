import * as API from "../util/api.js"


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
		res.send(API.initializeSession(body,atob(req.cookies.token)))
	} catch (error) {
		res.status(500).render("error", {
			status: 500,
			stack: process.env.NODE_ENV == "development" ? error.stack : "Page error"
		})
	}
}
