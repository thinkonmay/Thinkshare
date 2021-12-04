import * as API from "../util/api.js"
import * as RemotePage from "../util/remote-page-cookies.js"
import { getCookie, setCookie, deleteCookie } from "../util/cookie.js"
import * as Utils from "../util/utils.js"
import * as CheckDevice from "../util/checkdevice.js"

let datasetCPU = [];
let datasetGPU = [];
let datasetRAM = [];
let datasetNetwork = [];

let sessionInfor;
// API.getInfor().then(async data => {
// 	let body = await data.json();
// 	$("#dashboardSrcImg").attr("src", (body.avatar) == null ? "images/default_user.png" : body.avatar)
// 	$("#WelcomeUsername").html(body.fullName)
// 	$("#avatarSrc").attr("src", (body.avatar) == null ? "images/default_user.png" : body.avatar)
// 	$("#fullname").text(body.fullName)
// 	$("#jobs").text(body.jobs)
// })
$(document).ready(async () => {

	$('#registerCluster').click(() => {
		window.location = "/register-cluster"
	})

	$('#detailBtn').click(() => {
		// pop up module details infor user
	})

	$('#logout').click(() => {
		setCookie("logout", "true")
		setCookie("token", null, 1)

		deleteCookie("token", "/", document.domain)

		try {
			gapi.auth.signOut();
			window.location = "/login"
		} catch {
			window.location = "/login"
		}
	})


	tutorial()

	if (CheckDevice.isElectron()) {
		// desktop app
	} else {
		// website
	}

	if (CheckDevice.isWindows()) {
		// Windows
	} else if (CheckDevice.isMacintosh()) {
		// Macintosh (MacOS)
	}

	try {
		// const userinfor = await (await API.getInfor()).json()
		// const sessions = await (await API.fetchSession()).json()
		// const slaves = await (await API.fetchSlave()).json()
		// sessionInfor = await (await API.getSession()).json()
		document.getElementById("WelcomeUsername").innerHTML = userinfor.fullName;

		for (const slave of sessions) {
			createSlave(slave, "slavesInUses");
		}
		for (const slave of slaves) {
			createSlave(slave, "availableSlaves");
		}
	} catch (err) {
		// location.reload();
	}
	setDataForChart();
	$('#analyticCPU').click(() => {
		datasetNetwork = [11, 51, 22, 40, 95, 43, 2, 30, 1, 40, 95, 1, 16, 14, 49, 21, 29, 15, 43, 2, 30, 1, 40, 95, 1, 16, 54, 61, 25, 95, 43, 2, 30, 1, 40, 95, 1, 16, 1, 1, 40, 95, 1, 32, 5, 32, 40, 95, 43, 2, 30, 1, 40, 95, 1, 16, 14, 49, 21, 29, 45, 61, 25, 3, 1]
		setDataForChart('#1F3BB3');
	});

	$('#analyticGPU').click(() => {
		datasetNetwork = [11, 1, 40, 95, 1, 16, 14, 49, 21, 29, 15, 43, 2, 30, 1, 40, 11, 51, 22, 40, 95, 43, 51, 22, 40, 95, 43, 2, 30, 1, 40, 95, 1, 16, 14, 49, 21, 29, 15, 43, 2, 30, 1, 40, 95, 1, 16, 54, 61, 25, 95, 43, 2, 30, 1, 40, 95, 1, 16, 1, 1, 40, 95, 1, 32, 5, 32, 40, 95, 43, 2, 30, 1, 40, 95, 1, 16, 14, 49, 21, 29, 45, 61, 25, 3, 1]
		setDataForChart('#52CDFF');
	})
	$('#analyticRAM').click(() => {
		datasetNetwork = [11, 51, 22, 40, 95, 43, 2, 30, , 2, 30, 1, 40, 95, 1, 16, 14, 49, 21, , 95, 1, 16, 54, 61, 25, 95, 43, 2, 30, 1, 40, 95, 1, 16, 1, 1, 40, 95, 1, 32, 5, 32, 40, 95, 43, 2, 30, 1, 40, 95, 1, 16, 14, 49, 21, 29, 45, 61, 25, 3, 1]
		setDataForChart('#eded68');
	})
	$('#analyticNetwork').click(() => {
		datasetNetwork = [29, 15, 43, 2, 30, 1, 40, 95, 1, 16, 54, 61, 25, 95, 43, 2, 30, 1, 40, 95, 1, 16, 1, 1, 40, 95, 1, 32, 5, 32, 40, 95, 43, 2, 30, 1, 40, 95, 1, 16, 14, 49, 21, 29, 45, 61, 25, 3, 1]
		setDataForChart('#e65555');
	})



	// using websocket to connect to systemhub
	// const Websocket = new WebSocket(API.UserHub + `?token=${getCookie("token")}`)
	// Websocket.addEventListener('open', onWebsocketOpen);
	// Websocket.addEventListener('message', onClientHubEvent);
	// Websocket.addEventListener('error', onWebsocketClose);
	// Websocket.addEventListener('close', onWebsocketClose);
})



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
		var slaveId = message_json.Message
		setState("OFF_REMOTE", slaveId)
	}
	if (message_json.EventName === "ReportSessionReconnected") {
		var slaveId = message_json.Message
		setState("ON_SESSION", slaveId);
	}
	if (message_json.EventName === "ReportSessionTerminated") {
		var slaveId = message_json.Message
		var slave = document.getElementById(`slavesInUses${slaveId}`);
		slave.remove()
	}
	if (message_json.EventName === "ReportSlaveObtained") {
		var slaveId = message_json.Message
		var slave = document.getElementById(`availableSlaves${slaveId}`);
		slave.remove()
	}
	if (message_json.EventName === "ReportSessionInitialized") {
		var device = JSON.parse(message_json.Message)
		device.os = device.OS;
		device.raMcapacity = device.RAMcapacity;
		device.gpu = device.GPU;
		device.id = device.ID;
		device.cpu = device.CPU;
		device.serviceState = "ON_SESSION";
		createSlave(device, "slavesInUses")
	}
	if (message_json.EventName === "ReportNewSlaveAvailable") {
		var device = JSON.parse(message_json.Message)
		device.os = device.OS;
		device.raMcapacity = device.RAMcapacity;
		device.gpu = device.GPU;
		device.id = device.ID;
		device.cpu = device.CPU;
		createSlave(device, "availableSlaves")
	}
}

function onWebsocketOpen() {
	console.log("connected to client hub");
}
function onWebsocketClose(event) {
	// location.reload();
};

function createSlave(slave, queue) {
	append(queue, `
    <div class="col-12 col-sm-6 col-md-3 d-flex align-items-stretch flex-column slave" id="${queue}${slave.id}">
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
          <div class="row slaveState" id="button${slave.id}"></div>
        </div>
      </div>
    </div>`)
	setState(slave.serviceState, slave.id);
}


function setState(serviceState, slaveID) {
	var button = document.getElementById(`button${slaveID}`);
	button.innerHTML = slaveState(serviceState, slaveID);

	// if (serviceState === "ON_SESSION") {
	// 	var initbutt = document.getElementById(`disconnect${slaveID}`)
	// 	initbutt.addEventListener("click", async function () {
	// 		await API.disconnectSession(slaveID)
	// 	});
	// 	var terminatebutt = document.getElementById(`terminate${slaveID}`)
	// 	terminatebutt.addEventListener("click", async function () {
	// 		await API.terminateSession(slaveID)
	// 	});
	// }
	// if (serviceState === "OFF_REMOTE") {
	// 	var recbutt = document.getElementById(`reconnect${slaveID}`)
	// 	recbutt.addEventListener("click", async function () {
	// 		RemotePage.sessionReconnect(slaveID)
	// 	});
	// 	var terminatebutt = document.getElementById(`terminate${slaveID}`)
	// 	terminatebutt.addEventListener("click", async function () {
	// 		await API.terminateSession(slaveID)
	// 	});;
	// }
	if (serviceState === "DEVICE_OPEN") {
		var connbutt = document.getElementById(`connect${slaveID}`)
		connbutt.addEventListener("click", async function () {
			RemotePage.sessionInitialize(slaveID)
		});
	}
}

function slaveState(state, slaveId) {
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

function popUpTurorial(id, name_shortcut, excute_shortcut, src_shortcut) {
	$(`${id}`).click(function (e) {
		$("#name_shorcut").text(name_shortcut);
		$("#excute_shortcut").text(excute_shortcut)
		document.getElementById("videoHiddenMouse").src = `/videos/${src_shortcut}.gif`
		$('.popup-wrap').fadeIn(500);
		$('.popup-box').removeClass('transform-out').addClass('transform-in');
		var vid = document.getElementById("videoHiddenMouse");
		vid.autoplay = true;
		vid.load();

		e.preventDefault();
	});

}

async function tutorial() {

	$('#tutorialButton').click(() => {
		$('#content').show()
	})

	$('#exitButton').click(() => {
		$('#content').hide()
	})

	await popUpTurorial('#hiddenMouse', 'Hidden Mouse', 'Ctrl + Shift + P', 'Hidden_Mouse_x2.5')
	await popUpTurorial('#fullScreen', 'Full Screen', 'Ctrl + Shift + F', 'Full_Screen_x2.5')


	$('.popup-close').click(function (e) {
		$('.popup-wrap').fadeOut(500);
		$('.popup-box').removeClass('transform-in').addClass('transform-out');

		e.preventDefault();
	});
}


function setDataForChart(color) {
	let isSetElement = false;
	// for (let i = 0; i < 7; i++) {
	// 	datasets[i] = 0;
	// }
	// for (let i = 0; i < sessionInfor.length; i++) {
	// 	datasets[sessionInfor[i].dayofWeek] = sessionInfor[i].sessionTime;
	// }
	let _lables = [];
	for (let index = 0; index <= 60; index++) {
		_lables.unshift(index);
	}
	if ($("#performaneLine").length) {
		var graphGradient = document.getElementById("performaneLine").getContext('2d');
		var graphGradient2 = document.getElementById("performaneLine").getContext('2d');
		var saleGradientBg = graphGradient.createLinearGradient(5, 0, 5, 100);
		saleGradientBg.addColorStop(0, 'rgba(0, 0, 0, 0)');
		saleGradientBg.addColorStop(1, 'rgba(0, 0, 0, 0)');
		var saleGradientBg2 = graphGradient2.createLinearGradient(100, 0, 50, 150);
		saleGradientBg2.addColorStop(0, 'rgba(0, 0, 0, 0)');
		saleGradientBg2.addColorStop(1, 'rgba(0, 0, 0, 0)');
		var salesTopData = {
			labels: _lables,
			datasets: [{
				label: 'CPU',
				// data: datasets,
				//data: [21, 20, 1, 51, 22, 40, 95, 43, 2, 30, 5, 16, 14, 49, 21, 1, 51, 22, 40, 95, 43, 2, 30, 1, 40, 95, 1, 16, 14, 49, 30, 15, 40, 1, 51, 22, 40, 95, 43, 2, 30, 1, 40, 95, 1, 16, 14, 49, 95, 12],
				backgroundColor: saleGradientBg,
				borderColor: [
					color,
				],
				borderWidth: 1.5,
				fill: true, // 3: no fill
				pointBorderWidth: 1,
				pointRadius: [4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4],
				pointHoverRadius: [2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2],
				pointBackgroundColor: ['#1F3BB3', '#1F3BB3', '#1F3BB3', '#1F3BB3', '#1F3BB3', '#1F3BB3', '#1F3BB3', '#1F3BB3', '#1F3BB3', '#1F3BB3', '#1F3BB3', '#1F3BB3', '#1F3BB3'],
				pointBorderColor: ['#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff',],
			}, {
				label: 'GPU',
				//data: [21, 20, 30, 15, 40, 95, 16, 14, 49, 21, 1, 51, 22, 40, 95, 43, 2, 30, 1, 40, 95, 1, 16, 14, 49, 29, 45, 61, 25, 34, 61, 5, 3, 51, 4, 51, 24, 23, 6, 1, 16, 14, 49, 21, 29, 45, 61, 25, 34, 61, 5, 3],
				backgroundColor: saleGradientBg2,
				borderColor: [
					color,
				],
				borderWidth: 1.5,
				fill: true, // 3: no fill
				pointBorderWidth: 1,
				pointRadius: [4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4],
				pointHoverRadius: [2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2],
				pointBackgroundColor: ['#52CDFF', '#52CDFF', '#52CDFF', '#52CDFF', '#52CDFF', '#52CDFF', '#52CDFF', '#52CDFF', '#52CDFF', '#52CDFF', '#52CDFF', '#52CDFF', '#52CDFF'],
				pointBorderColor: ['#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff',],
			},
			{
				label: 'RAM',
				//data: [43, 2, 30, 1, 40, 95, 1, 16, 14, 49, 21, 29, 1, 51, 22, 40, 95, 43, 2, 30, 1, 40, 95, 1, 16, 14, 49, 45, 61, 25, 3, 1, 40, 95, 1, 16, 14, 49, 21, 29, 45, 6],
				backgroundColor: saleGradientBg2,
				borderColor: [
					color,
				],
				borderWidth: 1.5,
				fill: true, // 3: no fill
				pointBorderWidth: 1,
				pointRadius: [4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4],
				pointHoverRadius: [2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2],
				pointBackgroundColor: ['#eded68', '#eded68', '#eded68', '#eded68', '#eded68', '#eded68', '#eded68', '#eded68', '#eded68', '#eded68', '#eded68', '#eded68', '#eded68'],
				pointBorderColor: ['#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff',],
			},
			{
				label: 'Network',
				data: datasetNetwork,
				backgroundColor: saleGradientBg2,
				borderColor: [
					color,
				],
				borderWidth: 1.5,
				fill: true, // 3: no fill
				pointBorderWidth: 1,
				tension: 0.1
				//pointRadius: [4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4],
				//pointHoverRadius: [2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2],
				//pointBackgroundColor: ['#e65555', '#e65555', '#e65555', '#e65555', '#e65555', '#e65555', '#e65555', '#e65555', '#e65555', '#e65555', '#e65555', '#e65555', '#e65555'],
				//pointBorderColor: ['#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff',],
			}
			]
		};
		var salesTopOptions = {
			responsive: true,
			maintainAspectRatio: false,
			scales: {
				yAxes: [{
					gridLines: {
						display: true,
						drawBorder: false,
						color: "#F0F0F0",
						zeroLineColor: '#F0F0F0',
					},
					ticks: {
						beginAtZero: true,
						autoSkip: false,
						maxTicksLimit: 4,
						fontSize: 10,
						color: "#6B778C"
					}
				}],
				xAxes: [{
					gridLines: {
						display: false,
						drawBorder: false,
					},
					ticks: {
						beginAtZero: false,
						autoSkip: true,
						maxTicksLimit: 1,
						fontSize: 10,
						color: "#6B778C"
					}
				}],
			},
			legend: false,
			legendCallback: function (chart) {
				if (!isSetElement) {
					isSetElement = true;
					var text = [];
					return text.join("");
				}
			},
			elements: {
				line: {
					tension: 0.4,
				}
			},
			tooltips: {
				backgroundColor: 'rgba(31, 59, 179, 1)',
			}
		}
		var salesTop = isSetElement ? Chart(graphGradient, {
			type: 'line',
			data: salesTopData,
			options: salesTopOptions
		}) :
			new Chart(graphGradient, {
				type: 'line',
				data: salesTopData,
				options: salesTopOptions
			});
		if(!isSetElement)
		document.getElementById('performance-line-legend').innerHTML = salesTop.generateLegend();
	}
}

//////////////////////////////////////////////////////////////////////////////////////////////////////
(function ($) {

})(jQuery);
