import {getCookie} from "./cookie.js"

const host = "https://conductor.thinkmay.net"

// local api
export const Dashboard = "/dashboard"
export const Initialize = "/initialize"
export const Reconnect = "/reconnect"

export const Error = "/500"

// thinkmay api
export const Login = `${host}/Account/Login`

export const FetchSlave = `${host}/User/FetchSlave`
export const FetchSession = `${host}/User/FetchSession`

export const AddDevice = `${host}/Device/Add`
export const RejectDevice = `${host}/Device/Reject/`
export const DisconnectDevice = `${host}/Device/Disconnect`

export const TerminateSession = `${host}/Session/Terminate`
export const DisconnectSession = `${host}/Session/Disconnect`
export const ReconnectSession = `${host}/Session/Reconnect`
export const InitializeSession = `${host}/Session/Initialize`

export const QueryDevice = `${host}/Query/Device`
export const QuerySession = `${host}/Query/Session`
export const QueryCommand = `${host}/Query/Command`

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
			userName: body.username,
			password: body.password
		})
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







export const queryDevice = () => {
	return fetch(QueryDevice, {
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

export const queryCommand = SlaveID => {
	return fetch(QueryCommand + "?SlaveID=" + SlaveID, {
		method: "GET",
		headers: genHeaders()
	})
}






export const addDevice = () => {
	return fetch(AddDevice, {
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
	return fetch(DisconnectSession + "?SlaveID=" + SlaveID, {
		method: "POST",
		headers: genHeaders()
	})
}

export const reconnectSession = (SlaveID) => {
	return fetch(ReconnectSession + "?SlaveID=" + SlaveID, {
		method: "POST",
		headers: genHeaders()
	})
}


export const initializeSession = (SlaveID) => {
	var cap = getCookie("cap");

	var body = {
		SlaveID: SlaveID,
		cap: JSON.parse(cap)
	}
	return fetch(InitializeSession, {
		method: "POST",
		headers: genHeaders(), 
		body: JSON.stringify(body)
	})
}






