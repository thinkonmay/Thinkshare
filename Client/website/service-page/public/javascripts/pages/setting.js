import * as API from "../util/api.js"
import * as Utils from "../util/utils.js"
import * as CheckDevice from "../util/checkdevice.js"

export function Codec (key)
{
    switch (key) {
    
    case "H265":
        return 0
    case "H264":
        return 1
    case "VP8":
        return 2
    case "VP9":
        return 3
    case "OPUS":
        return 4
    case "MP3":
        return 5
    }
};

export function CoreEngine(key)  
{
    switch (key) {
    case "GSTREAMER":
        return 0
    case "CHROME":
        return 1
    }
};


export function DeviceType(key) 
{
    switch (key) {
    case "WEB_APP":
        return 1
    case "WINDOW_APP":
        return 2
    case "LINUX_APP":
        return 3
    case "MAC_OS_APP":
        return 4
    case "ANDROID_APP":
        return 5
    case "IOS_APP":
        return 6
    }
};

export function QoEMode  (key)
{
    switch (key) {
    case "ULTRA_LOW_CONST":
        return 0;
    case "LOW_CONST":
        return 1;
    case "MEDIUM_CONST":
        return 2;
    case "HIGH_CONST":
        return 3;
    case "VERY_HIGH_CONST":
        return 4;
    case "ULTRA_HIGH_CONST":
        return 5;
    case "SEGMENTED_ADAPTIVE_BITRATE":
        return 6;
    case "NON_OVER_SAMPLING_ADAPTIVE_BITRATE":
        return 7;
    case "OVER_SAMPLING_ADAPTIVE_BITRATE":
        return 8;
    case "PREDICTIVE_ADAPTIVE_BITRATE":
        return 9;
    }
}

$(document).ready(async () => {
    var body = await (await API.getInfor()).json();
    $("#usernameCtrler").attr("placeholder", body.userName)
    $("#fullnameCtrler").attr("placeholder", body.fullName)
    $("#jobsCtrler").attr("placeholder", body.jobs)
    $("#phonenumberCtrler").attr("placeholder", body.phoneNumber)
    $("#genderCtrler").val(body.gender)
    $("#dobCtrler").val((body.dateOfBirth).substring(0, 10))

    $("#usernameCtrler").on("change", function () {
        body.username = this.value;
    });
    $("#fullnameCtrler").on("change", function () {
        body.fullname = this.value;
    })
    $("#jobsCtrler").on("change", function () {
        body.jobs = this.value;
    })
    $("#phonenumberCtrler").on("change", function () {
        body.phonenumber = this.value
    })
    $("#genderCtrler").on("change", function () {
        body.gender = this.value
    })
    $("#dobCtrler").on("change", function () {
        body.dob = (this.value)
        body.dob = new Date(body.dob).toISOString().substring(0, 10)
    })
    $("#avatarCtrler").on("change", function() {
        body.avatar = this.value
    })

    $("#langVN").on("change", function() {
        window.location = '/dashboard/vi'
    })

    $("#langEN").on("change", function() {
        window.location = '/dashboard/en'
    })

    var display = await (await API.getSetting()).json();
    $('[name="resolutionOptions"]').click(function () {
        var value = $(this).find("input").val();
        switch (value) {
            case "FullHD": 
                display.screenWidth= 1920;
                display.screenHeight= 1080;
                break;
            case "2K":
                display.screenWidth = 2560;
                display.screenHeight = 1440;
                break;
            case "4K":
                display.screenWidth= 3840;
                display.screenHeight = 2160;
                break;
        }
    });
    $('[name="bitrateOptions"]').click(function () {
        var value = $(this).find("input").val();
        display.mode = QoEMode(value);
    });
    $('[name="audioOptions"]').click(function () {
        var value = $(this).find("input").val();
        display.audioCodec = Codec(value)
    });
    $('[name="videoOptions"]').click(function () {
        var value = $(this).find("input").val();
        display.videoCodec = Codec(value);
    });
    $('#remoteCoreOption1').click(function () {
        var value = $(this).find("input").val();
        display.engine = CoreEngine(value);
    })


    $('#submitChangeInfoCtrler').click(() => {
        Utils.newSwal.fire({
            title: "Đang đăng kí",
            text: "Vui lòng chờ . . .",
            didOpen: () => {
                console.log(body)
                API.setInfor(body)
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
        Utils.newSwal.fire({
            title: "Đang đăng kí",
            text: "Vui lòng chờ . . .",
            didOpen: () => {
                API.setSetting(display)
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


    if (CheckDevice.isElectron()) {
        $('#optionsVideo3').removeAttr('disabled');
        $('#remoteCtrler2').removeAttr('disabled');
    } else {
        // website
    }


})
