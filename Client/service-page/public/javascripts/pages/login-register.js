import * as API from "../util/api.js"
import * as Validates from "../validates/index.js"
import { setCookie } from "../util/cookie.js"
import * as Utils from "../util/utils.js"
const MINUTES59 = 59 * 60 * 1000;

(function ($) {
	"use strict"

	/*==================================================================
	[ Validate ]*/
	$('#login-google').click(() => {

	} )
	$('#login').click(() => {
		$("form").submit(event => {
			event.preventDefault()
			if ($("form").valid()) {
				const body = serializeArrToObject($("form").serializeArray())
				if (window.login) login(body)
				else if (window.register) register(body)
			}
		})
	})	
	$('#register').click(() =>{
		$("form").submit(event => {
			event.preventDefault()
			if ($("form").valid()) {
				const body = serializeArrToObject($("form").serializeArray())
				if (window.login) login(body)
				else if (window.register) register(body)
			}
		})
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
})(jQuery)

function serializeArrToObject(serializeArr) {
	const obj = {}
	serializeArr.map(item => (obj[item.name] = item.value))
	return obj
}

function login(body) {
	Utils.newSwal.fire({
		title: "Đang đăng nhập",
		text: "Vui lòng chờ . . .",
		didOpen: () => {
			Swal.showLoading()
			API.login(body)
				.then(async data => {
					const response = await data.json()
					if (data.status == 200) {
						if (response.errorCode == 0) {
							setCookie("token", response.token, MINUTES59)
							window.location.replace(API.Dashboard)
						} else {
							Utils.responseError("Lỗi!", "Sai email hoặc mật khẩu", "error")
						}
					} else Utils.responseErrorHandler(response)
				})
				.catch(Utils.fetchErrorHandler)
		}
	})
}

function register(body) {
	Utils.responseError("Lỗi!", "Sai email hoặc mật khẩu", "error")

	Utils.newSwal.fire({
		title: "Đang đăng kí",
		text: "Vui lòng chờ . . .",
		didOpen: () => {
			Swal.showLoading()
			var date = new Date(body.dob);
			body.dob = date.toISOString(); //will return an ISO representation of the date
			API.register(body)
				.then(async data => {
					const response = await data.json()
					if (data.status == 200) {
						if (response.errorCode == 0) {
							setCookie("token", response.token, MINUTES59)
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
							Utils.responseError("Lỗi!", "Đã gặp lỗi trong quá trình đăng kí, vui lòng liên hệ admin để hỗ trợ!", "error")
						}
					} else {
						Utils.responseErrorHandler(response)
					}
				})
				.catch(Utils.fetchErrorHandler)
		}
	})
}

