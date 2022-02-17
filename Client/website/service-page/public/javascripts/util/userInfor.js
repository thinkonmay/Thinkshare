import * as API from "./api.js"
import { createSlave } from "./workerComponent.js";

/**
 * 
 */
export async function 
prepare_user_infor() {
	const userinfor = await (await API.getInfor()).json()
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
			for (var worker in sessions) {
				createSlave(worker, sessions[worker], "slavesInUses");
			}
			for (var worker in slaves) {
				createSlave(worker, slaves[worker], "availableSlaves");
			}
		})
	})
}

