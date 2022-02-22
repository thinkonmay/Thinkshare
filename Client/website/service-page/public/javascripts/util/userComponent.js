import * as API from "./api.js"
import { setCookie } from "./cookie.js";
import { createWorkerBlock } from "./workerComponent.js";

/**
 * 
 */
export async function
	prepare_user_infor() {
	const userinfor = await (await API.getInfor()).json()
	if (userinfor == null || userinfor == "")
		throw new Error('Get Userinfor Failed');

	$("#fullName").html(userinfor.fullName)
	$("#WelcomeUsername").html(userinfor.userName);
}

/**
 * 
 */
export async function
prepare_worker_dashboard() {
	API.fetchWorker().then(async response => {
		var workers = await response.json()
		for (var worker in workers) {
			await createWorkerBlock(worker, workers[worker], "availableWorkers");
		}
	})
	
	API.fetchSession().then(async response => {
		var sessions = await response.json()
		for (var worker in sessions) {
			await createWorkerBlock(worker, sessions[worker], "sessionInUse");
		}
	})
}

/**
 * 
 */
export function prepare_logout() {
	$('#logout').click(() => { API.Logout(); })
}