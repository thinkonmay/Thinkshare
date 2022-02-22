/**
 * * In this function, I will: 
 * ? - Handle about userInfor 
 * ? - Get Device and Session and show
 */
import * as API from "./api.js"
import { createSlave } from "./workerComponent.js";

/**
 * TODO:Fill #fullName and #WelcomeUsername
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
 * 	TODO: generate device UI
 */
export async function
	prepare_worker_dashboard() {
	API.fetchSession().then(async sessionsData => {
		API.fetchSlave().then(async slavesData => {
			var sessions = await sessionsData.json()
			var slaves = await slavesData.json()
			if (sessions == null || sessions == "")
				for (var worker in sessions) {
					createSlave(worker, sessions[worker], "slavesInUses");
				}
			for (var worker in slaves) {
				createSlave(worker, slaves[worker], "availableSlaves");
			}
		})
	})
}

/**
 * TODO: Execute Logout
 */
export function prepare_logout() {
	$('#logout').click(() => { API.Logout(); })
}
