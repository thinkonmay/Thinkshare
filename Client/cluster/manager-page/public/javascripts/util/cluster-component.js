import * as API from "./api.js"
import {appendWorkerNode} from "./user-row-component.js"
import { setDataForChart } from "./chart.js";
import * as Utils from "./utils.js"
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
    $('#AddBooking').click(async () => {
        let timeStampFrom = $("#stamp-time-from").val()
        let timeStampTo = $("#stamp-time-to").val()
        let bookUser = $("#stamp-time-to").val()

        let startTime = new Date(timeStampFrom).toISOString()
        let endTime   = new Date(timeStampTo).toISOString()

        await API.createNewRole(startTime, endTime, bookUser)
        Swal.fire('Success!', `...`, 'success')
    })

    $('#AddTempBooking').click(async () => {
        var bookUser = $("#bookUserTemp").val()
        let minutes = $("#bookTimeMinutes").val()

        await API.createNewInstantRole(bookUser,minutes)
        Swal.fire('Success!', `...`, 'success')
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
    document.getElementById("BookingTable").innerHTML = "";
    let BookerInfors = await (await API.getExistingRole()).json()
    BookerInfors.forEach(element => {
        BookerBlock(element.id,element.description,element.user,null,null);
    });
}


function BookerBlock (ID, Description,user, From, To) 
{

    var result = 
    `
    <tbody id="BookInfor${ID}">
    <tr>
        <td>
        <div class="d-flex ">
            <img src="images/faces/face1.jpg" alt="">
            <div>
            <h6>${Description}</h6>
            <p>${user.userName}</p>
            </div>
        </div>
        </td>
        <td>
        <h6>${user.email}</h6>
        <p>${user.jobs}</p>
        </td>
        <td>
        <div>
            <div class="d-flex justify-content-between align-items-center mb-1 max-width-progress-wrap">
            <p class="text-success">79%</p>
            <p>85/162</p>
            </div>
            <div class="progress progress-md">
            <div class="progress-bar bg-success" role="progressbar" style="width: 85%" aria-valuenow="25" aria-valuemin="0" aria-valuemax="100"></div>
            </div>
        </div>
        </td>
        <td><div class="badge badge-opacity-warning">In progress</div></td>
    </tr>
    </tbody>
    `
    append("BookingTable",result);

    $(`#BookInfor${ID}`).click(() =>{
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
                var res = await API.deleteRole(ID);

                if (res.ok)
                    Swal.fire('Success!', '...', 'success')
                else
                    Swal.fire('Fail!', '...', 'error')
            }
	})
        
    })
}

function append(id, html) { $(`#${id}`).append(html) }