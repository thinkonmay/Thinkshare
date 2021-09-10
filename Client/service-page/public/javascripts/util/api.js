import {getCookie} from "./cookie.js"

const host = "http://conductor.thinkmay.net" // thinkmay host

// local api
export const Dashboard = "/dashboard"
export const DashboardAdmin = "/dashboard-admin"
export const Initialize = "/initialize"

// thinkmay api
export const Login = `${host}/Account/Login`
export const LoginAdmin = `${host}/Account/Login-admin`
export const Register = `${host}/Account/Register`
export const FetchSlave = `${host}/User/FetchSlave`
export const FetchSession = `${host}/User/FetchSession`
export const RejectDevice = `${host}/Session/Disconnect`
export const QuerySession = `${host}/Query/Session`

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

export const login = body => {
	return fetch(Login, {
		method: "POST",
		headers: genHeaders(),
		body: JSON.stringify({
			email: body.email,
			password: body.password
		})
	})
}

export const register = body => {
	return fetch(Register, {
		method: "POST",
		headers: genHeaders(),
		body: JSON.stringify({
			email: body.email,
			password: body.password,
			fullName: body.fullName,
			dayOfBirth: body.dayOfBirth
		})
	})
}

export const fetchSlave = () => {
	return fetch(FetchSlave, {
		method: "GET",
		headers: genHeaders()
	})
}

export const querySession = SlaveID => {
	return fetch(QuerySession, {
		method: "GET",
		headers: genHeaders(),
		body: JSON.stringify({
			SlaveID
		})
	})
}

export const rejectDevice = sessionClientId => {
	return fetch(RejectDevice, {
		method: "POST",
		headers: genHeaders(),
		body: JSON.stringify({
			sessionClientId
		})
	})
}
