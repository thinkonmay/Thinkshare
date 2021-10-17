

const host = "https://conductor.thinkmay.net"

// local api
const Dashboard = "/dashboard"
const Initialize = "/initialize"
const Reconnect = "/reconnect"

const Error = "/500"

// thinkmay api
const Login = `${host}/Account/Login`

const FetchSlave = `${host}/User/FetchSlave`
const FetchSession = `${host}/User/FetchSession`

const AddDevice = `${host}/Device/Add`
const RejectDevice = `${host}/Device/Reject/`
const DisconnectDevice = `${host}/Device/Disconnect`

const TerminateSession = `${host}/Session/Terminate`
const DisconnectSession = `${host}/Session/Disconnect`
const ReconnectSession = `${host}/Session/Reconnect`
const InitializeSession = `${host}/Session/Initialize`

const QueryDevice = `${host}/Query/Device`
const QuerySession = `${host}/Query/Session`
const QueryCommand = `${host}/Query/Command`

const genHeaders = () => {
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

const login = body => {
	return fetch(Login, {
		method: "POST",
		headers: genHeaders(),
		body: JSON.stringify({
			userName: body.username,
			password: body.password
		})
	})
}



const fetchSlave = () => {
	return fetch(FetchSlave, {
		method: "GET",
		headers: genHeaders()
	})
}

const fetchSession = () => {
	return fetch(FetchSession, {
		method: "GET",
		headers: genHeaders()
	})
}







const queryDevice = () => {
	return fetch(QueryDevice, {
		method: "GET",
		headers: genHeaders()
	})
}

const querySession = SlaveID => {
	return fetch(QuerySession + "?SlaveID=" + SlaveID, {
		method: "GET",
		headers: genHeaders()
	})
}

const queryCommand = SlaveID => {
	return fetch(QueryCommand + "?SlaveID=" + SlaveID, {
		method: "GET",
		headers: genHeaders()
	})
}






const addDevice = () => {
	return fetch(AddDevice, {
		method: "GET",
		headers: genHeaders()
	})
}

const rejectDevice = SlaveID => {
	return fetch(RejectDevice + "?SlaveID=" + SlaveID, {
		method: "DELETE",
		headers: genHeaders()
	})
}

const disconnectDevice = SlaveID => {
	return fetch(DisconnectDevice + "?SlaveID=" + SlaveID, {
		method: "DELETE",
		headers: genHeaders()
	})
}








const terminateSession = SlaveID => {
	return fetch(TerminateSession + "?SlaveID=" + SlaveID, {
		method: "DELETE",
		headers: genHeaders()
	})
}

const disconnectSession = SlaveID => {
	return fetch(DisconnectSession + "?SlaveID=" + SlaveID, {
		method: "POST",
		headers: genHeaders()
	})
}

const reconnectSession = (SlaveID) => {
	return fetch(ReconnectSession + "?SlaveID=" + SlaveID, {
		method: "POST",
		headers: genHeaders()
	})
}


const initializeSession = (SlaveID) => {
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






