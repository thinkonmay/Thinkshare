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


const Login = "/login"

// owner api
const LoginRoute = 				`${host}/Owner/Login`
const GetInforClusterRoute = 	`${host}/Owner/Cluster/Infor`
const IsRegisteredRoute = 		`${host}/Owner/Cluster/isRegistered`
const UnRegisteredRoute = 		`${host}/Owner/Cluster/Unregister`
const GetWorkerStateRoute = 	`${host}/Owner/Worker/State`
const SetupRoleRoute = 			`${host}/Owner/Cluster/Role`




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

export const getInforClusterRoute = () => {
	return fetch(GetInforClusterRoute, {
		method: "GET",
		headers: genHeaders()
	}, function (error) {
		if (401 == error.response.status) {
			window.location.replace(Login)
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
			window.location.replace(Login)
		} else {
			return Promise.reject(error);
		}
	})
}



export const UnRegistered = () => {
	return fetch(UnRegisteredRoute, {
		method: "DELETE",
		headers: genHeaders()
	}, function (error) {
		if (401 == error.response.status) {
			window.location.replace(Login)
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
			window.location.replace(Login)
		} else {
			return Promise.reject(error);
		}
	})
}
















export const getExistingRole = () => {
	return fetch(SetupRoleRoute, {
		method: "GET",
		headers: genHeaders()
	}, function (error) {
		if (401 == error.response.status) {
			window.location.replace(Login)
		} else {
			return Promise.reject(error);
		}
	})
}

export const createNewRole = (start,end,user) => {
	return fetch(`SetupRoleRoute?Start=${start}&End=${end}&UserName=${user}`, {
		method: "POST",
		headers: genHeaders()
	}, function (error) {
		if (401 == error.response.status) {
			window.location.replace(Login)
		} else {
			return Promise.reject(error);
		}
	})
}
export const deleteRole = () => {
	return fetch(SetupRoleRoute, {
		method: "DELETE",
		headers: genHeaders()
	}, function (error) {
		if (401 == error.response.status) {
			window.location.replace(Login)
		} else {
			return Promise.reject(error);
		}
	})
}