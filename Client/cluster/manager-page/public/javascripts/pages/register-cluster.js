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
function register(body, status) {
	Utils.newSwal.fire({
		title: "Đang đăng kí",
		text: "Vui lòng chờ . . .",
		didOpen: () => {
			Swal.showLoading()
			var date = new Date(body.dob);
			body.dob = body.dob ? date.toISOString(): "1990-01-01T00:00:00.000Z"; //will return an ISO representation of the date
			body.jobs = body.jobs == null ? "nosetJobs": body.jobs,
			API.register(body)
				.then(async data => {
					const response = await data.json()
					if (data.status == 200) {
						if (response.errors == null) {
							setCookie("token-cluster", response.token, MINUTES59)
							Utils.newSwal.fire({
								title: "Thành công!",
								text: "Chuyển hướng tới bảng điều khiển sau 2s",
								icon: "success",
								didOpen: () => {
									setTimeout(() => {
										window.location.href = "/dashboard"
									}, 2000)
								}
							})
						} else {
								Utils.responseError(response.errors[0].code, response.errors[0].description, "error")
						}
					} else {
						if (status)
							Utils.responseErrorHandler(response)
					}
				})
				.catch(status ? Utils.fetchErrorHandler : "")
		}
	})
}

$(document).ready(() => {
    $('#register').click(() => {
		const body = serializeArrToObject($("form").serializeArray())
		if (window.register) register(body)
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

	$("#dateOfBirth").focus(function () {
		$(this).attr("type", "date")
	})
})