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
				videoCodec: Number(query.cap.videoCodec),
				audioCodec: Number(query.cap.audioCodec),
				mode: Number(query.cap.mode),
				screenWidth: Number(query.cap.screenWidth),
				screenHeight: Number(query.cap.screenHeight)
			}
		}
	} catch (error) {
		res.status(500).render("error", {
			status: 500,
			stack: process.env.NODE_ENV == "development" ? error.stack : "Page error"
		})
	}
}


