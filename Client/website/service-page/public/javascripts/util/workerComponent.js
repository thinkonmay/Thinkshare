import * as API from "./api.js"
import { append } from "./utils.js";

export async function 
createSlave(workerID, workerState, queue) 
{
	try 
	{
		var slave = await (await API.fetchInfor(workerID)).json();
		var worker = document.getElementById(`${queue}${workerID}`);
		if(workerState == null)
			worker.remove();

		if(worker == null)
		{
			append(queue, `
			<div class="col-12 col-sm-6 col-md-3 d-flex align-items-stretch flex-column slave" id="${queue}${workerID}">
			<div class="card bg-light d-flex flex-fill">
				<div style="text-alignt: center" class="card-header text-muted border-bottom-0">
				<img width="20px" height="20px" src="images/window-logo.png" alt="user-avatar" class="img-fluid">
				</div>
				<div class="card-body pt-0">
				<div class="row">
					<h2 class="lead"><b>Device</b></h2>
					<ul class="ml-4 mb-0 fa-ul text-muted">
					<li class="small"><span class="fa-li"><i class="fab fa-windows"></i></span>CPU: ${slave.cpu}</li>
					<li class="small"><span class="fa-li"><i class="fab fa-windows"></i></span>OS: ${slave.os}</li>
					<li class="small"><span class="fa-li"><i class="fas fa-memory"></i></span>RAM: ${Math.round(slave.raMcapacity / 1024)}GB</li>
					<li class="small"><span class="fa-li"><i class="fas fa-tv"></i></span>GPU: ${slave.gpu}</li>
					</ul>
				</div>
				</div>
				<div class="devicebutton">
				<div class="row slaveState" id="${queue}button${slave.id}"></div>
				</div>
			</div>
			</div>`)
		}

		setState(workerState, slave.id, queue);
	} catch (error) {
		
	}
}


function 
setState(serviceState, slaveID, queue) {
	var button = document.getElementById(`${queue}button${slaveID}`);
	button.innerHTML = slaveState(serviceState, slaveID);

	// ??? why?
	// if (serviceState == "OFF_REMOTE") {
	// 	Utils.newSwal.fire({
	// 		title: "Successfully!",
	// 		text: "",
	// 		icon: "info",
	// 		showConfirmButton: false,
	// 		timer: 1500
	// 	})
	// }

	if (serviceState === "ON_SESSION") {

		var initbtn = document.getElementById(`disconnect${slaveID}`)
		initbtn.addEventListener("click", async function () {
			await API.disconnectSession(slaveID)
		});
		var terminatebtn = document.getElementById(`terminate${slaveID}`)
		terminatebtn.addEventListener("click", async function () {
			await API.terminateSession(slaveID)
		});
	}
	if (serviceState === "OFF_REMOTE") {
		var recbtn = document.getElementById(`reconnect${slaveID}`)
		recbtn.addEventListener("click", async function () {
			Utils.newSwal.fire({
				title: "Processing",
				text: "Wait a minute . . .",
				didOpen: () => {
					Swal.showLoading()
					RemotePage.sessionReconnect(slaveID)
				}
			})
		});
		var terminatebtn = document.getElementById(`terminate${slaveID}`)
		terminatebtn.addEventListener("click", async function () {
			await API.terminateSession(slaveID)
		});;
	}
	if (serviceState === "DEVICE_OPEN") {
		var connectbtn = document.getElementById(`connect${slaveID}`)
		connectbtn.addEventListener("click", async function () {
			Utils.newSwal.fire({
				title: "Processing",
				text: "Wait a minute . . .",
				didOpen: () => {
					Swal.showLoading()
					RemotePage.sessionInitialize(slaveID)
				}
			})
		});
	}
}

function 
slaveState(state, slaveId) {
	const nl = '<div class="w-100"></div>'
	const btn = {
		connect: `<button type="button" class="btn btn-info btn-icon-text" id="connect${slaveId}"><i class="ti-file btn-icon-prepend"></i>Connect</button></div>`,
		disconnect: `<button type="button" class="btn btn-outline-warning btn-icon-text" id="disconnect${slaveId}"><i class="ti-reload btn-icon-prepend"></i>Disconnect</button>`,
		reconnect: `<button type="button" class="btn btn-outline-warning btn-icon-text" id="reconnect${slaveId}"><i class="ti-reload btn-icon-prepend"></i>Reconnect</button>`,
		terminate: `<button type="button" class="btn btn-danger btn-icon-text" id="terminate${slaveId}"><i class="ti-upload btn-icon-prepend"></i>Terminate</button>`
	}
	if (state === "ON_SESSION") {
		return btn.disconnect + btn.terminate
	}
	if (state === "OFF_REMOTE") {
		return btn.reconnect + btn.terminate
	}
	if (state === "DEVICE_DISCONNECTED") {
		return ""
	}
	if (state === "DEVICE_OPEN") {
		return btn.connect
	}
}
