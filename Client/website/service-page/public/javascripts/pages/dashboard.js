import * as API from "../util/api.js"
import * as RemotePage from "../util/remote.js"
import {
	getCookie,
	setCookie,
	deleteCookie
} from "../util/cookie.js"
import * as Utils from "../util/utils.js"
import * as CheckDevice from "../util/checkdevice.js"
import * as Setting from "./setting.js"



let datasets = [];
let sessionInfor;

API.getInfor().then(async data => {
	$("#fullName").html((await data.json()).fullName)
})

function
	clusterFormGen(isManager) {
	var form = document.getElementById("ClusterForm");
	if (isManager === true) {
		form.innerHTML =
			`
		<label class="col-sm-3 col-form-label">
			ClusterName
		</label>
		<div class="col-sm-9">
		<input type="text" class="form-control" id="clusterNameCtrler"
			placeholder=" ClusterName ">
		</div>
		<label class="col-sm-3 col-form-label">
			Password
		</label>
		
		<div class="col-sm-9">
		<input type="password" class="form-control" id="passwordCtrler"
			placeholder=" Type your password here ">
		</div>
		<label class="col-sm-3 col-form-label">
			Region
		</label>
		<div class="col-sm-9">
			<select class="form-control" id="regionCtrler">
				<option value="US-West">US West</option>
				<option value="US-East">US East</option>
				<option value="Canada">Canada</option>
				<option value="Singapore">Singapore</option>
				<option value="India">India</option>
				<option value="South_Korea">South Korea</option>
				<option value="Australia">Australia</option>
				<option value="Tokyo">Tokyo</option>
			</select>
		</div>
		`;
	}
	else {
		form.innerHTML =
			`
		<label class="col-sm-3 col-form-label">
			<b>Description</b> 
			<br>
			<i>Is there any specific requirement that we can support? </i>
		</label>
		<div class="col-sm-9">
			<input type="text" class="form-control" id="descriptionCtrler"
				style="height: 100px;" placeholder=" Why do you want to host your own worker node ? ">
		</div>
		`;
	}
}

async function
	isManager() {
	var isManager = (await (await API.getRoles()).json()).isManager;
	return isManager === "true";
}

function
	managerRegister() {
	Utils.newSwal.fire({
		title: "Registering",
		text: "Please wait . . .",
		showConfirmButton: false,
		didOpen: async () => {
			if (await isManager()) {
				var password = document.getElementById("passwordCtrler").value;
				var name = document.getElementById("clusterNameCtrler").value;
				var region = document.getElementById("regionCtrler").value
				API.requestCluster(name, password, region)
					.then(async data => {
						const response = await data.text()
						if (data.status == 200) {
							Utils.newSwal.fire({
								title: "Success",
								text: `You are able to host your own worker using cluster name ${name}`,
								icon: "success",
								didOpen: () => { }
							})
						} else {
							Utils.newSwal.fire({
								title: "Failed",
								text: `Unable to request cluster ${name}: ${response}`,
								icon: "error",
								didOpen: () => { }
							})
						}
					})
					.catch(Utils.fetchErrorHandler)
			}
			else {
				var des = document.getElementById("descriptionCtrler").value;
				API.managerRegister(des)
					.then(async data => {
						const response = await data.json()
						if (data.status == 200) {
							if (response.errors == null) {
								Utils.newSwal.fire({
									title: "Success",
									text: "Now you can create your own cluster",
									icon: "success",
									didOpen: () => { clusterFormGen(true); }
								})
							} else {
								Utils.responseError(response.errors[0].code, response.errors[0].description, "error")
							}
						} else Utils.responseErrorHandler(response)
					})
					.catch(Utils.fetchErrorHandler)
			}
		}
	});
};

API.getSetting().then(async data => {
	var body = await data.json()
	$(`[value=${Setting.DecodeCoreEngine(parseInt(body.engine))}]`).attr('checked', true);
	$(`[value=${Setting.DecodeCodec(parseInt(body.audioCodec))}]`).attr('checked', true);
	$(`[value=${Setting.DecodeCodec(parseInt(body.videoCodec))}]`).attr('checked', true);
	$(`[value=${Setting.DecodeDeviceType(parseInt(body.device))}]`).attr('checked', true);
	$(`[value=${Setting.DecodeQoeMode(parseInt(body.mode))}]`).attr('checked', true);
	$(`[value=${Setting.DecodeResolution(body)}]`).attr('checked', true);

})


API.getClusters().then(async data =>{
	var body = await data.json()

	body.forEach(element => {
		append("clusterBoard",
		` <h5>Cluster Name: ${element.name}, <a href="${element.url}">Access cluster dashboard here</a></h5> `)
	});

})


$(document).ready(async () => {
	document.querySelector(".preloader").style.opacity = "0";
	document.querySelector(".preloader").style.display = "none";


	$('[name="download-app"]').click(function () {
		let value;
		fetch('https://api.github.com/repos/thinkonmay/Thinkremote/releases?page=1&per_page=100/assets/')
			.then(response => response.json())
			.then(data => {
				value = data
				let url = `https://github.com/thinkonmay/Thinkremote/releases/download/${value[0].tag_name}/Thinkremote.msi`
				window.location.href = url
			});
	})

	clusterFormGen(await isManager());
	if (getCookie("show-tutorial") == "true") {
		$('#checkboxTutorial').attr("checked", true)
	}
	if (getCookie("show-tutorial") != "true") {
		$('#modal-btn3').attr('checked', true)
	}

	$('#showTutorial').click(function () {
	})
	$('#showSettings').click(function () {
		window.location = '/dashboard#settings-modal'
	})
	$('#registertoManager').click(function () {
		window.location = '/dashboard#manager-modal'
	})




	$('.modal__checkbox').click(function () {
		if ($('#checkboxTutorial').attr("checked") == 'checked') {
			// have been click this checkbox
			document.getElementById('checkboxTutorial').removeAttribute("checked")
			// $('#checkboxTutorial').removeAttr("checked")
			setCookie("show-tutorial", "false", 99999999999999)
		} else
			if ($('#checkboxTutorial').attr("checked") != 'checked') {
				// click checkbox
				$('#showTutorialState').html("Yes, I won\'t show again")
				$('#checkboxTutorial').attr("checked", true)
				setCookie("show-tutorial", "true", 99999999999999)
			}
	})



	// Remote Core
	$('[name="remote"]').click(async function () {
		var display = await (await API.getSetting()).json();
		var value = $(this).val();
		display.engine = Setting.CoreEngine(value);
		await Setting.updateSetting(display);
	})

	// Remote control bitrate
	$('[name="bitrate"]').click(async function () {
		var display = await (await API.getSetting()).json();
		var value = $(this).val();
		display.mode = Setting.QoEMode(value);
		await Setting.updateSetting(display);
	});

	$('[id="submitClusterCtrler"]').click(managerRegister);

	// Resolution
	$('[name="res"]').click(async function () {
		var display = await (await API.getSetting()).json();
		var value = $(this).attr("value");
		switch (value) {
			case "FullHD":
				display.screenWidth = 1920;
				display.screenHeight = 1080;
				break;
			case "2K":
				display.screenWidth = 2560;
				display.screenHeight = 1440;
				break;
			case "4K":
				display.screenWidth = 3840;
				display.screenHeight = 2160;
				break;
		}
		await Setting.updateSetting(display);

	})

	// VideoCodec
	$('[name="video"]').click(async function () {
		var display = await (await API.getSetting()).json();
		var value = $(this).val();
		display.videoCodec = Setting.Codec(value);
		await Setting.updateSetting(display);
	});

	// Remote control bitrate
	$('[name="bitrate"]').click(async function () {
		var display = await (await API.getSetting()).json();
		var value = $(this).val();
		display.mode = Setting.QoEMode(value);
		await Setting.updateSetting(display);
	});

	// AudioCodec
	$('[name="audio"]').click(async function () {
		var display = await (await API.getSetting()).json();
		var value = $(this).val();
		display.audioCodec = Setting.Codec(value)
		await Setting.updateSetting(display);
	});

	$(".next-tab").click(() => {
		let value = null;
		if ($('#NativeApp').attr('checked') == 'checked') {
			value = 'HowToUse'
			$("#NativeApp").attr('checked', false)
		}
		if ($("#HowToUse").attr('checked') == 'checked') {
			value = 'ShorcutKey';
			$("#HowToUse").attr('checked', false)
		}
		if ($("#ShorcutKey").attr('checked') == 'checked') {
			value = 'NativeApp';
			$("#ShorcutKey").attr('checked', false)
		}
		if (value != null) {
			value = '#' + value
			$(`${value}`).attr('checked', true)
		}
	})




	if (CheckDevice.isElectron()) {
		$('#downloadApp').css("display", "none")
		$('#remoteApp2').removeAttr("disabled")
		$('#videoCodec3').removeAttr("disabled")
	}

	$('#logout').click(() => {
		setCookie("logout", "true")
		setCookie("token", null, 1)

		try {
			gapi.auth.signOut();
			window.location = "/login"
		} catch {
			window.location = "/login"
		}
	})


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

	await connectToClientHub();
	await prepare_user_infor();
	await prepare_worker_dashboard();
	await setDataForChart();
	// set data for chart to anaylize hour used
})

function handleCheckedTab() {

}

async function prepare_user_infor() {
	try {
		const userinfor = await (await API.getInfor()).json()
		document.getElementById("WelcomeUsername").innerHTML = userinfor.fullName;
	} catch {
		(new Promise(resolve => setTimeout(resolve, 5000)))
			.then(() => {
				prepare_user_infor();
			});
	}

}

async function prepare_worker_dashboard() {
	try {
		document.getElementById("slavesInUses").innerHTML = " ";
		document.getElementById("availableSlaves").innerHTML = " ";

		// Enhance performance loading
		let sessions
		let slaves
		API.fetchSession().then(async sessionsData => {
			API.fetchSlave().then(async slavesData => {
				sessions = await sessionsData.json()
				slaves = await slavesData.json()
				for (var worker in sessions) {
					createSlave(worker, sessions[worker], "slavesInUses");
				}
				for (var worker in slaves) {
					createSlave(worker, slaves[worker], "availableSlaves");
				}
			})
		})






	} catch (err) {
		(new Promise(resolve => setTimeout(resolve, 5000)))
			.then(() => {
				prepare_worker_dashboard();
			});
	}
}




async function connectToClientHub() {
	// using websocket to connect to systemhub
	const Websocket = new WebSocket(
		await API.getUserHub(getCookie("token")));

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

function onWebsocketClose(event) {
	(new Promise(resolve => setTimeout(resolve, 5000)))
		.then(() => {
			location.reload();
		});
};

async function createSlave(workerID, workerState, queue) {
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
}


function setState(serviceState, slaveID, queue) {
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


async function setDataForChart() {
	try {
		sessionInfor = await (await API.getSession()).json()
	} catch (error) {
		await setDataForChart();
	}
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
			case 0:
				_lables.unshift("SUN");
				break;
			case 1:
				_lables.unshift("MON");
				break;
			case 2:
				_lables.unshift("TUE");
				break;
			case 3:
				_lables.unshift("WED");
				break;
			case 4:
				_lables.unshift("THU");
				break;
			case 5:
				_lables.unshift("FRI");
				break;
			case 6:
				_lables.unshift("SAT");
				break;
		}
		day--;
		if (day < 0) {
			day = 6;
		}
		countDay++;
	}
	if ($("#performanceLine").length) {
		var graphGradient = document.getElementById("performanceLine").getContext('2d');
		var graphGradient2 = document.getElementById("performanceLine").getContext('2d');
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



	///////////////////////////////////////////////////////////////////////////////////////////////////////
	// do not delete this block, i intentionally reserve doughnutChart for future use
	if ($("#doughnutChart").length) {
		var doughnutChartCanvas = $("#doughnutChart").get(0).getContext("2d");
		var doughnutPieData = {
			datasets: [{
				data: [40, 20, 30, 10],
				backgroundColor: [
					"#1F3BB3",
					"#FDD0C7",
					"#52CDFF",
					"#81DADA"
				],
				borderColor: [
					"#1F3BB3",
					"#FDD0C7",
					"#52CDFF",
					"#81DADA"
				],
			}],

			// These labels appear in the legend and in the tooltips when hovering different arcs
			labels: [
				'Total',
				'Net',
				'Gross',
				'AVG',
			]
		};
		var doughnutPieOptions = {
			cutoutPercentage: 50,
			animationEasing: "easeOutBounce",
			animateRotate: true,
			animateScale: false,
			responsive: true,
			maintainAspectRatio: true,
			showScale: true,
			legend: false,
			legendCallback: function (chart) {
				var text = [];
				text.push('<div class="chartjs-legend"><ul class="justify-content-center">');
				for (var i = 0; i < chart.data.datasets[0].data.length; i++) {
					text.push('<li><span style="background-color:' + chart.data.datasets[0].backgroundColor[i] + '">');
					text.push('</span>');
					if (chart.data.labels[i]) {
						text.push(chart.data.labels[i]);
					}
					text.push('</li>');
				}
				text.push('</div></ul>');
				return text.join("");
			},

			layout: {
				padding: {
					left: 0,
					right: 0,
					top: 0,
					bottom: 0
				}
			},
			tooltips: {
				callbacks: {
					title: function (tooltipItem, data) {
						return data['labels'][tooltipItem[0]['index']];
					},
					label: function (tooltipItem, data) {
						return data['datasets'][0]['data'][tooltipItem['index']];
					}
				},

				backgroundColor: '#fff',
				titleFontSize: 14,
				titleFontColor: '#0B0F32',
				bodyFontColor: '#737F8B',
				bodyFontSize: 11,
				displayColors: false
			}
		};
		var doughnutChart = new Chart(doughnutChartCanvas, {
			type: 'doughnut',
			data: doughnutPieData,
			options: doughnutPieOptions
		});
		document.getElementById('doughnut-chart-legend').innerHTML = doughnutChart.generateLegend();
	}
	//////////////////////////////////////////////////////////////////////////////////////////////////
}

//////////////////////////////////////////////////////////////////////////////////////////////////////
(function ($) { })(jQuery);