import * as API from "../util/api.js"
import { setCookie, deleteCookie } from "../util/cookie.js"
import { appendWorkerNode } from "../util/user-row-component.js"
import { setDataForChart } from "../util/chart.js"

let dataset = [];

let sessionInfor;
API.getInfor().then(async data => {
	let body = await data.json();
	$("#dashboardSrcImg").attr("src", (body.avatar) == null ? "images/default_user.png" : body.avatar)
	$("#WelcomeUsername").html(body.fullName)
	$("#avatarSrc").attr("src", (body.avatar) == null ? "images/default_user.png" : body.avatar)
	$("#fullname").text(body.fullName)
	$("#jobs").text(body.jobs)
})





$(document).ready(async () => {
	let nameOfCluster;
	let timeStampFrom = $("#stamp-time-from").val()
	let timeStampTo = $("#stamp-time-to").val()


	$("#stamp-time-from").on("change", function () {
		timeStampFrom = $("#stamp-time-from").val()
	})
	$("#stamp-time-to").on("change", function () {
		timeStampTo = $("#stamp-time-to").val()
	})


	$("#nameCluster").on("change", function () {
		nameOfCluster = this.value;
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


	$('#analyticCPU').click(() => {
		setDataForChart('#1F3BB3', 'CPU', false);
	});

	$('#analyticGPU').click(() => {
		setDataForChart('#52CDFF', 'GPU', false);
	})
	$('#analyticRAM').click(() => {
		setDataForChart('#eded68', 'RAM', false);
	})
	$('#analyticNetwork').click(() => {
		setDataForChart('#e65555', "Network", false);
	})


	let _ClusterInfor = await (await API.getInforClusterRoute()).json()

	let data = _ClusterInfor["workerNode"];
	var date = new Date(_ClusterInfor["register"])
	append('Cluster-Infor', `
	<div>
		<h4 class="card-title card-title-dash">Name: ${_ClusterInfor["name"]}</h4> 
	</div>
	<div>
		<h6 class="card-subtitle card-subtitle-dash">Register: ${String(date).slice(0, 24)}</h6>
	</div>
	`)

	let workerState = await (await API.getWorkerStateRoute()).json();
	for (let i = 0; i < data.length; i++) {
		let workerNodeId = data.at(i).id
		let state = workerState[workerNodeId]
		appendWorkerNode(data.at(i).os, 
		                 data.at(i).cpu, 
						 data.at(i).gpu, 
						 data.at(i).id, 
						 data.at(i).raMcapacity, 
						 data.at(i).register, 
						 state)
	}
})

function setCurrentDateTimeLog() {
	let currentDate = String(new Date().toISOString()).slice(0, 16)
	let minDate = String(new Date((new Date()).getTime() - 86400000).toISOString()).slice(0, 16)
	let defaultDate = String(new Date((new Date()).getTime() - 3600).toISOString()).slice(0, 16)
	$('#stamp-time-from').attr({
		"max": currentDate,
		"min": minDate,
		"value": defaultDate
	});
	$('#stamp-time-to').attr({
		"max": currentDate,
		"min": minDate,
		"value": defaultDate
	});
}


function append(id, html) { $(`#${id}`).append(html) }
(function ($) { })(jQuery);