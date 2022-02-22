/**
 * * In this component, I will:
 * TODO: Hanlde workerNode
 * ? - Create WorkerNode
 * ? - Set State WorkerNode
 * ? - Handle Button with State of WorkerNode
 */

import * as API from "./api.js"
import * as Utils from "./utils.js"
import * as RemotePage from "./remote.js"
import { append } from "./utils.js";

/**
 * TODO: Generate UI of WorkerNode with parameter provider
 * @param {int} workerID 
 * @param {String} workerState 
 * @param {String} queue 
 */
export async function
    createSlave(workerID, workerState, queue) {
    try {
        var slave = await (await API.fetchInfor(workerID)).json();
        var worker = document.getElementById(`${queue}${workerID}`);
        if (workerState == null)
            worker.remove();

        if (worker == null) {
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
					<li class="small"><span class="fa-li"><i class="fas fa-memory"></i></span>RAM: ${Math.round(slave.raMcapacity / 1000)}GB</li>
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

/**
 * TODO: SetState of WorkerNode
 * @param {String} serviceState 
 * @param {int} slaveID 
 * @param {String} queue 
 */
function
    setState(serviceState, slaveID, queue) {
    var button = document.getElementById(`${queue}button${slaveID}`);
    button.innerHTML = slaveState(serviceState, slaveID);

    if (serviceState === "ON_SESSION") {
        var disconnectButton = document.getElementById(`disconnect${slaveID}`)
        disconnectButton.addEventListener("click", async function () {
            Utils.newSwal.fire({
                title: "Processing",
                text: "Wait a minute . . .",
                didOpen: async () => {
                    Swal.showLoading()
                    await API.disconnectSession(slaveID)
                    Utils.newSwal.fire({
                        title: "Success",
                        icon: "success",
                        showConfirmButton: false,
                        timer: 1500
                    })
                }
            })
        });

        var terminateButton = document.getElementById(`terminate${slaveID}`)
        terminateButton.addEventListener("click", async function () {
            Utils.newSwal.fire({
                title: "Processing",
                text: "Wait a minute . . .",
                didOpen: async () => {
                    Swal.showLoading()
                    await API.terminateSession(slaveID)
                    Utils.newSwal.fire({
                        title: "Success",
                        icon: "success",
                        showConfirmButton: false,
                        timer: 1500
                    })
                }
            })
        });
    }
    if (serviceState === "OFF_REMOTE") {
        var reconnectButton = document.getElementById(`reconnect${slaveID}`)
        reconnectButton.addEventListener("click", async function () {
            Utils.newSwal.fire({
                title: "Processing",
                text: "Wait a minute . . .",
                didOpen: async () => {
                    Swal.showLoading()
                    var setting = await RemotePage.setupDevice();
                    let token =   await (await API.reconnectSession(slaveID)).json();
                    await RemotePage.getRemotePage(token.token, setting.engine)
                    Utils.newSwal.fire({
                        title: "Success",
                        icon: "success",
                        showConfirmButton: false,
                        timer: 1500
                    })
                }
            })
        });
        var terminateButton = document.getElementById(`terminate${slaveID}`)
        terminateButton.addEventListener("click", async function () {
            Utils.newSwal.fire({
                title: "Processing",
                text: "Wait a minute . . .",
                didOpen: async () => {
                    Swal.showLoading()
                    await API.terminateSession(slaveID)
                    Utils.newSwal.fire({
                        title: "Success",
                        icon: "success",
                        showConfirmButton: false,
                        timer: 1500
                    })
                }
            })
        });
    }
    if (serviceState === "DEVICE_OPEN") {
        var connectButton = document.getElementById(`connect${slaveID}`)
        connectButton.addEventListener("click", async function () {
            Utils.newSwal.fire({
                title: "Processing",
                text: "Wait a minute . . .",
                didOpen: async () => {
                    Swal.showLoading()
                    var setting = await RemotePage.setupDevice();
                    let token =   await (await API.initializeSession(slaveID)).json();
                    await RemotePage.getRemotePage(token.token, setting.engine)
                    Utils.newSwal.fire({
                        title: "Success",
                        icon: "success",
                        showConfirmButton: false,
                        timer: 1500
                    })
                }
            })
        });
    }
}

/**
 * TODO: Generate UI for Button Session
 * @param {String} state 
 * @param {int} slaveId 
 * @returns HTML btn of state
 */
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
