import { createSlave } from "./workerComponent.js";
import { getCookie } from "./cookie.js";
import * as API from "./api.js"

export async function connectToClientHub() {
	const Websocket = new WebSocket( await API.getUserHub(getCookie("token")));
	Websocket.addEventListener('open', onWebsocketOpen);
	Websocket.addEventListener('message', onClientHubEvent);
	Websocket.addEventListener('error', onWebsocketClose);
	Websocket.addEventListener('close', onWebsocketClose);

}

function onClientHubEvent(event) {
	try {
		if (event.data === "ping") {
			console.log("ping host successful")
			return;
		}
		var message_json = JSON.parse(event.data);
	} catch (e) {
		console.log("Error parsing incoming JSON: " + event.data);
		return;
	}

	if (message_json.EventName === "ReportSessionDisconnected") {
		var workerID = parseInt(message_json.Message)

		createSlave(workerID, "OFF_REMOTE", "slavesInUses");
	}
	if (message_json.EventName === "ReportSessionOn") {
		var workerID = parseInt(message_json.Message)

		RemotePage.check_remote_condition(workerID, null, null);
		createSlave(workerID, "ON_SESSION", "slavesInUses")
	}
	if (message_json.EventName === "ReportSessionTerminated") {
		var workerID = parseInt(message_json.Message)

		createSlave(workerID, null, "slavesInUses");
	}

	if (message_json.EventName === "ReportNewSlaveAvailable") {
		var workerID = parseInt(message_json.Message)

		createSlave(workerID, "DEVICE_OPEN", "availableSlaves")
	}
	if (message_json.EventName === "ReportSlaveObtained") {
		var workerID = parseInt(message_json.Message)

		createSlave(workerID, null, "availableSlaves");
	}
}

function onWebsocketOpen() {
	console.log("connected to client hub");
}

function onWebsocketClose() {
	(new Promise(resolve => setTimeout(resolve, 5000)))
		.then(() => {
			location.reload();
		});
};