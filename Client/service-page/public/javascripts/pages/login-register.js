import * as API from "../util/api.js"
import * as Validates from "../validates/index.js"
import {getCookie, setCookie} from "../util/cookie.js"

const MINUTES59 = 59 * 60 * 1000

const newSwal = Swal.mixin({
	heightAuto: false,
	allowOutsideClick: false,
	allowEscapeKey: false
})

$(document).ready(() => {
	"use strict"

	$("form").submit(event => {
		event.preventDefault()
		if ($("form").valid()) {
			const body = serializeArrToObject($("form").serializeArray())
			if (window.login) login(body)
			else if (window.register) register(body)
		}
	})
	$("form").validate(window.login ? Validates.login : Validates.register)

	const $textInputs = $("input")
	const $submit = $(".submit")
	const handler = function() {
		const $validTextInputs = $("input:valid")
		if ($textInputs.length === $validTextInputs.length) {
			$submit.attr("disabled", null)
		} else {
			$submit.attr("disabled", "")
		}
	}
	$("form :input").keyup(handler)
	$("form :input").change(handler)

	$("#dateOfBirth").focus(function() {
		$(this).attr("type", "date")
	})
})

function serializeArrToObject(serializeArr) {
	const obj = {}
	serializeArr.map(item => (obj[item.name] = item.value))
	return obj
}

function fetchErrorHandler(error) {
	newSwal.fire({
		title: "Lỗi!",
		text: error.message,
		icon: "error"
	})
}

function responseErrorHandler(response) {
	const keys = Object.keys(response.errors)
	const errors = keys.map(key => response.errors[key])
	const msg = keys.map((key, index) => `${key}: ${errors[index]}`).join(", ")
	newSwal.fire({
		title: "Lỗi!",
		text: msg,
		icon: "error"
	})
}

function register(body) {
	newSwal.fire({
		title: "Đang đăng kí",
		text: "Vui lòng chờ . . .",
		didOpen: () => {
			Swal.showLoading()
			API.register(body)
				.then(async data => {
					const response = await data.json()
					if (data.status == 200) {
						if (response.errorCode == 0) {
							setCookie("token", response.token, MINUTES59)
							newSwal.fire({
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
							newSwal.fire({
								title: "Lỗi!",
								text:
									"Đã gặp lỗi trong quá trình đăng kí, vui lòng liên hệ admin để hỗ trợ!",
								icon: "error"
							})
						}
					} else {
						responseErrorHandler(response)
					}
				})
				.catch(fetchErrorHandler)
		}
	})
}

function login(body) {
	newSwal.fire({
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
							setCookie("cap", "{\"Mode\":\"ultra high\", \"AudioCodec\": \"opus\", \"VideoCodec\": \"h264\", \"screenWidth\": 2650, \"screenHeight\": 1400}", 9999999999999999999)
							console.log(JSON.stringify(getCookie("cap")));		
							/// How to convert to JSON 
							/// var cap = getCookie("cap");
							//  var parse = JSON.parse(cap);
							// use "parse" like json 
							// For ex: parse["mode"]

							window.location.replace(API.Dashboard)
						} else {
							newSwal.fire({
								title: "Lỗi!",
								text: "Sai email hoặc mật khẩu!"
							})
						}
					} else responseErrorHandler(response)
				})
				.catch(fetchErrorHandler)
		}
	})
}
