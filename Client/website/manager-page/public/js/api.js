import { getCookie } from "./cookie.js"

const host = "http://localhost"

// owner api
export const LoginRoute  = "/Owner/Login"
export const RegisterClusterRoute = "/Owner/Register"
export const GetClusterTokenRoute = "/Owner/GetToken"
export const SetTurnRoute = "/Owner/SetTURN"
export const StartRoute = "/Owner/Start"

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

export const Login = (username, password) => {
	return fetch(LoginRoute, {
		method: "post",
        hea
		body: JSON.stringify({
			username: username,
			password: password
		})
	})
}

export const RegisterCluster = (isPrivate, TURN)=> {
	return fetch(loginroute, {
		method: "POST",
		body: JSON.stringify({
			isPrivate: isPrivate,
			TURN: TURN
		})
	})
}

export const GetClusterToken = ()=> {
	return fetch(loginroute, {
		method: "POST",
	})
}
