import * as API from "../util/api.js"
import * as Utils from "../util/utils.js"
import { setCookie, deleteCookie } from "../util/cookie.js"
import { appendWorkerNode } from "../util/user-row-component.js"
import { setDataForChart } from "../util/chart.js"


async function 
prepare_cluster_infor()
{
	let ClusterInfor = await (await API.getInforClusterRoute()).json();
	let workerState = await (await API.getWorkerStateRoute()).json();

	let data = ClusterInfor["workerNode"];
	var name = ClusterInfor["name"];
	var owner = ClusterInfor["owner"];

	document.getElementById("ClusterName").innerHTML = name;
	document.getElementById("OwnerName").innerHTML = owner["fullName"];

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
}

$(document).ready(async () => {
	let timeStampFrom = $("#stamp-time-from").val()
	let timeStampTo = $("#stamp-time-to").val()

	prepare_cluster_infor();

	$('#DeleteCluster').click(() => {
		Utils.newSwal.fire({ 
		title: 'Are you sure?',
		text: "You won't be able to revert this!",
		icon: 'warning',
		showCancelButton: true,
		confirmButtonColor: '#3085d6',
		cancelButtonColor: '#d33',
		confirmButtonText: 'Yes, delete it!'
		}).then(async (result) => {
			if (result.isConfirmed) {
				await API.UnRegister();
				Swal.fire(
				'Deleted!',
				'Your cluster has been deleted.',
				'success'
				)
		}})
	});

	$("#stamp-time-from").on("change", function () {
		timeStampFrom = $("#stamp-time-from").val()
	})
	$("#stamp-time-to").on("change", function () {
		timeStampTo = $("#stamp-time-to").val()
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
})

function setCurrentDateTimeLog() {
	let currentDate = 	String(new Date().toISOString()).slice(0, 16)
	let minDate = 		String(new Date((new Date()).getTime() - 86400000).toISOString()).slice(0, 16)
	let defaultDate = 	String(new Date((new Date()).getTime() - 3600).toISOString()).slice(0, 16)
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