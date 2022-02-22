import * as API from "./api.js"
import {appendWorkerNode} from "./user-row-component.js"
import { setDataForChart } from "./chart.js";
/**
 * 
 */
export function set_worker_chart() {
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
}

export function setup_cluster_booking() {
    let timeStampFrom = $("#stamp-time-from").val()
    let timeStampTo = $("#stamp-time-to").val()
    let bookUser = $("#stamp-time-to").val()

    $("#stamp-time-from").on("change", function () {
        timeStampFrom = $("#stamp-time-from").val()
    })
    $("#stamp-time-to").on("change", function () {
        timeStampTo = $("#stamp-time-to").val()
    })
    $("#bookUser").on("change", function () {
        bookUser = $("#bookUser").val()
    })

    $('#AddBooking').click(async () => {
        let startTime = new Date(timeStampFrom).toISOString()
        // let endTime = convert_date(timeStampTo)
        let endTime = new Date((new Date(startTime)).getTime() + 900000).toISOString() // plus 15 mins
        console.log(startTime)
        console.log(endTime)
        if (startTime == endTime) {
            Swal.fire('Fail!', 'startTime and endTime must have the difference', 'error')
        } else if (startTime > endTime) {
            Swal.fire('Fail!', 'startTime must be low to endTime', 'error')
        }
        else {
            var res = await API.createNewRole(startTime, endTime, bookUser)
            if (res.ok)
                Swal.fire('Success!', `Booking success from ${startTime} to ${endTime} for user "${bookUser}".`, 'success')
            else
                Swal.fire('Fail!', 'Booking failed. Please check and try again.', 'error')
        }

    })
}


export async function prepare_cluster_infor() {
	let ClusterInfor = await (await API.getInforClusterRoute()).json();
	let workerState = await (await API.getWorkerStateRoute()).json();

	let data = ClusterInfor["workerNode"];
	var name = ClusterInfor["name"];
	var owner = ClusterInfor["owner"];

	document.getElementById("ClusterName").innerHTML = `Welcome to your cluster <b>${name}</b> dashboard`;
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

export async function prepare_booker_infor() {
    // let BookerInfor = await (await API.getExistingRole()).json()
    // To do
}