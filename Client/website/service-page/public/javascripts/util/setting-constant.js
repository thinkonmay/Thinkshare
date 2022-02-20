import * as API from "../util/api.js"
import * as Utils from "../util/utils.js"
import * as CheckDevice from "../util/checkdevice.js"

var updatePassword =
{
    Old: "",
    New: ""
};


var body = await (await API.getInfor()).json();

export function Codec(key) {
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

export function DecodeCodec(key) {
    switch (key) {
        case 0:
            return "H265"
        case 1:
            return "H264"
        case 2:
            return "VP8"
        case 3:
            return "VP9"
        case 4:
            return "OPUS"
        case 5:
            return "MP3"
    }
}

export function CoreEngine(key) {
    switch (key) {
        case "GSTREAMER":
            return 0
        case "CHROME":
            return 1
    }
};

export function DecodeCoreEngine(key) {
    switch (key) {
        case 0:
            return "GSTREAMER"
        case 1:
            return "CHROME"
    }
}

export function DeviceType(key) {
    switch (key) {
        case "WEB_APP":
            return 0
        case "WINDOW_APP":
            return 1
        case "LINUX_APP":
            return 2
        case "MAC_OS_APP":
            return 3
        case "ANDROID_APP":
            return 4
        case "IOS_APP":
            return 5
    }
};

export function DecodeDeviceType(key) {
    switch (key) {
        case 0:
            return "WEB_APP"
        case 1:
            return "WINDOW_APP"
        case 2:
            return "LINUX_APP"
        case 3:
            return "MAC_OS_APP"
        case 4:
            return "ANDROID_APP"
        case 5:
            return "IOS_APP"
    }
}

export function QoEMode(key) {
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
    }
}

export function DecodeQoeMode(key) {
    switch (key) {
        case 0:
            return "ULTRA_LOW_CONST"
        case 1:
            return "LOW_CONST"
        case 2:
            return "MEDIUM_CONST"
        case 3:
            return "HIGH_CONST"
        case 4:
            return "VERY_HIGH_CONST"
        case 5:
            return "ULTRA_HIGH_CONST"
    }
}


export function DecodeResolution(display) {
    if (display.screenWidth == 1920 &&
        display.screenHeight == 1080) {
        return "FullHD";
    } else if (display.screenWidth == 2560 &&
        display.screenHeight == 1440) {
        return "2K";
    } else if (display.screenWidth == 3840 &&
        display.screenHeight == 2160) {
        return "4K";
    }
}


export async function updateSetting(display) {
    API.setSetting(display)
        .then(async data => {
            if (data.status == 200) {} else {
                Utils.responseError("Lỗi!", "Thay đổi không thành công, vui lòng kiểm tra lại thông tin", "error")
            }
        })
}


export function 
prepare_user_setting()
{
    $("#usernameCtrler").attr("placeholder", body.userName)
    $("#fullnameCtrler").attr("placeholder", body.fullName)
    $("#jobsCtrler").attr("placeholder", body.jobs)
    $("#phonenumberCtrler").attr("placeholder", body.phoneNumber)
    $("#genderCtrler").val(body.gender)
    $("#dobCtrler").val((body.dateOfBirth).substring(0, 10))

    $("#usernameCtrler").on("change", function() {
        body.username = this.value;
    });
    $("#fullnameCtrler").on("change", function() {
        body.fullname = this.value;
    })
    $("#jobsCtrler").on("change", function() {
        body.jobs = this.value;
    })
    $("#phonenumberCtrler").on("change", function() {
        body.phonenumber = this.value
    })
    $("#genderCtrler").on("change", function() {
        body.gender = this.value
    })
    $("#dobCtrler").on("change", function() {
        body.dob = (this.value)
        body.dob = new Date(body.dob).toISOString().substring(0, 10)
    })
    $("#avatarCtrler").on("change", function() {
        body.avatar = this.value
    })

    $("#newPasswordCtrler").on("change", function() {
        updatePassword.New = this.value
    })
    $("#oldPasswordCtrler").on("change", function() {
        updatePassword.Old = this.value
    })

    $("#langVN").on("change", function() {
        window.location = '/dashboard/vi'
    })
    $("#langEN").on("change", function() {
        window.location = '/dashboard/en'
    })



    $('[name="resolutionOptions"]').click(async function() {
        var display = await (await API.getSetting()).json();
        var value = $(this).find("input").val();
        switch (value) {
            case "FullHD":
                display.screenWidth = 1920;
                display.screenHeight = 1080;
                break;
            case "2K":
                display.screenWidth = 2560;
                display.screenHeight = 1440;
                break;
            case "4K":
                display.screenWidth = 3840;
                display.screenHeight = 2160;
                break;
        }
        await updateSetting(display);
    });
    $('[name="bitrate"]').click(async function() {
        var display = await (await API.getSetting()).json();
        var value = $(this).find("input").val();
        display.mode = QoEMode(value);
        await updateSetting(display);
    });
    $('[name="audioOptions"]').click(async function() {
        var display = await (await API.getSetting()).json();
        var value = $(this).find("input").val();
        display.audioCodec = Codec(value)
        await updateSetting(display);
    });
    $('[name="videoOptions"]').click(async function() {
        var display = await (await API.getSetting()).json();
        var value = $(this).find("input").val();
        display.videoCodec = Codec(value);
        await updateSetting(display);
    });
    $('[name="optionsRemoteCore"]').click(async function() {
        var display = await (await API.getSetting()).json();
        var value = $(this).find("input").val();
        display.engine = CoreEngine(value);
        await updateSetting(display);
    })

    $('#submitChangePasswordCtrler').click(() => {
        Utils.newSwal.fire({
            title: "Updating",
            text: "Wait a minute",
            didOpen: () => {
                API.updatePassword(updatePassword)
                    .then(async data => {
                        const response = await data.json()
                        if (response.status == 200) {
                            if (response.errors == null) {
                                Utils.newSwal.fire({
                                    title: "Success!",
                                    text: "User information has been updated successfully",
                                    icon: "success",
                                })
                            } else {
                                Utils.responseError(response.errors[0].code, response.errors[0].description, "error")
                            }
                        } else {
                            Utils.responseError("Error!", "The change has failed, check your information and try again.", "error")
                        }
                    })
                    .catch()
            }
        })
    });

    $('#submitChangeInfoCtrler').click(() => {
        Utils.newSwal.fire({
            title: "Updating",
            text: "Wait a minute",
            didOpen: () => {
                console.log(body)
                API.setInfor(body)
                    .then(async data => {
                        const response = await data
                        if (response.status == 200) {
                            if (response.errors == null) {
                            body = await (await API.getInfor()).json();
                                Utils.newSwal.fire({
                                    title: "Success!",
                                    text: "User information has been updated successfully",
                                    icon: "success",
                                })
                            } else {
                                Utils.responseError(response.errors[0].code, response.errors[0].description, "error")
                            }
                        }
                        else {
                            Utils.responseError("Error!", "The change has failed, check your information and try again.", "error")
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
            didOpen: () => {}
        })
    });


    if (CheckDevice.isElectron()) {
        $('#optionsVideo3').removeAttr('disabled');
        $('#remoteCtrler2').removeAttr('disabled');
    } else {
        // website
    }
}