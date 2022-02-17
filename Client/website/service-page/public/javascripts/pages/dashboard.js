import { setCookie, } from "../util/cookie.js"
import { clusterFormGen } from "../util/manager.js"
import { connectToClientHub } from "../util/clientHub.js"
import { prepare_setting } from "../util/setting.js"
import { setDataForChart } from "../util/chart.js"
import { prepare_user_infor , prepare_worker_dashboard} from "../util/userInfor.js"
import { prepare_user_setting } from "../util/setting-constant.js"
import { prepare_download_button} from "../util/appVersion.js"
import { prepare_tutorial_popup } from "../util/popup.js"



$(document).ready(async () => {
	document.querySelector(".preloader").style.opacity = "0";
	document.querySelector(".preloader").style.display = "none";

	await prepare_tutorial_popup();
	await prepare_user_infor();
	await prepare_worker_dashboard();
	await setDataForChart();
	await prepare_download_button();
	await clusterFormGen();
	await prepare_user_setting();
	await prepare_setting();
	await connectToClientHub();

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
})
