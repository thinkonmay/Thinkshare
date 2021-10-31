import * as Setting from "../util/setting.js"
import * as API from "../util/api.js"
import * as Utils from "../util/utils.js"


$(document).ready(() => {

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
        Utils.newSwal.fire({
            title: "Đang đăng kí",
            text: "Vui lòng chờ . . .",
            didOpen: () => {
                Swal.showLoading()
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
    });



})
