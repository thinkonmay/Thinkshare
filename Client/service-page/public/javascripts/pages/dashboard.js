import * as API from "../util/api.js"
import * as RemotePage from "../util/remote-page-cookies.js"
import * as Setting from "../util/setting.js"
import { getCookie, setCookie } from "../util/cookie.js"
import * as Utils from "../util/utils.js"



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

	noti()
	search()
	inbox()
	user()

	setCookie("cap", JSON.stringify(defaultDeviceCap), 999999)
	console.log("set default device capability to " + getCookie("cap"));
	/// How to convert to JSON 
	/// var cap = getCookie("cap");
	//  var parse = JSON.parse(cap);
	// use "parse" like json 
	// For ex: parse["mode"]

	try {
		const userinfor = await (await API.getInfor()).json()
		const sessions = await (await API.fetchSession()).json()
		const slaves = await (await API.fetchSlave()).json()
		const sessionInfor = await (await API.getSession()).json()

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



	var stateSignalR = document.getElementById('state-signalr');
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


//////////////////////////////////////////////////////////////////////////////////////////////////////
(function($) {
	'use strict';
	$(function() {
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
			labels: ["SUN","sun", "MON", "mon", "TUE","tue", "WED", "wed", "THU", "thu", "FRI", "fri", "SAT"],
			datasets: [{
				label: 'This week',
				data: [50, 110, 60, 390, 200, 115, 130, 170, 90, 210, 240, 280, 200],
				backgroundColor: saleGradientBg,
				borderColor: [
					'#1F3BB3',
				],
				borderWidth: 1.5,
				fill: true, // 3: no fill
				pointBorderWidth: 1,
				pointRadius: [4, 4, 4, 4, 4,4, 4, 4, 4, 4,4, 4, 4],
				pointHoverRadius: [2, 2, 2, 2, 2,2, 2, 2, 2, 2,2, 2, 2],
				pointBackgroundColor: ['#1F3BB3)', '#1F3BB3', '#1F3BB3', '#1F3BB3','#1F3BB3)', '#1F3BB3', '#1F3BB3', '#1F3BB3','#1F3BB3)', '#1F3BB3', '#1F3BB3', '#1F3BB3','#1F3BB3)'],
				pointBorderColor: ['#fff','#fff','#fff','#fff','#fff','#fff','#fff','#fff','#fff','#fff','#fff','#fff','#fff',],
			},{
			  label: 'Last week',
			  data: [30, 150, 190, 250, 120, 150, 130, 20, 30, 15, 40, 95, 180],
			  backgroundColor: saleGradientBg2,
			  borderColor: [
				  '#52CDFF',
			  ],
			  borderWidth: 1.5,
			  fill: true, // 3: no fill
			  pointBorderWidth: 1,
			  pointRadius: [0, 0, 0, 4, 0],
			  pointHoverRadius: [0, 0, 0, 2, 0],
			  pointBackgroundColor: ['#52CDFF)', '#52CDFF', '#52CDFF', '#52CDFF','#52CDFF)', '#52CDFF', '#52CDFF', '#52CDFF','#52CDFF)', '#52CDFF', '#52CDFF', '#52CDFF','#52CDFF)'],
				pointBorderColor: ['#fff','#fff','#fff','#fff','#fff','#fff','#fff','#fff','#fff','#fff','#fff','#fff','#fff',],
		  }]
		};
	
		var salesTopOptions = {
		  responsive: true,
		  maintainAspectRatio: false,
			scales: {
				yAxes: [{
					gridLines: {
						display: true,
						drawBorder: false,
						color:"#F0F0F0",
						zeroLineColor: '#F0F0F0',
					},
					ticks: {
					  beginAtZero: false,
					  autoSkip: true,
					  maxTicksLimit: 4,
					  fontSize: 10,
					  color:"#6B778C"
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
					color:"#6B778C"
				  }
			  }],
			},
			legend:false,
			legendCallback: function (chart) {
			  var text = [];
			  text.push('<div class="chartjs-legend"><ul>');
			  for (var i = 0; i < chart.data.datasets.length; i++) {
				console.log(chart.data.datasets[i]); // see what's inside the obj.
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
	  if ($("#performaneLine-dark").length) {
		var graphGradient = document.getElementById("performaneLine-dark").getContext('2d');
		var graphGradient2 = document.getElementById("performaneLine-dark").getContext('2d');
		var saleGradientBg = graphGradient.createLinearGradient(5, 0, 5, 100);
		saleGradientBg.addColorStop(0, 'rgba(26, 115, 232, 0.18)');
		saleGradientBg.addColorStop(1, 'rgba(34, 36, 55, 0.5)');
		var saleGradientBg2 = graphGradient2.createLinearGradient(10, 0, 0, 150);
		saleGradientBg2.addColorStop(0, 'rgba(0, 208, 255, 0.19)');
		saleGradientBg2.addColorStop(1, 'rgba(34, 36, 55, 0.2)');

		

		var salesTopDataDark = {
			labels: ["SUN","sun", "MON", "mon", "TUE","tue", "WED", "wed", "THU", "thu", "FRI", "fri", "SAT"],
			datasets: [{
				label: '# of Votes',
				data: [50, 110, 60, 290, 200, 115, 130, 170, 90, 210, 240, 280, 200],
				backgroundColor: saleGradientBg,
				borderColor: [
					'#1F3BB3',
				],
				borderWidth: 1.5,
				fill: true, // 3: no fill
				pointBorderWidth: 1,
				pointRadius: [4, 4, 4, 4, 4,4, 4, 4, 4, 4,4, 4, 4],
				pointHoverRadius: [2, 2, 2, 2, 2,2, 2, 2, 2, 2,2, 2, 2],
				pointBackgroundColor: ['#1F3BB3)', '#1F3BB3', '#1F3BB3', '#1F3BB3','#1F3BB3)', '#1F3BB3', '#1F3BB3', '#1F3BB3','#1F3BB3)', '#1F3BB3', '#1F3BB3', '#1F3BB3','#1F3BB3)'],
				pointBorderColor: ['#222437','#222437','#222437','#222437','#222437','#222437','#222437','#222437','#222437','#222437','#222437','#222437','#222437',],
			},{
			  label: '# of Votes',
			  data: [30, 150, 190, 250, 120, 150, 130, 20, 30, 15, 40, 95, 180],
			  backgroundColor: saleGradientBg2,
			  borderColor: [
				  '#52CDFF',
			  ],
			  borderWidth: 1.5,
			  fill: true, // 3: no fill
			  pointBorderWidth: 1,
			  pointRadius: [0, 0, 0, 4, 0],
			  pointHoverRadius: [0, 0, 0, 2, 0],
			  pointBackgroundColor: ['#52CDFF)', '#52CDFF', '#52CDFF', '#52CDFF','#52CDFF)', '#52CDFF', '#52CDFF', '#52CDFF','#52CDFF)', '#52CDFF', '#52CDFF', '#52CDFF','#52CDFF)'],
				pointBorderColor: ['#222437','#222437','#222437','#222437','#222437','#222437','#222437','#222437','#222437','#222437','#222437','#222437','#222437',],
		  }]
		};
	
		var salesTopOptionsDark = {
		  responsive: true,
		  maintainAspectRatio: false,
			scales: {
				yAxes: [{
					gridLines: {
						display: true,
						drawBorder: false,
						color:"rgba(255,255,255,.05)",
						zeroLineColor: "rgba(255,255,255,.05)",
					},
					ticks: {
					  beginAtZero: false,
					  autoSkip: true,
					  maxTicksLimit: 4,
					  fontSize: 10,
					  color:"#6B778C"
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
					color:"#6B778C"
				  }
			  }],
			},
			legend:false,
			legendCallback: function (chart) {
			  var text = [];
			  text.push('<div class="chartjs-legend"><ul>');
			  for (var i = 0; i < chart.data.datasets.length; i++) {
				console.log(chart.data.datasets[i]); // see what's inside the obj.
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
		var salesTopDark = new Chart(graphGradient, {
			type: 'line',
			data: salesTopDataDark,
			options: salesTopOptionsDark
		});
		document.getElementById('performance-line-legend-dark').innerHTML = salesTopDark.generateLegend();
	  }
	  if ($("#datepicker-popup").length) {
		$('#datepicker-popup').datepicker({
		  enableOnReadonly: true,
		  todayHighlight: true,
		});
		$("#datepicker-popup").datepicker("setDate", "0");
	  }
	  if ($("#status-summary").length) {
		var statusSummaryChartCanvas = document.getElementById("status-summary").getContext('2d');;
		var statusData = {
			labels: ["SUN", "MON", "TUE", "WED", "THU", "FRI"],
			datasets: [{
				label: '# of Votes',
				data: [50, 68, 70, 10, 12, 80],
				backgroundColor: "#ffcc00",
				borderColor: [
					'#01B6A0',
				],
				borderWidth: 2,
				fill: false, // 3: no fill
				pointBorderWidth: 0,
				pointRadius: [0, 0, 0, 0, 0, 0],
				pointHoverRadius: [0, 0, 0, 0, 0, 0],
			}]
		};
	
		var statusOptions = {
		  responsive: true,
		  maintainAspectRatio: false,
			scales: {
				yAxes: [{
				  display:false,
					gridLines: {
						display: false,
						drawBorder: false,
						color:"#F0F0F0"
					},
					ticks: {
					  beginAtZero: false,
					  autoSkip: true,
					  maxTicksLimit: 4,
					  fontSize: 10,
					  color:"#6B778C"
					}
				}],
				xAxes: [{
				  display:false,
				  gridLines: {
					  display: false,
					  drawBorder: false,
				  },
				  ticks: {
					beginAtZero: false,
					autoSkip: true,
					maxTicksLimit: 7,
					fontSize: 10,
					color:"#6B778C"
				  }
			  }],
			},
			legend:false,
			
			elements: {
				line: {
					tension: 0.4,
				}
			},
			tooltips: {
				backgroundColor: 'rgba(31, 59, 179, 1)',
			}
		}
		var statusSummaryChart = new Chart(statusSummaryChartCanvas, {
			type: 'line',
			data: statusData,
			options: statusOptions
		});
	  }
	  if ($('#totalVisitors').length) {
		var bar = new ProgressBar.Circle(totalVisitors, {
		  color: '#fff',
		  // This has to be the same size as the maximum width to
		  // prevent clipping
		  strokeWidth: 15,
		  trailWidth: 15, 
		  easing: 'easeInOut',
		  duration: 1400,
		  text: {
			autoStyleContainer: false
		  },
		  from: {
			color: '#52CDFF',
			width: 15
		  },
		  to: {
			color: '#677ae4',
			width: 15
		  },
		  // Set default step function for all animate calls
		  step: function(state, circle) {
			circle.path.setAttribute('stroke', state.color);
			circle.path.setAttribute('stroke-width', state.width);
	
			var value = Math.round(circle.value() * 100);
			if (value === 0) {
			  circle.setText('');
			} else {
			  circle.setText(value);
			}
	
		  }
		});
	
		bar.text.style.fontSize = '0rem';
		bar.animate(.64); // Number from 0.0 to 1.0
	  }
	  if ($('#visitperday').length) {
		var bar = new ProgressBar.Circle(visitperday, {
		  color: '#fff',
		  // This has to be the same size as the maximum width to
		  // prevent clipping
		  strokeWidth: 15,
		  trailWidth: 15,
		  easing: 'easeInOut',
		  duration: 1400,
		  text: {
			autoStyleContainer: false
		  },
		  from: {
			color: '#34B1AA',
			width: 15
		  },
		  to: {
			color: '#677ae4',
			width: 15
		  },
		  // Set default step function for all animate calls
		  step: function(state, circle) {
			circle.path.setAttribute('stroke', state.color);
			circle.path.setAttribute('stroke-width', state.width);
	
			var value = Math.round(circle.value() * 100);
			if (value === 0) {
			  circle.setText('');
			} else {
			  circle.setText(value);
			}
	
		  }
		});
	
		bar.text.style.fontSize = '0rem';
		bar.animate(.34); // Number from 0.0 to 1.0
	  }
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
			  title: function(tooltipItem, data) {
				return data['labels'][tooltipItem[0]['index']];
			  },
			  label: function(tooltipItem, data) {
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
	});
  })(jQuery);