import * as API from "./api.js"
import { setCookie } from "./cookie.js";
import { createSlave } from "./workerComponent.js";

/**
 * 
 */
export async function
	prepare_user_infor() {
	const userinfor = await (await API.getInfor()).json()
	if (userinfor == null || userinfor == "")
		throw new Error('Get Userinfor Failed');

	$("#fullName").html(userinfor.fullName)
	$("#WelcomeUsername").html(userinfor.fullName);
}

/**
 * 
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
 * 
 */
export function prepare_logout() {
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
}
