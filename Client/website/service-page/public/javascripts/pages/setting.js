import * as Setting from "../util/setting.js"
import * as API from "../util/api.js"
import * as Utils from "../util/utils.js"
import * as CheckDevice from "../util/checkdevice.js"


$(document).ready(() => {
    API.getInfor().then(async data => {
        let body = await data.json()
        $("#usernameCtrler").attr("placeholder", body.userName)
        $("#fullnameCtrler").attr("placeholder", body.fullName)
        $("#jobsCtrler").attr("placeholder", body.jobs)     
        $("#phonenumberCtrler").attr("placeholder", body.phoneNumber)
        $("#genderCtrler").val(body.gender)
        $("#dobCtrler").val((body.dateOfBirth).substring(0, 10))
    })

    let display = new Object();
    let info = new Object();
    $('[name="resolutionOptions"]').click(function () {
        display.resolution = $(this).find("input").val()
    });

    $('[name="bitrateOptions"]').click(function () {
        display.bitrate = $(this).find("input").val()
    });
    $('[name="audioOptions"]').click(function () {
        display.audio = $(this).find("input").val()
    });
    $('[name="videoOptions"]').click(function () {
        display.video = $(this).find("input").val()
    });
    $('[name="platformOptions"]').click(function () {
        display.platform = $(this).find("input").val()
    });

    $("#usernameCtrler").on("change", function () {
        info.username = this.value;
    });

    $('#submitChangeInfoCtrler').click(() => {
        body = {
            
        }
        Utils.newSwal.fire({
            title: "Đang đăng kí",
            text: "Vui lòng chờ . . .",
            didOpen: () => {
                API.setInfor(info.username)
                    .then(async data => {
                        if (data.status == 200) {
                            Utils.newSwal.fire({
                                title: "Thành công!",
                                text: "Thông tin của bạn đã được cập nhật",
                                icon: "success",
                            })
                        } else {
                            Utils.responseError("Lỗi!", "Thay đổi không thành công, vui lòng kiểm tra lại thông tin", "error")
                        }
                    })
                    .catch(status ? Utils.fetchErrorHandler : "")
            }
        })

    });

    $('#submitDisplayCtrler').click(() => {
        Setting.Mode(display.bitrate);
        Setting.AudioCodec(display.audio);
        Setting.VideoCodec(display.video);
        Setting.mapVideoRes(display.resolution);
        Setting.Platform(display.platform);
        Utils.newSwal.fire({
            title: "Thành công!",
            text: "Cấu hình của bạn đã được cập nhật",
            icon: "success",
        })
        console.log('asdasd')



    });


    if (CheckDevice.isElectron()) {
        $('#optionsVideo3').removeAttr('disabled');
        $('#optionsPlatform2').removeAttr('disabled');
    } else {
        // website
    }


})
