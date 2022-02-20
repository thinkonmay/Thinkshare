import { getCookie, setCookie } from "./cookie.js"
import * as Utils from "../util/utils.js"

var host;
var Login;
var Register;
var Token;
var Infor;
var Roles;
var Session;
var Password;

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

var LogUI;

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
	Password = `https://${host}/Account/Password/Update`

	Manager = `https://${host}/Manager/Request`
	Clusters = `https://${host}/Manager/Clusters`
	Cluster = `https://${host}/Manager/Cluster/Request`

	Setting = `https://${host}/Setting`

	InitializeSession = `https://${host}/Session/Initialize`
	TerminateSession = `https://${host}/Session/Terminate`
	DisconnectSession = `https://${host}/Session/Disconnect`
	ReconnectSession = `https://${host}/Session/Reconnect`

	FetchSlave = `https://${host}/Fetch/Node`
	FetchSession = `https://${host}/Fetch/Session`
	FetchInfor = `https://${host}/Fetch/Worker/Infor`

	LogUI = `https://development.thinkmay.net/Log/UI`
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

async function CheckError(response) {

	if (response.status == 200)
		return;
	if (response.status == 401) {
		var clone_body = await (response.clone()).text();
		Utils.newSwal.fire({
			title: 'Unauthorize',
			text: 'This process will logout your account',
			icon: "error"
		}).then((result) => {
			if (result.isConfirmed) {
				setCookie("logout", "true")
				setCookie("token", null, 1)
				try {
					gapi.auth.signOut();
					window.location = "/login"
				} catch {
					window.location = "/login"
				}
			}
		})
		throw new Error('Unauthorize')
	}
	else if (response.status == 400) {
		var clone_body = await (response.clone()).text();
		Utils.newSwal.fire({
			title: 'error',
			text: clone_body,
			icon: "error"
		})
	}
	else {
		Utils.responseErrorHandler(response)
	}

}

export const logUI = async error => {
	await setup();
	await fetch(LogUI, {
		method: "POST",
		headers: genHeaders(),
		body: JSON.stringify({
			Error: error,
			timestamp: new Date().toISOString()
		})
	})
}

export const login = async body => {
	await setup();
	var res = await fetch(Login, {
		method: "POST",
		headers: genHeaders(),
		body: JSON.stringify({
			userName: body.username,
			password: body.password
		})
	})
	CheckError(res)
	return res;
}

export const updatePassword = async body => {
	await setup();
	var res = await fetch(Password, {
		method: "POST",
		headers: genHeaders(),
		body: JSON.stringify({
			Old: body.Old,
			New: body.New
		})
	})
	CheckError(res)
	return res;
}

export const register = async body => {
	await setup();
	var res = await fetch(Register, {
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
	CheckError(res)
	return res;
}





export const managerRegister = async des => {
	await setup();
	var res = await fetch(`${Manager}?Description=${des}`, {
		method: "POST",
		headers: genHeaders(),
	})
	CheckError(res)
	return res;
}


/**
 * 
 * @param {*} name 
 * @param {*} password 
 * @param {*} region 
 * @returns 
 */
export const requestCluster = async (name, password, region) => {
	await setup();
	var res = await fetch(Cluster + `?ClusterName=${name}&region=${region}`, {
		method: "POST",
		headers: genHeaders(),
		body: `"${password}"`
	})
	CheckError(res)
	return res;
}
export const getClusters = async () => {
	await setup();
	var res = await fetch(Clusters, {
		method: "GET",
		headers: genHeaders(),
	})
	CheckError(res)
	return res;
}














export const tokenExchange = async body => {
	await setup();
	var res = await fetch(Token, {
		method: "POST",
		headers: genHeaders(),
		body: JSON.stringify({
			token: body.token,
			Validator: body.Validator
		})
	})
	CheckError(res)
	return res;
}



export const fetchSlave = async () => {
	await setup();
	try {
		var res = await fetch(FetchSlave, {
			method: "GET",
			headers: genHeaders()
		})
		CheckError(res)
	} catch (err) {
		const [, lineno, colno] = e.stack.match(/(\d+):(\d+)/);
		var errors = `${e.message} ${e.stack} Line: ${lineno} Column: ${colno}}`
		logUI(errors)
	}
	return res;
}

export const fetchSession = async () => {
	await setup();
	var res = await fetch(FetchSession, {
		method: "GET",
		headers: genHeaders()
	})
	CheckError(res)
	return res;
}
export const fetchInfor = async (workerID) => {
	await setup();
	var response = await fetch(`${FetchInfor}?WorkerID=${workerID}`, {
		method: "GET",
	})
	CheckError(response);
	return response;
}

export const getSession = async () => {
	await setup();
	var response = await fetch(Session, {
		method: "GET",
		headers: genHeaders()
	})
	CheckError(response);
	return response;
}

















export const terminateSession = async SlaveID => {
	await setup();
	var response = await fetch(TerminateSession + "?SlaveID=" + SlaveID, {
		method: "DELETE",
		headers: genHeaders()
	})
	CheckError(response);
	return response;
}

export const disconnectSession = async SlaveID => {
	await setup();
	var response = await fetch(DisconnectSession + "?SlaveID=" + SlaveID, {
		method: "POST",
		headers: genHeaders()
	})
	CheckError(response);
	return response;
}

export const reconnectSession = async (SlaveID) => {
	await setup();
	var response = await fetch(ReconnectSession + "?SlaveID=" + SlaveID, {
		method: "POST",
		headers: genHeaders()
	})
	CheckError(response);
	return response;
}


export const initializeSession = async (SlaveID) => {
	await setup();
	var response = await fetch(InitializeSession + "?SlaveID=" + SlaveID, {
		method: "POST",
		headers: genHeaders()
	})
	CheckError(response);
	return response;
}




























/**
 * 
 * @returns 
 */
export const getInfor = async () => {
	await setup();
	try {

		var response = await fetch(Infor, {
			method: "GET",
			headers: genHeaders()
		})

		CheckError(response);
		return response;
	} catch (err) {

	}

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
			Utils.newSwal.fire({
				title: 'error',
				text: error.response,
				icon: "error"
			})
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
			Utils.newSwal.fire({
				title: 'error',
				text: error.response,
				icon: "error"
			})
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


