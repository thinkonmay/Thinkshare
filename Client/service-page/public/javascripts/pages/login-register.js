import * as API from "../util/api.js"
import * as Validates from "../validates/index.js"
import { setCookie } from "../util/cookie.js"
import * as Utils from "../util/utils.js"
const MINUTES59 = 59 * 60 * 1000;

(function ($) {
	"use strict"

	/*==================================================================
	[ Validate ]*/

	renderButton();

	$('#login-google').click(() => {
		function onSignIn(googleUser) {
			var profile = googleUser.getBasicProfile();
			console.log('ID: ' + profile.getId()); // Do not send to your backend! Use an ID token instead.
			console.log('Name: ' + profile.getName());
			console.log('Image URL: ' + profile.getImageUrl());
			console.log('Email: ' + profile.getEmail()); // This is null if the 'email' scope is not present.
		}
		onSignIn(auth2.isSignedIn.get())
	})
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
})(jQuery)

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

	console.log(body);
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
							Utils.responseError("Lỗi!", response.message, "error")
						}
					} else {
						Utils.responseErrorHandler(response)
					}
				})
				.catch(Utils.fetchErrorHandler)
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
function onSuccess(googleUser)  {
	// Get the Google profile data (basic)
	//var profile = googleUser.getBasicProfile();

	// Retrieve the Google account data
	 gapi.client.load('oauth2', 'v2', function ()  {
		var request =  gapi.client.oauth2.userinfo.get({
			'userId': 'me'
		});
		request.execute(function (resp) {

			let userName = ((resp.given_name).normalize('NFD').replace(/[\u0300-\u036f]/g, '').replace(/đ/g, 'd').replace(/Đ/g, 'D')).split(" ")[0];

			doSth(resp.email, userName, resp.given_name.normalize('NFD').replace(/[\u0300-\u036f]/g, '').replace(/đ/g, 'd').replace(/Đ/g, 'D'), resp.link +"123")
			// given_name
			// picture
			// email
			// gender
			// locale
			// link
		});
	});
}

// Sign-in failure callback
function onFailure(error) {
	alert("Đã xảy ra lỗi trong quá trình Đăng Nhập, Vui Lòng thử lại! ")
}

// Sign out the user
function signOut() {
	var auth2 = gapi.auth2.getAuthInstance();
	auth2.signOut().then(function () {
		document.getElementsByClassName("userContent")[0].innerHTML = '';
		document.getElementsByClassName("userContent")[0].style.display = "none";
		document.getElementById("gSignIn").style.display = "block";
	});

	auth2.disconnect();
}

function doSth(email, userName, fullName, sth) {
	// 	dob: "2021-09-08"
// email: "thienvanlea2@gmail.com"
// fullname: "Lê Văn Thiện"
// jobs: "hacker123123"
// password: "Lienminh1"
// phonenumber: "01235667869"
// repassword: "Lienminh1"
// username: "epitchi1"
	
	register({
		email: email,
		username: userName + "gg",
		fullname: fullName + (Math.floor((Math.random() * 9999) + 1000)).toString(),
		dob: "2021-09-08",
		jobs: "None",
		phonenumber: (Math.floor((Math.random() * 849999999) + 841111111)).toString(),
		password: sth,
		repassword: sth,
	})
	login({
		username: userName + "gg",
		password: sth
	})
}