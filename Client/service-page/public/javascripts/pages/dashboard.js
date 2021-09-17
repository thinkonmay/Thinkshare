import * as API from "../util/api.js"
import * as RemotePage from "../util/remote-page-cookies.js"
import * as Setting from "../util/setting.js"
import {getCookie,setCookie} from "../util/cookie.js"


API.getInfor().then(async data => {
	$("#fullName").html((await data.json()).fullName)
})

$(document).ready(async () => {
	var defaultDeviceCap = {
		...Setting.AudioCodec("opus"),
		...Setting.VideoCodec("h264"),
		...Setting.Mode("very high"),
		screenWidth: 2560,
		screenHeight: 1440
	}

	
	setCookie("cap", JSON.stringify(defaultDeviceCap), 999999)
	console.log("set default device capability to "+getCookie("cap"));		
	/// How to convert to JSON 
	/// var cap = getCookie("cap");
	//  var parse = JSON.parse(cap);
	// use "parse" like json 
	// For ex: parse["mode"]

	try {
		const sessions = await (await API.fetchSession()).json()
		const slaves = await (await API.fetchSlave()).json()
		for (const slave of sessions) {
			createSlave(slave,"slavesInUses");
		}
		for (const slave of slaves) {
			createSlave(slave,"availableSlaves");
		}
	} catch (err) {
		alert(err.message)
	}



	var stateSignalR = document.getElementById('state-signalr');
	// Connect to hub signalR with access-token Bearer Authorzation
	const connection = new signalR.HubConnectionBuilder()
		.withUrl(`https://conductor.thinkmay.net/ClientHub`,  {
		accessTokenFactory: () => getCookie("token") // Return access token
	}).build()
	connection.start().then(function () {
		console.log("connected to signalR hub");

		// we use signalR to inform browser 
		// about all state changes event of slave and session
		connection.on("ReportSessionDisconnected", function (slaveId) {
			setState("OFF_REMOTE", slaveId)
		})
		connection.on("ReportSessionReconnected", function (slaveId) {
			setState("ON_SESSION", slaveId);
		})
		connection.on("ReportSessionTerminated", function (slaveId) {
			var slave = document.getElementById(`slavesInUses${slaveId}`);
			slave.remove()
		})
		connection.on("ReportSlaveObtained", function (slaveId) {
			var slave = document.getElementById(`availableSlaves${slaveId}`);
			slave.remove()
		})
		connection.on("ReportSessionInitialized", function (slaveInfor) {
			slaveInfor.serviceState = "ON_SESSION";
			createSlave(slaveInfor,"slavesInUses")
		})
		connection.on("ReportNewSlaveAvailable", function (device) {
			createSlave(device,"availableSlaves")
		})
	}).catch(function (err) {
		location.reload();
	})
})

function 	createSlave(slave,queue) {
	append(queue,  `
    <div class="col-12 col-sm-6 col-md-3 d-flex align-items-stretch flex-column slave" id="${queue}${slave.id}">
      <div class="card bg-light d-flex flex-fill">
        <div style="text-alignt: center" class="card-header text-muted border-bottom-0">
		<img width="20px" height="20px" src="images/window-logo.png" alt="user-avatar" class="img-fluid">
		</div>
        <div class="card-body pt-0">
          <div class="row">
			<h2 class="lead"><b>Device</b></h2>
			<ul class="ml-4 mb-0 fa-ul text-muted">
			<li class="small"><span class="fa-li"><i class="fab fa-windows"></i></span>OS: ${slave.cpu}</li>
			<li class="small"><span class="fa-li"><i class="fab fa-windows"></i></span>OS: ${slave.os}</li>
			<li class="small"><span class="fa-li"><i class="fas fa-memory"></i></span>RAM: ${Math.round(slave.raMcapacity / 1024)}GB</li>
			<li class="small"><span class="fa-li"><i class="fas fa-tv"></i></span>GPU: ${slave.gpu}</li>
			</ul>
          </div>
        </div>
        <div class="overlay">
          <div class="row slaveState" id="button${slave.id}"></div>
        </div>
      </div>
    </div>`)
	setState(slave.serviceState,slave.id);
}


function setState(serviceState, slaveID){
	var button = document.getElementById(`button${slaveID}`);
	button.innerHTML = slaveState(slave.serviceState,slave.id);

	if (serviceState === "ON_SESSION"){
		var initbutt = document.getElementById(`disconnect${slaveID}`)
		initbutt.addEventListener("click", async function () {
			await API.disconnectSession(slave.id)
		});
		var terminatebutt = document.getElementById(`terminate${slaveID}`)
		terminatebutt.addEventListener("click", async function () {
			await API.terminateSession(slave.id)
		});
	}
	if (serviceState === "OFF_REMOTE"){
		var recbutt = document.getElementById(`reconnect${slaveID}`)
		recbutt.addEventListener("click",  async function () {
			RemotePage.sessionReconnect(slave.id)
		});
		var terminatebutt = document.getElementById(`terminate${slaveID}`)
		terminatebutt.addEventListener("click", async function () {
			await API.terminateSession(slave.id)
		});;
	}
	if (serviceState === null){
		var connbutt = document.getElementById(`connect${slaveID}`)
		connbutt.addEventListener("click",  async function () {
			RemotePage.sessionInitialize(slave.id)
		});
	}
}

function slaveState(state,slaveId) {
	const nl = '<div class="w-100"></div>'
	const btn = {
		connect:    `<button type="button" class="btn btn-primary btn-icon-text" id="connect${slaveId}"><i class="ti-file btn-icon-prepend"></i>Connect</button></div>`,
		disconnect: `<button type="button" class="btn btn-warning btn-icon-text" id="disconnect${slaveId}"><i class="ti-reload btn-icon-prepend"></i>Disconnect</button>`,
		reconnect:  `<button type="button" class="btn btn-warning btn-icon-text" id="reconnect${slaveId}"><i class="ti-reload btn-icon-prepend"></i>Reconnect</button>`,
		terminate:  `<button type="button" class="btn btn-outline-danger btn-icon-text" id="terminate${slaveId}"><i class="ti-upload btn-icon-prepend"></i>Terminate</button>`
	}
	if (state === "ON_SESSION"){
		return btn.disconnect + btn.terminate
	}
	if (state === "OFF_REMOTE"){
		return btn.reconnect + btn.terminate
	}
	if (state === "DEVICE_DISCONNECTED"){
		return ""
	}
	if (state === null){
		return btn.connect
	}
}

function append(id, html) {
	$(`#${id}`).append(html)
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
