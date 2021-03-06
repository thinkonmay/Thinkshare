import * as API from "../util/api.js"
import * as Validates from "../validates/index.js"
import { getCookie, setCookie } from "../util/cookie.js"
import * as Utils from "../util/utils.js"
const MINUTES59 = 59 * 60 * 1000;
const HOUR5 = 5 * 60 * 60 * 1000;


function serializeArrToObject(serializeArr) {
	const obj = {}
	serializeArr.map(item => (obj[item.name] = item.value))
	return obj
}

API.isRegistered().then(async data => {
})

function login(body) {
	Utils.newSwal.fire({
		title: "Logging",
		text: "Wait a min. . .",
		didOpen: () => {
			Swal.showLoading()
			API.login(body)
				.then(async data => {
					const response = await data.json()
					if (data.status == 200) {
						if (response.errors == null) {
							setCookie("token", response.token, HOUR5)
							window.location.replace("dashboard")
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