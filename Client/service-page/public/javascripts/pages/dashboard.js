import * as API from "../util/api.js"
import * as RemotePage from "./remote-page.js"


const AVAILABLE = 1 << 1
const ONSESSION = 1 << 2
const DISCONNECT = 1 << 3

API.getInfor().then(async data => {
	$("#fullName").html((await data.json()).fullName)
})

$(document).ready(async () => {
	$("[data-toggle=\"tooltip\"]").tooltip()
	$("#addslave").click(() => {
		Swal.fire({
			title: "Submit your slave ID",
			input: "text",
			showCancelButton: true,
			confirmButtonText: "Add",
			showLoaderOnConfirm: true,
			preConfirm: id => {
				// fetch goes here
				return id
			},
			allowOutsideClick: () => !Swal.isLoading()
		}).then(result => {
			if (result.isConfirmed) {
				Swal.fire({
					title: `Added slave ID ${result.value}`
				})
			}
		})
	})

	const getParent = input => $($(input).parent().parent().parent().parent().parent())
	const getSlaveID = input => getParent(input).attr("id")

	$(document).on("click", '.overlay :input[name="connect"]', async function () {
		const SlaveID = getSlaveID(this)
		API.Reconnect(SlaveID).then( (response) => {
			if(response.status === 200){
				remote_page = window.open()
				remote_page.innerHTML = response.body
			}
		})
	})
	$(document).on("click", '.overlay :input[name="disconnect"]', async function () {
		const SlaveID = getSlaveID(this)
		await API.disconnectDevice(SlaveID)
	})
	$(document).on("click", '.overlay :input[name="reconnect"]', async function () {
		const SlaveID = getSlaveID(this)
		window.open(getReconnectURL(SlaveID, true), "__blank")
	})
	$(document).on("click", '.overlay :input[name="terminate"]', async function () {
		const SlaveID = getSlaveID(this)
		await API.terminateSession(SlaveID)
	})
	$(document).on("click", '.overlay :input[name="reject"]', async function () {
		const SlaveID = getSlaveID(this)
		await API.rejectDevice(SlaveID)
		getParent(this).remove()
	})

	try {
		const sessions = await (await API.fetchSession()).json()
		const slaves = await (await API.fetchSlave()).json()
		for (const slave of sessions) {
			append("onlineSlaves", createSlave(slave))
		}
		for (const slave of slaves) {
			append("availableSlaves", createSlave(slave))
		}
	} catch (err) {
		alert(err.message)
	}
	var stateSignalR = document.getElementById('state-signalr');
	// Connect to hub signalR with access-token Bearer Authorzation
	const connection = new signalR.HubConnectionBuilder().withUrl(`http://localhost:5000/ClientHub`,  {
		accessTokenFactory: () => getCookie("token") // Return access token
	}).build()
	connection.start().then(function () {
		document.getElementById("sendButton").disabled = false
	}).catch(function (err) {
		return console.error(err.toString())
	})

	// receive from function have been trigger on signalr
	connection.on("ReportSlaveObtained", function (slaveId) {
		slave = document.getElementById(slaveId)
		slave.remove()
	})
	connection.on("ReportNewSlaveAvailable", function (device) {
		append("#availableSlaves",createSlave(device))
	})
	connection.on("ReportSessionDisconnected", function (slaveId) {
		button = slave.getElementById(`button${slaveId}`)
		button.innerHTML = slaveState("OFF_REMOTE")
	})
	connection.on("ReportSessionReconnected", function (slaveId) {
		button = slave.getElementById(`button${slaveId}`)
		button.innerHTML = slaveState("ON_SESSION")
	})
	
	//trigger function on signalR
	document.getElementById("triggerButton").addEventListener("click", function (event) {
		connection.invoke("trigger", "hello").catch(function (err) {
			return console.log(err)
		})
	})
})

function getInitURL(SlaveID) {
	return `${API.Initialize}?${serialize({
		SlaveID,
		cap: {
			...Quality("best"),
			mode: 1,
			screenWidth: window.innerWidth,
			screenHeight: window.innerHeight
		}
	})}`
}

function getReconnectURL(SlaveID) {
	return `${API.Reconnect}?SlaveID=${SlaveID}`
}

function createSlave(slave) {
	return `
    <div class="col-12 col-sm-6 col-md-3 d-flex align-items-stretch flex-column slave" id="${slave.id}">
      <div class="card bg-light d-flex flex-fill">
        <div class="card-header text-muted border-bottom-0">
          <br>
        </div>
        <div class="card-body pt-0">
          <div class="row">
            <div class="col-7">
              <h2 class="lead"><b>${slave.cpu}</b></h2>
              <ul class="ml-4 mb-0 fa-ul text-muted">
                <li class="small"><span class="fa-li"><i class="fab fa-windows"></i></span>OS: ${slave.os}</li>
                <li class="small"><span class="fa-li"><i class="fas fa-memory"></i></span>RAM: ${Math.round(slave.raMcapacity / 1024)}GB</li>
                <li class="small"><span class="fa-li"><i class="fas fa-tv"></i></span>GPU: ${slave.gpu}</li>
              </ul>
            </div>
            <div class="col-5 text-center">
              <img src="images/avtFounder.png" alt="user-avatar" class="img-circle img-fluid">
            </div>
          </div>
        </div>
        <div class="overlay">
          <div class="row slaveState" id="button${slave.id}">
            ${slaveState(slave.serviceState)}
          </div>
        </div>
      </div>
    </div>`
}

function slaveState(state) {
	const nl = '<div class="w-100"></div>'
	const btn = {
		connect: '<div class="col colbut"><input class="btn btn-primary" name="connect" type="submit" value="Connect"></div>',
		disconnect: '<div class="col colbut"><input class="btn btn-secondary" name="disconnect" type="submit" value="Disconnect"></div>',
		reconnect: '<div class="col colbut"><input class="btn btn-primary" name="reconnect" type="submit" value="Reconnect"></div>',
		terminate: '<div class="col colbut"><input class="btn btn-warning" name="terminate" type="submit" value="Terminate"></div>'
	}
	if (state === "ON_SESSION"){
		return btn.disconnect + btn.terminate
	}
	if (state === "OFF_REMOTE"){
		return btn.reconnect + btn.terminate
	}
	if (state === "DEVICE_OPEN"){
		return btn.connect
	}
}

function append(id, html) {
	$(`#${id}`).append(html)
}

function Quality(qual) {
	switch (qual) {
	case "best":
		return {
			audioCodec: 4,
			videoCodec: 4
		}
	case "good":
		return {
			audioCodec: 2,
			videoCodec: 2
		}
	case "best":
		return {
			audioCodec: 4,
			videoCodec: 4
		}
	case "good":
		return {
			audioCodec: 2,
			videoCodec: 2
		}
	
	case "best":
		return {
			audioCodec: 4,
			videoCodec: 4
		}
	case "good":
		return {
			audioCodec: 2,
			videoCodec: 2
		}
	}
}

function serialize(obj, prefix) {
	var str = [],
		p
	for (p in obj) {
		if (obj.hasOwnProperty(p)) {
			var k = prefix ? prefix + "[" + p + "]" : p,
				v = obj[p]
			str.push(
				v !== null && typeof v === "object" ?
				serialize(v, k) :
				encodeURIComponent(k) + "=" + encodeURIComponent(v)
			)
		}
	}
	return str.join("&")
}

async function rejectDevice(sessionClientId) {
	const data = await API.rejectDevice(sessionClientId)
	console.log(await data.text())
}
