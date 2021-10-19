import * as API from "../util/api.js"
import * as RemotePage from "../util/remote-page-cookies.js"
import * as Setting from "../util/setting.js"
import { getCookie, setCookie } from "../util/cookie.js"
import * as Utils from "../util/utils.js"
import * as CheckDevice from "../util/checkdevice.js"

let datasets = [];
let sessionInfor;
API.getInfor().then(async data => {
	$("#fullName").html((await data.json()).fullName)
})
$(document).ready(async () => {
	$('#logout').click(() => {
		setCookie("dalogout", 1)
	})

	var defaultDeviceCap = {
		mode: 4,
		videoCodec: 1,
		audioCodec: 4,
		screenWidth: 2560,
		screenHeight: 1440
	}
	setCookie("cap", JSON.stringify(defaultDeviceCap), 999999)

	var bitrate = document.getElementsByName("bitrate-setting");
	for (var item = 0; item < bitrate.length; item++) {
		bitrate[item].onclick = (event) => Setting.Mode(event.target.innerHTML);
	}

	var audio_codec = document.getElementsByName("audiocodec-setting");
	for (var item = 0; item < audio_codec.length; item++) {
		audio_codec[item].onclick = (event) => Setting.AudioCodec(event.target.innerHTML);
	}

	var video_codec = document.getElementsByName("videocodec-setting");
	for (var item = 0; item < video_codec.length; item++) {
		video_codec[item].onclick = (event) => Setting.VideoCodec(event.target.innerHTML);
	}

	var resolution = document.getElementsByName("resolution-setting");
	for (var item = 0; item < resolution.length; item++) {
		resolution[item].onclick = (event) => Setting.mapVideoRes(event.target.innerHTML);
	}

	noti()
	search()
	inbox()
	user()
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
		const userinfor = await (await API.getInfor()).json()
		const sessions = await (await API.fetchSession()).json()
		const slaves = await (await API.fetchSlave()).json()
		sessionInfor = await (await API.getSession()).json()
		let reUserName = userinfor.userName;
		if (reUserName[reUserName.length - 1] == "g" && reUserName[reUserName.length - 2] == "g") {
			userinfor.userName = reUserName.slice(0, reUserName.length - 2);
		}
		document.getElementById("WelcomeUsername").innerHTML = userinfor.userName;

		for (const slave of sessions) {
			createSlave(slave, "slavesInUses");
		}
		for (const slave of slaves) {
			createSlave(slave, "availableSlaves");
		}
	} catch (err) {
		alert(err.message)
	}

	// set data for chart to anaylize hour used
	setDataForChart();


	// var stateSignalR = document.getElementById('state-signalr');
	// Connect to hub signalR with access-token Bearer Authorzation
	const connection = new signalR.HubConnectionBuilder()
		.withUrl(`https://conductor.thinkmay.net/ClientHub`, {
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
			createSlave(slaveInfor, "slavesInUses")
		})
		connection.on("ReportNewSlaveAvailable", function (device) {
			createSlave(device, "availableSlaves")
		})
	}).catch(function (err) {
		location.reload();
	})

	connection.onclose(error => {
		console.assert(connection.state === signalR.HubConnectionState.Disconnected);
		location.reload();
	});
})


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
			<li class="small"><span class="fa-li"><i class="fab fa-windows"></i></span>OS: ${slave.cpu}</li>
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

	if (serviceState === "ON_SESSION") {
		var initbutt = document.getElementById(`disconnect${slaveID}`)
		initbutt.addEventListener("click", async function () {
			await API.disconnectSession(slaveID)
		});
		var terminatebutt = document.getElementById(`terminate${slaveID}`)
		terminatebutt.addEventListener("click", async function () {
			await API.terminateSession(slaveID)
		});
	}
	if (serviceState === "OFF_REMOTE") {
		var recbutt = document.getElementById(`reconnect${slaveID}`)
		recbutt.addEventListener("click", async function () {
			RemotePage.sessionReconnect(slaveID)
		});
		var terminatebutt = document.getElementById(`terminate${slaveID}`)
		terminatebutt.addEventListener("click", async function () {
			await API.terminateSession(slaveID)
		});;
	}
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


function search() {
	document.getElementById('searchButton').addEventListener('click', function () {
		Utils.responseError("Error!!!", "This feature hasn't developed! \n Next version will be update.", "info")
	});
}

function noti() {
	document.getElementById('notiButton').addEventListener('click', function () {
		Utils.responseError("Error!!!", "This feature hasn't developed! \n Next version will be update.", "info")
	});
}

function inbox() {
	document.getElementById('inboxButton').addEventListener('click', function () {
		Utils.responseError("Error!!!", "This feature hasn't developed! \n Next version will be update.", "info")
	});
}

function user() {
	document.getElementById('userButton').addEventListener('click', function () {
		Utils.responseError("Error!!!", "This feature hasn't developed! \n Next version will be update.", "info")
	});
}

function setDataForChart() {
	console.log(sessionInfor)
	for (let i = 0; i < 7; i++) {
		datasets[i] = 0;
	}
	for (let i = 0; i < sessionInfor.length; i++) {
		datasets[sessionInfor[i].dayofWeek] = sessionInfor[i].sessionTime;
	}
	var date = new Date();
	var day = date.getDay();
	let countDay = 0;
	let _lables = [];
	while (countDay <= 6) {
		switch (day) {
			case 0: _lables.unshift("SUN"); break;
			case 1: _lables.unshift("MON"); break;
			case 2: _lables.unshift("TUE"); break;
			case 3: _lables.unshift("WED"); break;
			case 4: _lables.unshift("THU"); break;
			case 5: _lables.unshift("FRI"); break;
			case 6: _lables.unshift("SAT"); break;
		}
		day--;
		if (day < 0) {
			day = 6;
		}
		countDay++;
	}
	if ($("#performaneLine").length) {
		var graphGradient = document.getElementById("performaneLine").getContext('2d');
		var graphGradient2 = document.getElementById("performaneLine").getContext('2d');
		var saleGradientBg = graphGradient.createLinearGradient(5, 0, 5, 100);
		saleGradientBg.addColorStop(0, 'rgba(26, 115, 232, 0.18)');
		saleGradientBg.addColorStop(1, 'rgba(26, 115, 232, 0.02)');
		var saleGradientBg2 = graphGradient2.createLinearGradient(100, 0, 50, 150);
		saleGradientBg2.addColorStop(0, 'rgba(0, 208, 255, 0.19)');
		saleGradientBg2.addColorStop(1, 'rgba(0, 208, 255, 0.03)');
		var salesTopData = {
			labels: _lables,
			datasets: [{
				label: 'This week',
				data: datasets,
				backgroundColor: saleGradientBg,
				borderColor: [
					'#1F3BB3',
				],
				borderWidth: 1.5,
				fill: true, // 3: no fill
				pointBorderWidth: 1,
				pointRadius: [4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4],
				pointHoverRadius: [2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2],
				pointBackgroundColor: ['#1F3BB3)', '#1F3BB3', '#1F3BB3', '#1F3BB3', '#1F3BB3)', '#1F3BB3', '#1F3BB3', '#1F3BB3', '#1F3BB3)', '#1F3BB3', '#1F3BB3', '#1F3BB3', '#1F3BB3)'],
				pointBorderColor: ['#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff',],
			},
				//  {
				// 	label: 'Last week',
				// 	data: [30, 150, 190, 250, 120, 150, 130],
				// 	backgroundColor: saleGradientBg2,
				// 	borderColor: [
				// 		'#52CDFF',
				// 	],
				// 	borderWidth: 1.5,
				// 	fill: true, // 3: no fill
				// 	pointBorderWidth: 1,
				// 	pointRadius: [4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4],
				// 	pointHoverRadius: [2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2],
				// 	pointBackgroundColor: ['#52CDFF)', '#52CDFF', '#52CDFF', '#52CDFF', '#52CDFF)', '#52CDFF', '#52CDFF', '#52CDFF', '#52CDFF)', '#52CDFF', '#52CDFF', '#52CDFF', '#52CDFF)'],
				// 	pointBorderColor: ['#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff', '#fff',],
				// }
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
						beginAtZero: false,
						autoSkip: true,
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
						maxTicksLimit: 7,
						fontSize: 10,
						color: "#6B778C"
					}
				}],
			},
			legend: false,
			legendCallback: function (chart) {
				var text = [];
				text.push('<div class="chartjs-legend"><ul>');
				for (var i = 0; i < chart.data.datasets.length; i++) {
					text.push('<li>');
					text.push('<span style="background-color:' + chart.data.datasets[i].borderColor + '">' + '</span>');
					text.push(chart.data.datasets[i].label);
					text.push('</li>');
				}
				text.push('</ul></div>');
				return text.join("");
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
		var salesTop = new Chart(graphGradient, {
			type: 'line',
			data: salesTopData,
			options: salesTopOptions
		});
		document.getElementById('performance-line-legend').innerHTML = salesTop.generateLegend();
	}
}

//////////////////////////////////////////////////////////////////////////////////////////////////////
(function ($) {
})(jQuery);