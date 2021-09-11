import {getCookie} from "./cookie.js"

const host = "https://conductor.thinkmay.net"

// local api
export const Dashboard = "/dashboard"
export const DashboardAdmin = "/dashboard-admin"
export const Initialize = "/initialize"
export const Reconnect = "/reconnect"

// thinkmay api
export const Login = `${host}/Account/Login`
export const LoginAdmin = `${host}/Account/Login-admin`
export const Register = `${host}/Account/Register`
export const GetInfor = `${host}/Account/GetInfor`

export const FetchSlave = `${host}/User/FetchSlave`
export const FetchSession = `${host}/User/FetchSession`

export const RejectDevice = `${host}/Device/Reject/`
export const DisconnectDevice = `${host}/Device/Disconnect`

export const TerminateSession = `${host}/Session/Terminate`
export const DisconnectSession = `${host}/Session/Disconnect`
export const ReconnectSession = `${host}/Session/Reconnect`
export const InitializeSession = `${host}/Session/Initialize`


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

export const getInfor = () => {
	return fetch(GetInfor, {
		method: "GET",
		headers: genHeaders()
	})
}

export const fetchSlave = () => {
	return fetch(FetchSlave, {
		method: "GET",
		headers: genHeaders()
	})
}

export const fetchSession = () => {
	return fetch(FetchSession, {
		method: "GET",
		headers: genHeaders()
	})
}

export const querySession = SlaveID => {
	return fetch(QuerySession + "?SlaveID=" + SlaveID, {
		method: "GET",
		headers: genHeaders()
	})
}

export const rejectDevice = SlaveID => {
	return fetch(RejectDevice + "?SlaveID=" + SlaveID, {
		method: "DELETE",
		headers: genHeaders()
	})
}

export const disconnectDevice = SlaveID => {
	return fetch(DisconnectDevice + "?SlaveID=" + SlaveID, {
		method: "DELETE",
		headers: genHeaders()
	})
}

export const terminateSession = SlaveID => {
	return fetch(TerminateSession + "?SlaveID=" + SlaveID, {
		method: "DELETE",
		headers: genHeaders()
	})
}

export const disconnectSession = SlaveID => {
	return fetch(DisconnectSession, {
		method: "POST",
		headers: genHeaders(),
		body: JSON.stringify({
			SlaveID
		})
	})
}

export const reconnectSession = (SlaveID,token) => {
	return fetch(ReconnectSession, {
		method: "POST",
		headers: {
			Authorization: "Bearer " + token,
			"Content-Type": "application/json"
		},
		body: JSON.stringify({
			SlaveID
		})
	})
}

export const initializeSession = (body,token) => {
		return fetch(InitializeSession, {
			headers: {
				Authorization: "Bearer " + token,
				"Content-Type": "application/json"
			},
			method: "POST",
			body: JSON.stringify(body)
		}
	)
}