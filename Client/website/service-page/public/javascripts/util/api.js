import { getCookie } from "./cookie.js"

let currentURL = document.URL
let subdomain = currentURL.slice(0, 28)
const host = "https://host.thinkmay.net"

// local api
export const Dashboard = "/dashboard"
export const Initialize = "/initialize"
export const Reconnect = "/reconnect"


// thinkmay api
// Account API
const Login = `${host}/Account/Login`
const Register = `${host}/Account/Register`
const Token = `${host}/Account/ExchangeToken`
const Infor = `${host}/Account/Infor`
const Session = `${host}/Account/History`


const Setting = `${host}/Setting`

// Session API
const InitializeSession = `${host}/Session/Initialize`
const TerminateSession = `${host}/Session/Terminate`
const DisconnectSession = `${host}/Session/Disconnect`
const ReconnectSession = `${host}/Session/Reconnect`

const SessionInfor = `${host}/Session/Setting`

export const UserHub = `wss://host.thinkmay.net/Hub/User`

// User API
const FetchSlave = `${host}/Fetch/Node`
const FetchSession = `${host}/Fetch/Session`


export const genHeaders = () => {
	const token = getCookie("token")
	return Object.assign(
		{
			"Content-Type": "application/json"
		},
		token
			? {
				Authorization: `Bearer ${token}`
			}
			: {}
	)
}

export const login = body => {
	return fetch(Login, {
		method: "POST",
		headers: genHeaders(),
		body: JSON.stringify({
			userName: body.username,
			password: body.password
		})
	})
}

export const register = body => {
	return fetch(Register, {
		method: "POST",
		headers: genHeaders(),
		body: JSON.stringify({
			userName: body.username,
			password: body.password,
			email: body.email,
			fullName: body.fullname,
			dateOfBirth: body.dob,
			jobs: body.jobs,
			phoneNumber: body.phonenumber
		})
	})
}














export const tokenExchange = body => {
	return fetch(Token, {
		method: "POST",
		headers: genHeaders(),
		body: JSON.stringify({
			token: body.token,
			Validator: body.Validator
		})
	})
}



export const fetchSlave = () => {
	return fetch(FetchSlave, {
		method: "GET",
		headers: genHeaders()
	}, function (error) {
		if (401 == error.response.status) {
			window.location.replace(API.Login)
		} else {
			return Promise.reject(error);
		}
	})
}

export const fetchSession = () => {
	return fetch(FetchSession, {
		method: "GET",
		headers: genHeaders()
	}, function (error) {
		if (401 == error.response.status) {
			window.location.replace(API.Login)
		} else {
			return Promise.reject(error);
		}
	})
}

export const getSession = () => {
	return fetch(Session, {
		method: "GET",
		headers: genHeaders()
	}, function (error) {
		if (401 == error.response.status) {
			window.location.replace(API.Login)
		} else {
			return Promise.reject(error);
		}
	})
}

















export const terminateSession = SlaveID => {
	return fetch(TerminateSession + "?SlaveID=" + SlaveID, {
		method: "DELETE",
		headers: genHeaders()
	}, function (error) {
		if (401 == error.response.status) {
			window.location.replace(API.Login)
		} else {
			return Promise.reject(error);
		}
	})
}

export const disconnectSession = SlaveID => {
	return fetch(DisconnectSession + "?SlaveID=" + SlaveID, {
		method: "POST",
		headers: genHeaders()
	}, function (error) {
		if (401 == error.response.status) {
			window.location.replace(API.Login)
		} else {
			return Promise.reject(error);
		}
	})
}

export const reconnectSession = (SlaveID) => {
	return fetch(ReconnectSession + "?SlaveID=" + SlaveID, {
		method: "POST",
		headers: genHeaders()
	}, function (error) {
		if (401 == error.response.status) {
			window.location.replace(API.Login)
		} else {
			return Promise.reject(error);
		}
	})
}


export const initializeSession = (SlaveID) => {
	return fetch(InitializeSession + "?SlaveID=" + SlaveID, {
		method: "POST",
		headers: genHeaders()
	}, function (error) {
		if (401 == error.response.status) {
			window.location.replace(API.Login)
		} else {
			return Promise.reject(error);
		}
	})
}

export const sessionSetting = (remoteToken) => {
	return fetch(InitializeSession + "?token=" + remoteToken, {
		method: "GET",
		}, function (error) {
		if (401 == error.response.status) {
			window.location.replace(API.Login)
		} else {
			return Promise.reject(error);
		}
	})
}




























// User
export const getInfor = () => {
	return fetch(Infor, {
		method: "GET",
		headers: genHeaders()
	}, function (error) {
		if (401 == error.response.status) {
			window.location.replace(API.Login)
		} else {
			return Promise.reject(error);
		}
	})
}

export const getSetting = () => {
	return fetch(Setting+"/Get", {
		method: "GET",
		headers: genHeaders()
	}, function (error) {
		if (401 == error.response.status) {
			window.location.replace(API.Login)
		} else {
			return Promise.reject(error);
		}
	})
}

export const setInfor = (body) => {
	return fetch(Infor, {
		method: "POST",
		headers: genHeaders(),
		body: JSON.stringify({
			userName: body.username ? body.username : null,
			fullName: body.fullname ? body.fullname : null,
			jobs: body.jobs ? body.jobs : null,
			phoneNumber: body.phonenumber ? body.phonenumber : null,
			gender: body.gender ? body.gender : null,
			dateOfBirth: body.dob ? body.dob : null,
			avatar: body.avatar ? body.avatar : null,
		})
	})
}

export const setSetting = (body) => {
	return fetch(Setting+"/Set", {
		method: "POST",
		headers: genHeaders(),
		body: JSON.stringify({
			device: body.defaultSetting_device != null ? body.defaultSetting_device : null,
			audioCodec: body.defaultSetting_audioCodec != null ? body.defaultSetting_audioCodec : null,
			videoCodec: body.defaultSetting_videoCodec != null ? body.defaultSetting_videoCodec : null,
			mode: body.defaultSetting_mode != null ? body.defaultSetting_mode : null,
			screenWidth: body.defaultSetting_screenWidth != null ? body.defaultSetting_screenWidth : null,
			screenHeight: body.defaultSetting_screenHeight != null ? body.defaultSetting_screenHeight : null
		})
	})
}


