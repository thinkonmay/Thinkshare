import * as API from "../util/api.js"
import * as Validates from "../validates/index.js"
import { getCookie, setCookie } from "../util/cookie.js"
import * as Utils from "../util/utils.js"
const MINUTES59 = 59 * 60 * 1000;


function serializeArrToObject(serializeArr) {
	const obj = {}
	serializeArr.map(item => (obj[item.name] = item.value))
	return obj
}

function login(body) {
	console.log(body);
	Utils.newSwal.fire({
		title: "Đang đăng nhập",
		text: "Vui lòng chờ . . .",
		didOpen: () => {
			Swal.showLoading()
			API.Login(body)
				.then(async data => {
					const response = await data.json()
					if (data.status == 200) {
						if (response.errors == null) {
							setCookie("token", response.token, MINUTES59)
							window.location.replace(API.Dashboard)
						} else {
							console.log(response.error);
							Utils.responseError(response.errors[0].code, response.errors[0].description, "error")
						}
					} else Utils.responseError(response.errors[0].code, response.errors[0].description, "error")
				})
				.catch(Utils.fetchErrorHandler)
		}
	})
}

// Sign-in failure callback

$(document).ready(() => {

	$('#login').click(() => {
		const body = serializeArrToObject($("form").serializeArray())
		if (window.login) login(body)
	})
$("form").validate(window.login ? Validates.login : Validates.register)

const $textInputs = $("input")
const $submit = $(".submit")
const handler = function () {
	const $validTextInputs = $("input:valid")
	if ($textInputs.length === $validTextInputs.length) {
		$submit.attr("disabled", null)
	} else {
		$submit.attr("disabled", "")
	}
}
$("form :input").keyup(handler)
$("form :input").change(handler)
})