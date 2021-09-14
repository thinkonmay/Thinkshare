import * as API from "../../public/javascripts/util/api.js"

module.exports = async (req, res, next) => {
	try {
		const data = await API.initializeSession();
		res.send(html);
		
	} catch (error) {
		res.status(500).render("error", {
			status: 500,
			stack: process.env.NODE_ENV == "development" ? error.stack : "Page error"
		})
	}
}
