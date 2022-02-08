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



// local api
export const Dashboard = "/dashboard"
export const Initialize = "/initialize"
export const Reconnect = "/reconnect"

// owner api
const LoginRoute = `${host}/Owner/Login`
const GetWorkerStateRoute = `${host}/Owner/Worker/State`
const GetInforClusterRoute = `${host}/Owner/Cluster/Infor`
const IsRegisteredRoute = `${host}/Owner/Cluster/isRegistered`




export const genHeaders = () => {
	const token = getCookie("token")
	return Object.assign({
			"Content-Type": "application/json"
		},
		token ? {
			Authorization: `${token}`
		} : {}
	)
}

export const genHeadersUser = () => {
	const token = getCookie("token")
	return Object.assign({
			"Content-Type": "application/json"
		},
		token ? {
			Authorization: `Bearer ${token}`
		} : {}
	)
}






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
	}, function (error) {
		if (401 == error.response.status) {
			window.location.replace(API.Login)
		} else {
			return Promise.reject(error);
		}
	})
}

export const getInforClusterRoute = () => {
	return fetch(GetInforClusterRoute, {
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


export const getWorkerStateRoute = () => {
	return fetch(GetWorkerStateRoute, {
		method: "GET",
		headers: genHeaders(),
	}, function (error) {
		if (401 == error.response.status) {
			window.location.replace(API.Login)
		} else {
			return Promise.reject(error);
		}
	})
}




export const isRegistered = () => {
	return fetch(IsRegisteredRoute, {
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