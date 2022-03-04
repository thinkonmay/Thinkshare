/**
 * * In this componet, I will:
 * ? - Setup connect to ClientHub (open, message, error, close)
 * ? - In Case Websocket Close => reload page
 * ? - Websocket will receive signal of session (connect, disconnect, terminate, reconnect, newDeviceAvailable, newDeviceObtained)
*/
import { createWorkerBlock } from "./workerComponent.js";
import { getCookie } from "./cookie.js";
import * as API from "./api.js"

export async function connectToClientHub() {
	const Websocket = new WebSocket( await API.getUserHub(getCookie("token")));
	Websocket.addEventListener('open', onWebsocketOpen);
	Websocket.addEventListener('message', onClientHubEvent);
	Websocket.addEventListener('error', onWebsocketClose);
	Websocket.addEventListener('close', onWebsocketClose);
}

async function onClientHubEvent(event) {
	if (event.data === "ping") 
		return;

	var message = JSON.parse(event.data);
	var WorkerID = parseInt(message.Message)

	var queue = null;
	var WorkerState = null;
	if (message.EventName === "ReportSessionDisconnected") {
		WorkerState = "OFF_REMOTE";
		queue = "sessionInUse";
	}
	if (message.EventName === "ReportSessionOn") {
		WorkerState = "ON_SESSION";
		queue = "sessionInUse";
	}
	if (message.EventName === "ReportSessionTerminated") {
		queue = "sessionInUse";
	}

	if (message.EventName === "ReportNewSlaveAvailable") {
		WorkerState = "DEVICE_OPEN";
		queue = "availableWorkers";
	}
	if (message.EventName === "ReportSlaveObtained") {
		queue = "availableWorkers";
	}

	await createWorkerBlock(WorkerID,WorkerState,queue);
}

function onWebsocketOpen() {
	console.log("connected to client hub");
}

function onWebsocketClose() {
	(new Promise(resolve => setTimeout(resolve, 5000))).then(() => { location.reload(); });
};