import * as API from "../util/api.js"
import * as Validates from "../validates/index.js"
import { getCookie, setCookie } from "../util/cookie.js"
import * as Utils from "../util/utils.js"
import { isElectron } from "../util/checkdevice.js"
import { convertDate } from "../util/date-time-convert.js"
const HOUR5 = 5 * 60 * 60 * 1000;

let authorizeElectron = false;

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
                            setCookie("token", response.token, HOUR5)
                            window.location.replace(API.Dashboard)
                        } else {
                            console.log(response.error);
                            Utils.responseError(response.errors[0].code, response.errors[0].description, "error")
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
            body.dob = convertDate(body.dob)
            body.jobs = body.jobs == null ? "nosetJobs" : body.jobs,
                API.register(body)
                    .then(async data => {
                        const response = await data.json()
                        if (data.status == 200) {
                            if (response.errors == null) {
                                setCookie("token", response.token, HOUR5)
                                Utils.newSwal.fire({
                                    title: "Successfully!",
                                    text: "Redirecting to the dashboard",
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

export async function GoogleLogin(fromElectron) {
    authorizeElectron =  fromElectron
    var myParams = {
        'clientid': '610452128706-mplpl7mhld1u05p510rk9dino8phcjb8.apps.googleusercontent.com',
        'cookiepolicy': 'none',
        'callback': loginCallback,
        'approvalprompt': 'force',
        'scope': 'profile email openid',
    };

    await gapi.auth.signIn(myParams)
}

function loginCallback(result) {
    console.log(result)
    if (result['status']['signed_in']) {
        try {
            const loginForm = {
                token: result.id_token,
                Validator: "authenticator"
            }
            var logout = getCookie("logout");
            if (logout == "false" || logout == undefined || logout == "") {
                googleLoginUser(loginForm)
            }
        } catch (error) {
            console.log(error)
        }
    } else {
        onFailure();
    }
}

const googleLoginUser = async (userForm) => {
    Utils.newSwal.fire({
        title: "Signing",
        text: "Wait a minute . . .",
        didOpen: () => {
            API.tokenExchange(userForm)
                .then(async data => {
                    const response = await data.json()
                    if (data.status == 200) {
                        if (response.errors == null) {
                            setCookie("token", response.token, HOUR5)
                            if (authorizeElectron ==  true) {
                                window.location.href = `https://service.thinkmay.net/copy-token?=${response.token}`
                            } else
                                window.location.replace(API.Dashboard)
                        } else {
                            console.log(response.error);
                            Utils.responseError("Error!", "Login Error an unexpected error occurred please try logging in again", "error")
                        }
                    } else Utils.responseErrorHandler(response)
                })

        }
    })
}

// Sign-in failure callback
function onFailure() {
    alert("Login Error an unexpected error occurred please try logging in again")
}

function openLinkInIE(url) {
    window.location.replace(url, "_blank");
}

$(document).ready(() => {
    let access_token = window.location.href
    if (String(access_token).length > 50) {
        setCookie('token', access_token.slice(access_token.slice(access_token.indexOf('=')+1)), HOUR5)
        window.location.replace(API.Dashboard)
    }

    $('#gSignIn').click(() => {
        setCookie("logout", "false", 0)
        if (isElectron()) {
            window.location.assign(`loginThinkmay://`);
            $('#gSignIn').html('')

            $('#formLogin').attr('style', 'display: none')
            $('#formRegister').attr('style', 'display: none')
            $('#authorizeForm ').removeAttr('style')
            // openLinkInIE("https://service.thinkmay.net/token-auth")
            /// create box to set token id
        } else
            GoogleLogin(false);
    })

    $('#authorize').click(() =>{
        let token = $('#accessToken').val()
        setCookie('token', token, HOUR5)
        window.location.replace(API.Dashboard)
    })

    $('#login').click(() => {
        $("form").validate(window.login ? Validates.login : Validates.register)
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