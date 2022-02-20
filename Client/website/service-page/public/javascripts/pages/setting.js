import { setCookie, } from "../util/cookie.js"
import { prepare_setting } from "../util/setting.js"
import { prepare_user_setting } from "../util/setting-constant.js"




$(document).ready(async () => {
	document.querySelector(".preloader").style.opacity = "0";
	document.querySelector(".preloader").style.display = "none";

	await prepare_user_setting();
	await prepare_setting();

	$('#logout').click(() => { API.Logout(); })
})