import { getCookie } from "./cookie.js"

let host = "";
let currentURL = document.URL
let count = 0
for (let i = 0; i < currentURL.length; i++) {
	if (currentURL[i] == ":")
		count++;
	if (count == 2) break;
	host += currentURL[i];
}
host += ":5000"


const host_user = "https://host.thinkmay.net"


// local api
export const Dashboard = "/dashboard"
export const Initialize = "/initialize"
export const Reconnect = "/reconnect"

//////////////////////////////////////////////// 
// owner api
const LoginRoute = `${host}/Owner/Login`
let RegisterClusterRoute = `${host}/Owner/Register`
const GetInforClusterRoute = `${host}/Owner/Cluster/Infor`
const GetClusterTokenRoute = `${host}/Owner/Cluster/Token`
const GetWorkerStateRoute = `${host}/Owner/Worker/State`
const SetTurnRoute = `${host}/Owner/Cluster/TURN`
const StartRoute = `${host}/Owner/Start`
const StopRoute = `${host}/Owner/Stop`
const IsRegisteredRoute = `${host}/Owner/Cluster/isRegistered`
//////////////////////////////////////////////// 


/**
 * User API
 */
const Token = `${host_user}/Account/ExchangeToken`
const Infor = `${host_user}/Account/Infor`
const Session = `${host_user}/Account/History`

/**
 * Setting API
 */
const Setting = `${host_user}/Setting`

// Session API
const InitializeSession = `${host_user}/Session/Initialize`
const TerminateSession = `${host_user}/Session/Terminate`
const DisconnectSession = `${host_user}/Session/Disconnect`
const ReconnectSession = `${host_user}/Session/Reconnect`



const UserHub = `wss://host.thinkmay.net/Hub/User`

// User API
const FetchSlave = `${host_user}/Fetch/Node`
const FetchSession = `${host_user}/Fetch/Session`
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
export const login = body => {
	return fetch(LoginRoute, {
		method: "POST",
		headers: genHeaders(),
		body: JSON.stringify({
			userName: body.username,
			password: body.password
		})
	})
}

export const registerCluster = (isPrivate, Name) => {
    RegisterClusterRoute = `${host}/Owner/Register`
	if (isPrivate) {
		RegisterClusterRoute += `?isPrivate=true&ClusterName=${Name}`
	} else {
		RegisterClusterRoute += `?isPrivate=false&ClusterName=${Name}`
	}
	return fetch(RegisterClusterRoute, {
		method: "POST",
		headers: genHeaders()
	})
}

export const getInforClusterRoute = () => {
	return fetch(GetInforClusterRoute, {
		method: "GET",
		headers: genHeaders()
	})
}

// save token for database to access 
export const getClusterToken = () => {
	return fetch(GetClusterTokenRoute, {
		method: "GET",
		headers: genHeaders(),
	})
}

export const getWorkerStateRoute = () => {
	return fetch(GetWorkerStateRoute, {
		method: "GET",
		headers: genHeaders(),
	})
}

export const setTurn = (ip, username, password) => {
	return fetch(SetTurnRoute + `?turnIP=${ip}&turnUSER=${username}&turnPASSWORD${password}`, {
		method: 'POST',
		headers: genHeaders(),
	})
}

export const start = () => {
	return fetch(StartRoute, {
		method: "POST",
		headers: genHeaders(),
	})
}


export const stop = () => {
	return fetch(StopRoute, {
		method: "POST",
		headers: genHeaders(),
	})
}


export const isRegistered = () => {
	return fetch(IsRegisteredRoute, {
		method: "POST",
		headers: genHeaders()
	})
}




///////////////////////////////////////////////////////////////////////
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


// temp
////////////////////////////////////////////////////
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
	return fetch(SessionInfor + "?token=" + remoteToken, {
		method: "GET",
		}, function (error) {
		if (401 == error.response.status) {
			window.location.replace(API.Login)
		} else {
			return Promise.reject(error);
		}
	})
}