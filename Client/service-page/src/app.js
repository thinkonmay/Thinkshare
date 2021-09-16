require("dotenv").config()
const express = require("express")
const path = require("path")
const cookieParser = require("cookie-parser")
const bodyParser = require("body-parser")
const logger = require("morgan")

const routes = require("./routes")
const policies = require("./policies")

const app = express()

const cors = require('cors')
app.use(cors())


// view engine setup
app.set("views", path.join(__dirname, "views"))
app.set("view engine", "ejs")

app.use(logger("dev"))
app.use(express.json())
app.use(express.urlencoded({extended: false}))
app.use(cookieParser())
app.use(bodyParser.urlencoded({extended: false}))
app.use(bodyParser.json())
app.use(express.static(path.join(__dirname, "../public")))

//setup policies
for (const policyKey in policies) {
	const query = policies[policyKey]
	let path = "/"
	let middleware = null
	if (policyKey != "*") path = policyKey
	if (query.length > 0) {
		middleware = require("./policies/" + query)
	}
	if (middleware) app.use(path, middleware)
}

// setup routes
for (const routeKey in routes) {
	const query = routes[routeKey]
	const tmp = routeKey.split(" ")
	if (typeof query == "object") {
		const method = tmp[0]
		const route = tmp[1]
		app[method.toLowerCase()](
			route,
			require(path.join(__dirname, "controllers", query.action))
		)
	} else {
		app.get(routeKey, (req, res) => res.redirect(query))
	}
}

// error handler
app.get("*", (req, res) => {
	res.status(404).render("error", {
		status: 404,
		stack: "Page not found"
	})
})

module.exports = app
