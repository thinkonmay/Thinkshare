/**
 * TODO: handle access_token when login with google on electron
 */
$(document).ready(() => {
    let access_token = String(window.location.href).slice(String(window.location.href).indexOf('?')+2)
    document.getElementById('token').innerHTML = access_token
})
