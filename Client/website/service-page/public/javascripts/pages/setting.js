import * as Setting from "../util/setting.js"


$(document).ready(() => {

    let display  = new Object();
    $('[name="resolutionOptions"]').click(function() {  
        display.resolution =  $(this).find("input").val()
    });

    $('[name="bitrateOptions"]').click(function() {  
        display.bitrate =  $(this).find("input").val()
    });
    $('[name="audioOptions"]').click(function() {  
        display.audio =  $(this).find("input").val()
    });
    $('[name="videoOptions"]').click(function() {  
        display.video =  $(this).find("input").val()
    });
    $('[name="platformOptions"]').click(function() {  
        display.platform =  $(this).find("input").val()
    });
    
    $('#submitDisplayCtrler').click(() => {
        Setting.Mode(display.bitrate);
        Setting.AudioCodec(display.audio);
        Setting.VideoCodec(display.video);
        Setting.mapVideoRes(display.resolution);
        Setting.Platform(display.platform);
    });

    

})
