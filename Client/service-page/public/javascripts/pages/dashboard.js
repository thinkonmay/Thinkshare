import * as API from "../util/api.js"
import {getCookie} from "../util/cookie.js"

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
		window.open(getInitURL(SlaveID), "__blank")
		const slave = getParent(this)
		const clone = slave.clone()
		slave.remove()
		clone.children().children().children(".slaveState").html(slaveState(ONSESSION))
		$("#onlineSlaves").append(clone)
	})
	$(document).on("click", '.overlay :input[name="disconnect"]', async function () {
		const SlaveID = getSlaveID(this)
		await API.disconnectDevice(SlaveID)
		const slave = getParent(this)
		const clone = slave.clone()
		slave.remove()
		clone.children().children().children(".slaveState").html(slaveState(DISCONNECT))
		$("#disconnectSlaves").append(clone)
	})
	$(document).on("click", '.overlay :input[name="reconnect"]', async function () {
		const SlaveID = getSlaveID(this)
		window.open(getReconnectURL(SlaveID, true), "__blank")
	})
	$(document).on("click", '.overlay :input[name="terminate"]', async function () {
		const SlaveID = getSlaveID(this)
		await API.terminateSession(SlaveID)
		const slave = getParent(this)
		const clone = slave.clone()
		slave.remove()
		clone.children().children().children(".slaveState").html(slaveState(AVAILABLE))
		$("#availableSlaves").append(clone)
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
			append("onlineSlaves", createSlave(slave, ONSESSION))
		}
		for (const slave of slaves) {
			append("availableSlaves", createSlave(slave, AVAILABLE))
		}
	} catch (err) {
		alert(err.message)
	}
	// Connect to hub signalR with access-token Bearer Authorzation
	const connection = new signalR.HubConnectionBuilder().withUrl(`http://conductor.thinkmay.net/AdminHub`,  {
		accessTokenFactory: () => getCookie("token") // Return access token
	}).build()
	//Disable send button until connection is established
	document.getElementById("sendButton").disabled = true

	connection.on("ReceiveMessage", function (user, message) {
		var li = document.createElement("li")
		document.getElementById("messagesList").appendChild(li)
		// We can assign user-supplied strings to an element's textContent because it
		// is not interpreted as markup. If you're assigning in any other way, you 
		// should be aware of possible script injection concerns.
		li.textContent = `${user} says ${message}`
	})

	connection.start().then(function () {
		document.getElementById("sendButton").disabled = false
	}).catch(function (err) {
		return console.error(err.toString())
	})

	document.getElementById("sendButton").addEventListener("click", function (event) {
		var user = document.getElementById("userInput").value
		var message = document.getElementById("messageInput").value
		connection.invoke("SendMessage", user, message).catch(function (err) {
			return console.error(err.toString())
		})
		event.preventDefault()
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

function createSlave(slave, state) {
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
                <li class="small"><span class="fa-li"><i class="fab fa-windows"></i></span>OS: ${
	slave.os
}</li>
                <li class="small"><span class="fa-li"><i class="fas fa-memory"></i></span>RAM: ${Math.round(
		slave.raMcapacity / 1024
	)}GB</li>
                <li class="small"><span class="fa-li"><i class="fas fa-tv"></i></span>GPU: ${
	slave.gpu
}</li>
              </ul>
            </div>
            <div class="col-5 text-center">
              <img src="images/avtFounder.png" alt="user-avatar" class="img-circle img-fluid">
            </div>
          </div>
        </div>
        <div class="overlay">
          <div class="row slaveState">
            ${slaveState(state)}
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
		terminate: '<div class="col colbut"><input class="btn btn-warning" name="terminate" type="submit" value="Terminate"></div>',
		reject: '<div class="col colbut"><input class="btn btn-danger" name="reject" type="submit" value="Reject"></div>'
	}
	if (window.isAdmin) {
		if (state == AVAILABLE)
			return btn.connect + btn.disconnect + nl + btn.reject
		if (state == DISCONNECT)
			return btn.connect + btn.reject
		return btn.reconnect + btn.disconnect + nl + btn.reject + btn.terminate
	} else {
		if (state == AVAILABLE)
			return btn.connect
		return btn.terminate + btn.reconnect
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
	default:
		return {
			audioCodec: 0,
			videoCodec: 0
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
