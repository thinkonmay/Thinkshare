import { getCookie } from "./cookie.js"

// get environment as docker to match env
/*
	import @ from @
	const host = @.{your_environment_you_wanna_use}
*/
let host;
let currentURL = document.URL
let subdomain = currentURL.slice(0, 28)
if (subdomain == 'https://service.thinkmay.net') {
	host = "https://host.thinkmay.net"
} else {
	host = "http://hostdev.thinkmay.net"
}

// local api
export const Dashboard = "/dashboard"
export const Initialize = "/initialize"
export const Reconnect = "/reconnect"

// thinkmay api
export const Login = `${host}/Account/Login`
export const Register = `${host}/Account/Register`
export const GetInfor = `${host}/Account/GetInfor`
export const GetSession = `${host}/Account/GetSession`
export const ExternalLogin = `${host}/Account/ExternalLogin`

export const FetchSlave = `${host}/User/FetchSlave`
export const FetchSession = `${host}/User/FetchSession`

export const RejectDevice = `${host}/Device/Reject/`
export const DisconnectDevice = `${host}/Device/Disconnect`

export const TerminateSession = `${host}/Session/Terminate`
export const DisconnectSession = `${host}/Session/Disconnect`
export const ReconnectSession = `${host}/Session/Reconnect`
export const InitializeSession = `${host}/Session/Initialize`

// User
export const SetInfor = `${host}/User/SetInfor`



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

export const externalLogin = body => {
	window.open(ExternalLogin + "?provider=" + body + "&returnUrl=http://host.thinkmay.net", "", "width=520, height=520")
	return {};
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
export const setInfor = (username) => {
	return fetch(SetInfor, {
		method: "POST",
		headers: genHeaders(),
		body: JSON.stringify({
			userName: username,
		})
	})
}