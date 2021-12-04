const host =  process.env.CLUSTER_MANAGER_URL







//////////////////////////////////////////////// 
// owner api
const LoginRoute = `${host}/Owner/Login`
const RegisterClusterRoute = `${host}/Owner/Register`
const GetClusterTokenRoute = `${host}/Owner/Cluster/Token`
const SetTurnRoute = `${host}/Owner/Cluster/TURN`
const StartRoute = `${host}/Owner/Start`
const StopRoute = `${host}/Owner/Stop`
//////////////////////////////////////////////// 


export const genHeaders = (token) => {
	return Object.assign(
        { "Content-Type": "application/json" }, 
        { Authorization: `${token}` } 
	)
}

/////////////////////////////////////////
// user api 
export const Login = (body) => {
	return fetch(LoginRoute, {
		method: "POST",
		body: JSON.stringify({
			userName: body.username,
			password: body.password
		})
	})
}

export const RegisterCluster = (token,isPrivate, Name) => {
	if (isPrivate) {
		RegisterClusterRoute += `?isPrivate=true&ClusterName=${Name}`
	} else {
		RegisterClusterRoute += `?isPrivate=false&ClusterName=${Name}`
	}
	return fetch(RegisterClusterRoute, {
		method: "POST",
		headers: genHeaders(token)
	})
}

export const GetClusterToken = (token) => {
	return fetch(GetClusterTokenRoute, {
		method: "GET",
		headers: genHeaders(token),
	})
}

export const SetTurn = (ip, username, password, token) => {
	return fetch(SetTurnRoute + `?IP=${ip}&user=${username}&password${password}`, {
		method: 'POST',
		headers: genHeaders(token),
	})
}

export const Start = (token) => {
	return fetch(StartRoute, {
		method: "POST",
		headers: genHeaders(token),
	})
}


export const Stop = (token) => {
	return fetch(StopRoute, {
		method: "POST",
		headers: genHeaders(token),
	})
}