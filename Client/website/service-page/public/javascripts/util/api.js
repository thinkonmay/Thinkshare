import { getCookie } from "./cookie.js"
import * as Utils from "../util/utils.js"


var host;
var Login;
var Register;
var Token;
var Infor;
var Roles;
var Session;

var Manager;
var Cluster;
var Clusters;

var Setting;

var InitializeSession;
var TerminateSession;
var DisconnectSession;
var ReconnectSession;

var FetchSlave;
var FetchSession;
var FetchInfor;

const setup = async () => {
	if (host != null)
		return;

	host = await ((await fetch('API.js')).text())


	Login = `https://${host}/Account/Login`
	Register = `https://${host}/Account/Register`
	Token = `https://${host}/Account/ExchangeToken`
	Infor = `https://${host}/Account/Infor`
	Roles = `https://${host}/Account/Roles`
	Session = `https://${host}/Account/History`

	Manager = `https://${host}/Manager/Request`
	Clusters = `https://${host}/Manager/Cluster`
	Cluster = `https://${host}/Manager/ManagedCluster/Request`

	Setting = `https://${host}/Setting`

	InitializeSession = `https://${host}/Session/Initialize`
	TerminateSession = `https://${host}/Session/Terminate`
	DisconnectSession = `https://${host}/Session/Disconnect`
	ReconnectSession = `https://${host}/Session/Reconnect`

	FetchSlave = `https://${host}/Fetch/Node`
	FetchSession = `https://${host}/Fetch/Session`
	FetchInfor = `https://${host}/Fetch/Worker/Infor`
}


// local api
export const Dashboard = "/dashboard"


export const getUserHub = async (token) => {
	await setup();
	return `wss://${host}/Hub/User?token=${token}`
}

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

export const login = async body => {
	await setup();
	return fetch(Login, {
		method: "POST",
		headers: genHeaders(),
		body: JSON.stringify({
			userName: body.username,
			password: body.password
		})
	})
}

export const register = async body => {
	await setup();
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





export const managerRegister = async des => {
	await setup();
	return fetch(`${Manager}?Description=${des}`, {
		method: "POST",
		headers: genHeaders(),
	})
}
export const requestCluster = async (name, password, region) => {
	await setup();
	return fetch(Cluster + `?ClusterName=${name}&?region${region}`, {
		method: "POST",
		headers: genHeaders(),
		body: `"${password}"`
	})
}
export const getClusters = async () => {
	await setup();
	return fetch(Clusters, {
		method: "GET",
		headers: genHeaders(),
	})
}














export const tokenExchange = async body => {
	await setup();
	return fetch(Token, {
		method: "POST",
		headers: genHeaders(),
		body: JSON.stringify({
			token: body.token,
			Validator: body.Validator
		})
	}, function (error) {
		if (401 == error.response.status) {
			window.location.replace(API.Login)
		} else {
			return Promise.reject(error);
		}
	})
}



export const fetchSlave = async () => {
	await setup();
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

export const fetchSession = async () => {
	await setup();
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
export const fetchInfor = async (workerID) => {
	await setup();
	return fetch(FetchInfor + "?WorkerID=" + workerID, {
		method: "GET",
	}, function (error) {
		if (401 == error.response.status) {
			window.location.replace(API.Login)
		} else {
			return Promise.reject(error);
		}
	})
}

export const getSession = async () => {
	await setup();
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

















export const terminateSession = async SlaveID => {
	await setup();
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

export const disconnectSession = async SlaveID => {
	await setup();
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

export const reconnectSession = async (SlaveID) => {
	await setup();
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


export const initializeSession = async (SlaveID) => {
	await setup();
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




























/**
 * 
 * @returns 
 */
export const getInfor = async () => {
	await setup();
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

/**
 * 
 * @returns 
 */
export const getRoles = async () => {
	await setup();
	return fetch(Roles, {
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


export const getSetting = async () => {
	await setup();
	return fetch(Setting + "/Get", {
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

export const setInfor = async (body) => {
	await setup();
	return fetch(Infor, {
		method: "POST",
		headers: genHeaders(),
		body: JSON.stringify({ body })
	}, function (error) {
		if (401 == error.response.status) {
			window.location.replace(API.Login)
		} else {
			return Promise.reject(error);
		}
	})
}

export const setSetting = async (body) => {
	await setup();
	return fetch(Setting + "/Set", {
		method: "POST",
		headers: genHeaders(),
		body: JSON.stringify({
			device: body.device,
			engine: body.engine,
			audioCodec: body.audioCodec,
			videoCodec: body.videoCodec,
			mode: body.mode,
			screenWidth: body.screenWidth,
			screenHeight: body.screenHeight
		})
	}, function (error) {
		if (401 == error.response.status) {
			window.location.replace(API.Login)
		} else {
			return Promise.reject(error);
		}
	})
}


