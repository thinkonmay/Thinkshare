import * as API from "../util/api.js"
import * as Validates from "../validates/index.js"
import { getCookie, setCookie } from "../util/cookie.js"
import * as Utils from "../util/utils.js"
import { isElectron } from "../util/checkdevice.js"
const HOUR5 = 5 * 60 * 60 * 1000;

let authorizeElectron = isElectron();

function serializeArrToObject(serializeArr) {
    const obj = {}
    serializeArr.map(item => (obj[item.name] = item.value))
    return obj
}

function login(body) {
    Utils.newSwal.fire({
        title: "Đang đăng nhập",
        text: "Vui lòng chờ . . .",
        didOpen: async () => {
            Swal.showLoading()
            var response = await (await API.login(body)).json();
            if (response.token != null && response.errors == null) {
                    setCookie("token", response.token, HOUR5);
                    setTimeout(() => { window.location.href = "/dashboard" }, 2000)
            }
        }
    })
}

function register(body) {
    Utils.newSwal.fire({
        title: "Đang đăng kí",
        text: "Vui lòng chờ . . .",
        didOpen: async () => {
            Swal.showLoading();
            var response = await (await API.register(body)).json();
            if (response.token != null && response.errors == null) {
                Utils.newSwal.fire({
                    title: "Successfully!",
                    text: "Redirecting to the dashboard",
                    icon: "success",
                    didOpen: () => {
                        setCookie("token", response.token, HOUR5);
                        setTimeout(() => { window.location.href = "/dashboard" }, 2000)
                    }
                })
            }
        }
    })
}

export async function GoogleLogin() {
    authorizeElectron = fromElectron
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
        didOpen: async () => {
            var response = await (await API.tokenExchange(userForm)).json();
            if (response.errors == null) {
                if (authorizeElectron == true) {
                    window.location.href = `https://service.thinkmay.net/copy-token?=${response.token}`
                } else {
                    setCookie("token", response.token, HOUR5)
                    window.location.replace(API.Dashboard)
                }
            }
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
        setCookie('token', access_token.slice(access_token.slice(access_token.indexOf('=') + 1)), HOUR5)
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
            GoogleLogin();
    })

    $('#authorize').click(() => {
        let token = $('#accessToken').val()
        setCookie('token', token, HOUR5)
        window.location.replace(API.Dashboard)
    })

    $('#login').click(() => {
        $("form").validate(Validates.login)
        $("form").submit(event => {
            event.preventDefault()
            if ($("form").valid()) {
                const body = serializeArrToObject($("form").serializeArray())
                login(body)
            }
        })
    })
    $('#register').click(() => {
        $("form").validate(Validates.register)
        $("form").submit(event => {
            event.preventDefault()
            if ($("form").valid()) {
                const body = serializeArrToObject($("form").serializeArray())
                register(body)
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
    $("form").keypress(function(e) {
        //Enter key
        if (e.which == 13) {
          return false;
        }
      });      
      
    $("form :input").keyup(handler)
    $("form :input").change(handler)
    $("#dateOfBirth").focus(function () {
        $(this).attr("type", "date")
    })
})