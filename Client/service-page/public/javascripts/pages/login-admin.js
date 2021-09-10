import * as API from "../util/api.js"
import * as Validates from "../validates/index.js"
import {setCookie} from "../util/cookie.js"

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
			login(body)
		}
	})
	$("form").validate(Validates.login)

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
							window.location.replace(API.Dashboard + "-admin")
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
