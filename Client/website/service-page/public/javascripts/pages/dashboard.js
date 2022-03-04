/**
 * * In this page, I will
 * ? - Implement component, catch error and logError to logging site
 */
import { clusterFormGen } from "../util/manager.js"
import { connectToClientHub } from "../util/clientHub.js"
import { prepare_setting } from "../util/setting.js"
import { setDataForChart } from "../util/chart.js"
import { prepare_user_infor , prepare_worker_dashboard} from "../util/userComponent.js"
import { prepare_user_setting } from "../util/setting-constant.js"
import { prepare_download_button} from "../util/appVersion.js"
import { logUI } from "../util/api.js"
import { Logout } from "../util/api.js"
import { prepare_host_ui } from "../util/setup-worker.js"



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
	prepare_host_ui()					.then().catch(error => { logUI(error.stack)});
	connectToClientHub()				.then().catch(error => { logUI(error.stack)});
	$('#logout').click(() => { Logout(); })
})
