import { setCookie, } from "../util/cookie.js"
import { clusterFormGen } from "../util/manager.js"
import { connectToClientHub } from "../util/clientHub.js"
import { prepare_setting } from "../util/setting.js"
import { setDataForChart } from "../util/chart.js"
import { prepare_user_infor , prepare_worker_dashboard, prepare_logout} from "../util/userComponent.js"
import { prepare_user_setting } from "../util/setting-constant.js"
import { prepare_download_button} from "../util/appVersion.js"
import { prepare_tutorial_popup } from "../util/popup.js"
import { logUI } from "../util/api.js"
import { newSwal } from "../util/utils.js"
import { Logout } from "../util/api.js"



$(document).ready(async () => {
	document.querySelector(".preloader").style.opacity = "0";
	document.querySelector(".preloader").style.display = "none";

	prepare_worker_dashboard()			.then().catch(error => { logUI(error.stack)});
	prepare_download_button()			.then().catch(error => { logUI(error.stack)});
	prepare_user_setting()				.then().catch(error => { logUI(error.stack)});
	setDataForChart()					.then().catch(error => { logUI(error.stack)});
	clusterFormGen()					.then().catch(error => { logUI(error.stack)});
	prepare_user_infor()				.then().catch(error => { logUI(error.stack)});
	prepare_setting()					.then().catch(error => { logUI(error.stack)});
	connectToClientHub()				.then().catch(error => { logUI(error.stack)});
	$('#logout').click(() => { Logout(); })
})
