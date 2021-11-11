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
	Utils.newSwal.fire({
		title: "Đang đăng nhập",
		text: "Vui lòng chờ . . .",
		didOpen: () => {
			Swal.showLoading()
			API.login(body)
				.then(async data => {
					const response = await data.json()
					if (data.status == 200) {
						if (response.errors == null) {
							setCookie("token", response.token, MINUTES59)
							window.location.replace(API.Dashboard)
						} else {
							console.log(response.error);
							Utils.responseError("Lỗi!", "Sai email hoặc mật khẩu", "error")
						}
					} else Utils.responseErrorHandler(response)
				})
				.catch(Utils.fetchErrorHandler)
		}
	})
}

function register(body, status) {
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
							if (status)
								Utils.responseError("Lỗi!", response.message, "error")
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


function renderButton() {
	gapi.signin2.render('gSignIn', {
		'scope': 'profile email',
		'width': 240,
		'height': 50,
		'longtitle': true,
		'theme': 'dark',
		'onsuccess': onSuccess,
		'onfailure': onFailure
	});
}

// Sign-in success callback
function onSuccess(Response) {

	try {
		const loginForm = {
			token: Response.Zb.id_token,
			Validator: "authenticator"
		}

		const loginData = googleLoginUser(loginForm);
		setCookie("token", loginData.token, MINUTES59);

	} catch (error){
		console.log(error)
	}
}

const googleLoginUser = async (userForm) => {
	try {
		const response = await API.tokenExchange(userForm)
		return response.data;
	} catch (error) {
		if (error.response.data)
			return error.response.data;
		else 
			return {success: false, message: error.message};
	}
}


































// Sign-in failure callback
function onFailure(error) {
	alert("Đã xảy ra lỗi trong quá trình Đăng Nhập, Vui Lòng thử lại! ")
}


$(document).ready(() => {

	renderButton();

	if (getCookie('dalogout') == 1) {
		// signOut()
	}
	$('#login-google').click(() => {
		console.log("đời");
	})
	$('#login').click(() => {
		$("form").submit(event => {
			event.preventDefault()
			if ($("form").valid()) {
				const body = serializeArrToObject($("form").serializeArray())
				if (window.login) login(body)
				else if (window.register) register(body, true)
			}
		})
	})
	$('#register').click(() => {
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
})