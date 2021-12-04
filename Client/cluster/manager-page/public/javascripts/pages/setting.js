import * as API from "../util/api.js"
import * as Utils from "../util/utils.js"
import * as CheckDevice from "../util/checkdevice.js"


$(document).ready(() => {
    let idDisplay;
    let remoteCore;
    API.getInfor().then(async data => {
        let body = await data.json()
        idDisplay = body.defaultSetting['id'];  
        $("#usernameCtrler").attr("placeholder", body.userName)
        $("#fullnameCtrler").attr("placeholder", body.fullName)
        $("#jobsCtrler").attr("placeholder", body.jobs)
        $("#phonenumberCtrler").attr("placeholder", body.phoneNumber)
        $("#genderCtrler").val(body.gender)
        $("#dobCtrler").val((body.dateOfBirth).substring(0, 10))
    })
    let body = {}
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
    let display = {};
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
    $('#remoteCoreOption1').on('change', function () {
        remoteCore = 1
    })
    $('#remoteCoreOption2').on('change', function () {
        remoteCore = 2
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
                let body = {}
                body.defaultSetting_id = parseInt(idDisplay);
                body.defaultSetting_audioCodec = parseInt(display.audio);
                body.defaultSetting_videoCodec = parseInt(display.video)
                body.defaultSetting_mode = parseInt(display.bitrate)
                switch (parseInt(display.resolution)) {
                    case 0: body.defaultSetting_screenWidth = 1920;
                        body.defaultSetting_screenHeight = 1080;
                        break;
                    case 1:
                        body.defaultSetting_screenWidth = 2560;
                        body.defaultSetting_screenHeight = 1440;
                        break;
                    case 2:
                        body.defaultSetting_screenWidth = 3840;
                        body.defaultSetting_screenHeight = 2160;
                        break;
                }
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


    $('#sumbitRemoteCoreCtrler').click(() => {
        console.log(parseInt(remoteCore))
        Utils.newSwal.fire({
            title: "Đang đăng kí",
            text: "Vui lòng chờ . . .",
            didOpen: () => {
                let body = {}
                body.defaultSetting_device = parseInt(remoteCore)
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


    if (CheckDevice.isElectron()) {
        $('#optionsVideo3').removeAttr('disabled');
        $('#remoteCtrler2').removeAttr('disabled');
    } else {
        // website
    }


})
