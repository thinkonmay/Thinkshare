
$(document).ready(() => {
    console.log(window.location.href)
    let access_token = String(window.location.href).slice(String(window.location.href).indexOf('?')+2)
    document.getElementById('token').innerHTML = access_token
})
