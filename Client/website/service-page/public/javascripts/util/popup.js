import { getCookie, setCookie } from "./cookie.js"

export function
prepare_tutorial_popup()
{  
    if (getCookie("show-tutorial") == "true") {
        $('#checkboxTutorial').attr("checked", true)
    }
    if (getCookie("show-tutorial") != "true") {
        $('#modal-btn3').attr('checked', true)
    }

    $('.modal__checkbox').click(function () {
        if ($('#checkboxTutorial').attr("checked") == 'checked') {
            // have been click this checkbox
            document.getElementById('checkboxTutorial').removeAttribute("checked")
            // $('#checkboxTutorial').removeAttr("checked")
            setCookie("show-tutorial", "false", 99999999999999)
        } else
            if ($('#checkboxTutorial').attr("checked") != 'checked') {
                // click checkbox
                $('#showTutorialState').html("Yes, I won\'t show again")
                $('#checkboxTutorial').attr("checked", true)
                setCookie("show-tutorial", "true", 99999999999999)
            }
    })
}