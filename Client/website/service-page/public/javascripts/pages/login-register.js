/**
 * TODO: Handle all method login/ register
 */
import * as API from "../util/api.js"
import * as Validates from "../validates/index.js"
import { getCookie, setCookie } from "../util/cookie.js"
import * as Utils from "../util/utils.js"
import { isElectron } from "../util/checkdevice.js"
const HOUR5 = 5 * 60 * 60 * 1000;

/**
 * * Get Value from tag_name
 * @param {Array} serializeArr 
 * @returns obj of tag_name and value => item
 */
function serializeArrToObject(serializeArr) {
    const obj = {}
    serializeArr.map(item => (obj[item.name] = item.value))
    return obj
}

/**
 * TODO: Login with default form
 * @param {LoginModel} body 
 */
function login(body) {
    Utils.newSwal.fire({
        title: "Đang đăng nhập",
        text: "Vui lòng chờ . . .",
        didOpen: async () => {
            Swal.showLoading()
            var response = await (await API.login(body)).json();
            setCookie("token", response.token, HOUR5);
            Utils.newSwal.fire({
                title: "Successfully!",
                text: "Redirecting to the dashboard",
                icon: "success",
            })

            setTimeout(() => { window.location.href = "/dashboard" }, 2000)
        }
    })
}

/**
 * TODO: Register with default form
 * @param {RegisterModel} body 
 */
function register(body) {
    Utils.newSwal.fire({
        title: "Đang đăng kí",
        text: "Vui lòng chờ . . .",
        didOpen: async () => {
            Swal.showLoading();
            var response = await (await API.register(body)).json();
            setCookie("token", response.token, HOUR5);
            Utils.newSwal.fire({
                title: "Successfully!",
                text: "Redirecting to the dashboard",
                icon: "success",
            })

            setTimeout(() => { window.location.href = "/dashboard" }, 2000)
        }
    })
}

/**
 * Implement Button Login google with gapi
 */
export async function GoogleLogin() {
    var myParams = {
        'clientid': '610452128706-mplpl7mhld1u05p510rk9dino8phcjb8.apps.googleusercontent.com',
        'cookiepolicy': 'none',
        'callback': loginCallback,
        'approvalprompt': 'force',
        'scope': 'profile email openid',
    };
    await gapi.auth.signIn(myParams)
}

/**
 * TODO: The callBack Function will receive response from Google API when Execute Login
 * @param {AuthResult} result 
 */
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

/**
 * TODO: Handle Login with Google (from Website and Electron)
 * @param {Object} userForm 
 */
const googleLoginUser = async (userForm) => {
    Utils.newSwal.fire({
        title: "Signing",
        text: "Wait a minute . . .",
        didOpen: async () => {
            var response = await (await API.tokenExchange(userForm)).json();
            if (response.errors == null) {
                var str = window.location.pathname;
                if (str == '/token-auth') {
                    window.location.href = `https://service.thinkmay.net/copy-token?=${response.token}`
                } else {
                    setCookie("token", response.token, HOUR5)
                    window.location.replace(API.Dashboard)
                }
            }
        }
    })
}

/**
 *  TODO: Handle  Login with Google failure callback
 */
function onFailure() {
    alert("Login Error an unexpected error occurred please try logging in again")
}


$(document).ready(() => {
    /**
     * Setup Auto authorize with url have token (for Electron Login with Google)
     */
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
    /**
     * TODO: Disable when Button 'Enter' is press -> Login, Register pass authorize
     */
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