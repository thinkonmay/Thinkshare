import {getCookie} from "./cookie.js"

const host = "http://localhost:5000"

// owner api
export const LoginRoute = `${host}/Owner/Login`
export const RegisterClusterRoute = `${host}/Owner/Register`
export const GetClusterTokenRoute = `${host}/Owner/Cluster/Token`
export const SetTurnRoute = `${host}/Owner/Cluster/TURN`
export const StartRoute = `${host}/Owner/Start`
export const StopRoute = `${host}/Owner/Stop`
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

export const Login = (username, password) => {
	return fetch(LoginRoute, {
		method: "POST",
		headers: genHeaders(),
		body: JSON.stringify({
			userName: username,
			password: password
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