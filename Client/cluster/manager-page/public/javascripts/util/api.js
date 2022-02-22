import { prepare_booker_infor } from "./cluster-component.js"
import { getCookie } from "./cookie.js"
import * as Utils from "./utils.js"

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

// owner api
const LoginRoute = 				`${host}/Owner/Login`
const GetInforClusterRoute = 	`${host}/Owner/Cluster/Infor`
const IsRegisteredRoute = 		`${host}/Owner/Cluster/isRegistered`
const UnRegisteredRoute = 		`${host}/Owner/Cluster/Unregister`
const GetWorkerStateRoute = 	`${host}/Owner/Worker/State`
const SetupRoleRoute = 			`${host}/Owner/Cluster/Role`
const SetupInstantRoleRoute = 	`${host}/Owner/Cluster/Role/Temporary`


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


const genHeaders = () => {
	const token = getCookie("token")
	return Object.assign({
			"Content-Type": "application/json"
		},
		token ? {
			Authorization: `${token}`
		} : {}
	)
}



export async function login ( body ) 
{
	var res = await fetch(LoginRoute, {
		method: "POST",
		headers: genHeaders(),
		body: JSON.stringify({
			userName: body.username,
			password: body.password
		})
	})
	await CheckError(res);
	return res;
}

export async function getInforClusterRoute () 
{
	var res = await fetch(GetInforClusterRoute, {
		method: "GET",
		headers: genHeaders()
	})
	await CheckError(res);
	return res;
}


export async function getWorkerStateRoute () 
{
	var res = await fetch(GetWorkerStateRoute, {
		method: "GET",
		headers: genHeaders(),
	})
	await CheckError(res);
	return res;
}



export async function UnRegister () 
{
	var res = await fetch(UnRegisteredRoute, {
		method: "DELETE",
		headers: genHeaders()
	})
	await CheckError(res);
	return res;
}

export async function isRegistered () 
{
	var res = await fetch(IsRegisteredRoute, {
		method: "POST",
		headers: genHeaders()
	})
	await CheckError(res);
	return res;
}
















export async function getExistingRole () 
{
	var res = await fetch(SetupRoleRoute, {
		method: "GET",
		headers: genHeaders()
	})
	await CheckError(res);
	return res;
}

export async function createNewRole (start,end,user) 
{
	var res = await fetch(`${SetupRoleRoute}`, {
		method: "POST",
		headers: genHeaders(),
		/**
		 * ClusterRoleRequest
		 */
		body: JSON.stringify({
			Start: start,
			Endtime: end,
			Description: "Created by cluster owner",
			User: user
		})
	})
	await CheckError(res);
	await prepare_booker_infor();
	return res;
}

export async function createNewInstantRole (user,minutes) 
{
	var res = await fetch(`${SetupInstantRoleRoute}?UserName=${user}&minutes=${minutes}`, {
		method: "POST",
		headers: genHeaders()
	})
	await CheckError(res);
	await prepare_booker_infor();
	return res;
}

export async function deleteRole (ID) 
{
	var res = await fetch(`${SetupRoleRoute}?RoleID=${ID}`, {
		method: "DELETE",
		headers: genHeaders()
	})
	await CheckError(res);
	await prepare_booker_infor();
	return res;
}