import * as API from "../util/api.js"
import * as RemotePage from "../util/remote.js"
import { setCookie, } from "../util/cookie.js"
import { clusterFormGen } from "../util/manager.js"
import { connectToClientHub } from "../util/clientHub.js"
import { prepare_setting } from "../util/setting.js"
import { createSlave } from "../util/workerComponent.js"
import { setDataForChart } from "../util/chart.js"




$(document).ready(async () => {
	document.querySelector(".preloader").style.opacity = "0";
	document.querySelector(".preloader").style.display = "none";


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



	await connectToClientHub();
	await prepare_user_infor();
	await prepare_worker_dashboard();
	await setDataForChart();
	await clusterFormGen();
	await prepare_setting();
	// set data for chart to anaylize hour used
})


async function prepare_user_infor() {
	try {
		const userinfor = await (await API.getInfor()).json()
		$("#fullName").html(userinfor.fullName)
		$("WelcomeUsername").html(userinfor.fullName);
	} catch {
		(new Promise(resolve => setTimeout(resolve, 5000)))
			.then(() => {
				prepare_user_infor();
			});
	}

}

async function prepare_worker_dashboard() {
	try {
		// Enhance performance loading
		API.fetchSession().then(async sessionsData => {
			API.fetchSlave().then(async slavesData => {
				var sessions = await sessionsData.json()
				var slaves = await slavesData.json()
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






function append(id, html) { $(`#${id}`).append(html) }
(function ($) { })(jQuery);