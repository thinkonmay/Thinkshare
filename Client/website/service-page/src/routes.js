module.exports = {
	//  ╦ ╦╔═╗╔╗ ╔═╗╔═╗╔═╗╔═╗╔═╗
	//  ║║║║╣ ╠╩╗╠═╝╠═╣║ ╦║╣ ╚═╗
	//  ╚╩╝╚═╝╚═╝╩  ╩ ╩╚═╝╚═╝╚═╝
	"GET /": {action: "view-login"},
	"GET /login": {action: "view-login"},
	"GET /register": {action: "view-register"},
	"GET /dashboard": {action: "view-dashboard"},
	"GET /setting": {action: "view-setting"},
	"GET /logout": {action: "logout"},
	"GET /404": {action: "404"},
	"GET /500": {action: "500"},
	//  ╔╦╗╦╔═╗╔═╗  ╦═╗╔═╗╔╦╗╦╦═╗╔═╗╔═╗╔╦╗╔═╗   ┬   ╔╦╗╔═╗╦ ╦╔╗╔╦  ╔═╗╔═╗╔╦╗╔═╗
	//  ║║║║╚═╗║    ╠╦╝║╣  ║║║╠╦╝║╣ ║   ║ ╚═╗  ┌┼─   ║║║ ║║║║║║║║  ║ ║╠═╣ ║║╚═╗
	//  ╩ ╩╩╚═╝╚═╝  ╩╚═╚═╝═╩╝╩╩╚═╚═╝╚═╝ ╩ ╚═╝  └┘   ═╩╝╚═╝╚╩╝╝╚╝╩═╝╚═╝╩ ╩═╩╝╚═╝
	"/": "/dashboard"
	// "/logout": "/api/v1/account/logout"
}
