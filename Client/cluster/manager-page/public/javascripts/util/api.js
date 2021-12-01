import {getCookie} from "./cookie.js"

const host = "http://localhost:5000"

let host_user;
let currentURL = document.URL
let subdomain = currentURL.slice(0, 28)
// if (subdomain == 'https://service.thinkmay.net') {
	host_user = "https://host.thinkmay.net"
// } else {
// 	host = "http://hostdev.thinkmay.net"
// }


// local api
export const Dashboard = "/dashboard"
export const Initialize = "/initialize"
export const Reconnect = "/reconnect"

//////////////////////////////////////////////// 
// owner api
export const LoginRoute = `${host}/Owner/Login`
export const RegisterClusterRoute = `${host}/Owner/Register`
export const GetClusterTokenRoute = `${host}/Owner/Cluster/Token`
export const SetTurnRoute = `${host}/Owner/Cluster/TURN`
export const StartRoute = `${host}/Owner/Start`
export const StopRoute = `${host}/Owner/Stop`
//////////////////////////////////////////////// 


//////////////////////////////////////////////// 
// User API 
export const Token = `${host}/Account/ExchangeToken`
export const Infor = `${host}/Account/Infor`
export const Session = `${host}/Account/History`

export const Setting = `${host}/Setting`

// Session API
export const InitializeSession = `${host}/Session/Initialize`
export const TerminateSession = `${host}/Session/Terminate`
export const DisconnectSession = `${host}/Session/Disconnect`
export const ReconnectSession = `${host}/Session/Reconnect`

export const UserHub = `wss://host.thinkmay.net/Hub/User`

// User API
export const FetchSlave = `${host}/Fetch/Node`
export const FetchSession = `${host}/Fetch/Session`
//////////////////////////////////////////////// 

export const genHeaders = () => {
	const token = getCookie("token")
	return Object.assign({
			"Content-Type": "application/json"
		},
		token ?
		{
			Authorization: `${token}`
		} :
		{}
	)
}

export const genHeadersUser = () => {
	const token = getCookie("token")
	return Object.assign({
			"Content-Type": "application/json"
		},
		token ?
		{
			Authorization: `Bearer ${token}`
		} :
		{}
	)
}

/////////////////////////////////////////
// user api 
export const Login = body => {
	return fetch(LoginRoute, {
		method: "POST",
		headers: genHeaders(),
		body: JSON.stringify({
			userName: body.username,
			password: body.password
		})
	})
}

export const RegisterCluster = (isPrivate, TURN) => {
	if (isPrivate) {
		RegisterClusterRoute += `?isPrivate=true`
	} else {
		RegisterClusterRoute += `?isPrivate=false`
	}
	return fetch(RegisterClusterRoute, {
		method: "POST",
		headers: genHeaders()
	})
}

export const GetClusterToken = () => {
	return fetch(GetClusterTokenRoute, {
		method: "GET",
		headers: genHeaders(),
	})
}

export const SetTurn = (ip, username, password) => {
	return fetch(SetTurnRoute + `?IP=${ip}&user=${username}&password${password}`, {
		method: 'POST',
		headers: genHeaders(),
		body: JSON.stringify({
			username: username,
			password: password
		})
	})
}

export const Start = () => {
	return fetch(StartRoute, {
		method: "POST",
		headers: genHeaders(),
	})
}


export const Stop = () => {
	return fetch(StopRoute, {
		method: "POST",
		headers: genHeaders(),
	})
}




/////////////////////////////////////////////////
// user api
export const tokenExchange = body => {
	return fetch(Token, {
		method: "POST",
		headers: genHeadersUser(),
		body: JSON.stringify({
			token: body.token,
			Validator: body.Validator
		})
	})
}



export const fetchSlave = () => {
	return fetch(FetchSlave, {
		method: "GET",
		headers: genHeadersUser()
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
		headers: genHeadersUser()
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
		headers: genHeadersUser()
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
		headers: genHeadersUser()
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
		headers: genHeadersUser()
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
		headers: genHeadersUser()
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
		headers: genHeadersUser()
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
		headers: genHeadersUser()
	}, function (error) {
		if (401 == error.response.status) {
			window.location.replace(API.Login)
		} else {
			return Promise.reject(error);
		}
	})
}

export const getSetting = () => {
	return fetch(Infor, {
		method: "GET",
		headers: genHeadersUser()
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
		headers: genHeadersUser(),
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
	return fetch(Setting, {
		method: "POST",
		headers: genHeadersUser(),
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


