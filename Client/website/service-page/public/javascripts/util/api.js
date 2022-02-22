import { getCookie, setCookie } from "./cookie.js"
import { convertDate } from "../util/date-time-convert.js"
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
	if (host.length == 0)
		throw new Error("Failt to get API infor");


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
	LogUI = `https://${host}/Log/UI`
}


// local api
export const Dashboard = "/dashboard"

export const Logout = () => {
	setCookie("logout", "true")
	setCookie("token", null, 1)
	try {
		gapi.auth.signOut();
		window.location = "/login"
	} catch {
		window.location = "/login"
	}
}

/**
 * 
 * @param {string} token 
 * @returns {string} userhub connection string
 */
export const getUserHub = async (token) => {
	await setup();
	return `wss://${host}/Hub/User?token=${token}`
}

/**
 * @returns {Headers}
 */
export const genHeaders = () => {
	const token = getCookie("token")
	return Object.assign(
		{ "Content-Type": "application/json" },
		token ? { Authorization: `Bearer ${token}` } : {}
	)
}

/**
 * @param {Response} response 
 * @returns 
 */
async function CheckLoginError(loginResponse) {
	var clone = loginResponse.clone();
	const response = await clone.json()

	if (response.errors == null) 
		return;
	
	response.errors.forEach(element => 
	{
		Utils.responseError(element.code, 
							element.description, 
							"error")
	});

	throw Error ("Fail to login");
}

/**
 * 
 * @param {Response} response 
 * @returns 
 */
async function CheckError(response) {
	if (response.status == 200)
		return;

	var clone_body = await (response.clone()).text();
	if (response.status == 401) {
		Utils.newSwal.fire({
			title: 'Unauthorize',
			text: 'Try login to your account again',
			icon: "error"
		}).then((result) => { 
			if (result.isConfirmed) 
				Logout();  
			})
	} else if (response.status == 400) {
		Utils.newSwal.fire({
			title: 'error',
			text: clone_body,
			icon: "error" })
		throw new Error('Bad request')
	} else if (response.status == 500) {
		Utils.newSwal.fire({
			title: " Our server error 😭😭😭 ",
			text: 'Punish our developer please',
			icon: "error" })
		throw new Error('Server error')
	} else {
		Utils.responseErrorHandler(response)
		throw new Error('Unknown error')
	}
}

/**
 * @param {string} error 
 */
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










/**
 * 
 * @param {LoginModel} body 
 * @returns 
 */
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
	await CheckError(res);
	await CheckLoginError(res);
	return res;
}

/**
 * 
 * @param {AuthenticationRequest} body 
 * @returns 
 */
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
	await CheckError(res);
	await CheckLoginError(res);
	return res;
}

/**
 * 
 * @param {UpdatePasswordModel} body 
 * @returns 
 */
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
	await CheckError(res);
	await CheckLoginError(res);
	return res;
}

/**
 * 
 * autofill job and convert birthday 
 * @param {RegisterModel} body 
 * @returns {Promise<AuthResponse>}
 */
export const register = async body => {
	await setup();
	body.dob = convertDate(body.dob)
	body.jobs = body.jobs == null ? "nosetJobs" : body.jobs

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

	await CheckError(res);
	await CheckLoginError(res);
	return res;
}




/**
 * 
 * @param {string} des 
 * @returns {Promise<IdentityResult>}
 */
export const managerRegister = async des => {
	await setup();
	var res = await fetch(`${Manager}?Description=${des}`, {
		method: "POST",
		headers: genHeaders(),
	})
	await CheckError(res)
	return res;
}


/**
 * 
 * @param {string} name 
 * @param {string} password 
 * @param {string} region 
 * @returns {Promise<GlobalCluster>}
 */
export const requestCluster = async (name, password, region) => {
	await setup();
	var res = await fetch(Cluster + `?ClusterName=${name}&region=${region}`, {
		method: "POST",
		headers: genHeaders(),
		body: `"${password}"`
	})
	await CheckError(res)
	return res;
}

/**
 * 
 * @returns {Promise<List<Name,URL>>}
 */
export const getClusters = async () => {
	await setup();
	var res = await fetch(Clusters, {
		method: "GET",
		headers: genHeaders(),
	})
	await CheckError(res)
	return res;
}

















/**
 * 
 * @returns {Dictionary<int,string>}
 */
export const fetchSlave = async () => {
	await setup();
	var res = await fetch(FetchSlave, {
		method: "GET",
		headers: genHeaders()
	})
	await CheckError(res);
	return res;
}

/**
 * @returns {Dictionary<int,string>}
 */
export const fetchSession = async () => {
	await setup();
	var res = await fetch(FetchSession, {
		method: "GET",
		headers: genHeaders()
	})
	await CheckError(res)
	return res;
}

/**
 * 
 * @param {int} workerID 
 * @returns {Promise<WorkerNode>}
 */
export const fetchInfor = async (workerID) => {
	await setup();
	var response = await fetch(`${FetchInfor}?WorkerID=${workerID}`, {
		method: "GET",
	})
	await CheckError(response);
	return response;
}

/**
 * 
 * @returns {Promise<Dictionary<int,string>>}
 */
export const getSession = async () => {
	await setup();
	var response = await fetch(Session, {
		method: "GET",
		headers: genHeaders()
	})
	await CheckError(response);
	return response;
}

















/**
 * 
 * @param {int} SlaveID 
 * @returns 
 */
export const terminateSession = async (SlaveID) => {
	await setup();
	var response = await fetch(`${TerminateSession}?SlaveID=${SlaveID}`, {
		method: "DELETE",
		headers: genHeaders()
	})
	await CheckError(response);
	return response;
}

/**
 * 
 * @param {int} SlaveID 
 * @returns 
 */
export const disconnectSession = async SlaveID => {
	await setup();
	var response = await fetch(`${DisconnectSession}?SlaveID=${SlaveID}`, {
		method: "POST",
		headers: genHeaders()
	})
	await CheckError(response);
	return response;
}

/**
 * 
 * @param {int} SlaveID 
 * @returns 
 */
export const reconnectSession = async (SlaveID) => {
	await setup();
	var response = await fetch(`${ReconnectSession}?SlaveID=${SlaveID}`, {
		method: "POST",
		headers: genHeaders()
	})
	await CheckError(response);
	return response;
}


/**
 * 
 * @param {int} SlaveID 
 * @returns 
 */
export const initializeSession = async (SlaveID) => {
	await setup();
	var response = await fetch(`${InitializeSession}?SlaveID=${SlaveID}`, {
		method: "POST",
		headers: genHeaders()
	})
	await CheckError(response);
	return response;
}





/**
 * 
 * @returns { Promise<UserInforModel> } 
 */
export const getInfor = async () => {
	await setup();
	var response = await fetch(Infor, {
		method: "GET",
		headers: genHeaders()
	})
	await CheckError(response);
	return response;
}

/**
 * @returns { Promise<AuthenticationResponse> }
 */
export const getRoles = async () => {
	await setup();
	var response = await fetch(Roles, {
		method: "GET",
		headers: genHeaders()
	});
	await CheckError(response)
	return response;
}



/**
 * 
 * @param {Promise<UserInforModel>} body 
 * @returns 
 */
export const setInfor = async (body) => {
	await setup();
	var response = await fetch(Infor, 
	{
		method: "POST",
		headers: genHeaders(),
		body: JSON.stringify(body)
	})
	await CheckError(response);
	return response;
}

/**
 * 
 * @param {Promise<UserSetting>} body 
 * @returns 
 */
export const setSetting = async (body) => {
	await setup();
	var response = await fetch(`${Setting}/Set`, 
	{
		method: "POST",
		headers: genHeaders(),
		body: JSON.stringify(body)
	});
	await CheckError(response);
	return response;
}



/**
 * @returns {Promise<UserSetting>}
 */
export const getSetting = async () => {
	await setup();
	var response = await fetch(`${Setting}/Get`, 
	{
		method: "GET",
		headers: genHeaders()
	});
	await CheckError(response);
	return response;
}