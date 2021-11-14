import { getCookie } from "./cookie.js"

// get environment as docker to match env
/*
	import @ from @
	const host = @.{your_environment_you_wanna_use}
*/
let host;
let currentURL = document.URL
let subdomain = currentURL.slice(0, 28)
// if (subdomain == 'https://service.thinkmay.net') {
host = "https://host.thinkmay.net"
// } else {
// 	host = "http://hostdev.thinkmay.net"
// }

// local api
export const Dashboard = "/dashboard"
export const Initialize = "/initialize"
export const Reconnect = "/reconnect"

// thinkmay api
// Account API
export const Login = `${host}/Account/Login`
export const Register = `${host}/Account/Register`
export const Token = `${host}/Account/ExchangeToken`
export const GrantRole = `${host}/Account/GrantRole`
export const GetInfor = `${host}/Account/GetInfor`
export const SetInfor = `${host}/Account/SetInfor`
export const GetSession = `${host}/Account/GetSession`

// Session API
export const InitializeSession = `${host}/Session/Initialize`
export const TerminateSession = `${host}/Session/Terminate`
export const DisconnectSession = `${host}/Session/Disconnect`
export const ReconnectSession = `${host}/Session/Reconnect`

// export const RejectDevice = `${host}/Device/Reject/`
// export const DisconnectDevice = `${host}/Device/Disconnect`

// User API
export const FetchSlave = `${host}/User/FetchSlave`
export const FetchSession = `${host}/User/FetchSession`

export const QuerySession = `${host}/Query/Session`

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


export const getInfor = () => {
	return fetch(GetInfor, {
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
	return fetch(GetSession, {
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



export const querySession = SlaveID => {
	return fetch(QuerySession + "?SlaveID=" + SlaveID, {
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

export const rejectDevice = SlaveID => {
	return fetch(RejectDevice + "?SlaveID=" + SlaveID, {
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

export const disconnectDevice = SlaveID => {
	return fetch(DisconnectDevice + "?SlaveID=" + SlaveID, {
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
	var cap = getCookie("cap");

	var body = {
		SlaveID: SlaveID,
		cap: JSON.parse(cap)
	}
	return fetch(InitializeSession, {
		method: "POST",
		headers: genHeaders(),
		body: JSON.stringify(body)
	}, function (error) {
		if (401 == error.response.status) {
			window.location.replace(API.Login)
		} else {
			return Promise.reject(error);
		}
	})
}

// User
export const setInfor = (body) => {
	return fetch(SetInfor, {
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
			defaultSetting: {
				id: body.defaultSetting_id,
				device: body.defaultSetting_device ? body.defaultSetting_device : null,
				audioCodec: body.defaultSetting_audioCodec ? body.defaultSetting_audioCodec : null,
				videoCodec: body.defaultSetting_videoCodec ? body.defaultSetting_videoCodec : null,
				mode: body.defaultSetting_mode ? body.defaultSetting_mode : null,
				screenWidth: body.defaultSetting_screenWidth ? body.defaultSetting_screenWidth : null,
				screenHeight: body.defaultSetting_screenHeight ? body.defaultSetting_screenHeight : null
			}
		})
	})
}