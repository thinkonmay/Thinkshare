module.exports = {
	//  ╦ ╦╔═╗╔╗ ╔═╗╔═╗╔═╗╔═╗╔═╗
	//  ║║║║╣ ╠╩╗╠═╝╠═╣║ ╦║╣ ╚═╗
	//  ╚╩╝╚═╝╚═╝╩  ╩ ╩╚═╝╚═╝╚═╝
	"GET /login": {action: "view-login"},
	"GET /login-admin": {action: "view-login-admin"},
	"GET /register": {action: "view-register"},
	"GET /dashboard": {action: "view-dashboard"},
	"GET /dashboard-admin": {action: "view-dashboard-admin"},
	"GET /logout": {action: "logout"},
	"GET /initialize": {action: "initialize"},
	"GET /reconnect": {action: "reconnect"},
	//  ╔╦╗╦╔═╗╔═╗  ╦═╗╔═╗╔╦╗╦╦═╗╔═╗╔═╗╔╦╗╔═╗   ┬   ╔╦╗╔═╗╦ ╦╔╗╔╦  ╔═╗╔═╗╔╦╗╔═╗
	//  ║║║║╚═╗║    ╠╦╝║╣  ║║║╠╦╝║╣ ║   ║ ╚═╗  ┌┼─   ║║║ ║║║║║║║║  ║ ║╠═╣ ║║╚═╗
	//  ╩ ╩╩╚═╝╚═╝  ╩╚═╚═╝═╩╝╩╩╚═╚═╝╚═╝ ╩ ╚═╝  └┘   ═╩╝╚═╝╚╩╝╝╚╝╩═╝╚═╝╩ ╩═╩╝╚═╝
	"/": "/dashboard"
	// "/logout": "/api/v1/account/logout"
}
