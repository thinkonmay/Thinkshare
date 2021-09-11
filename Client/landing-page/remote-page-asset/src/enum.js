var 
applyTimestamp = (msg) => {
    var now = new Date();
    var ts = now.getHours() + ":" + now.getMinutes() + ":" + now.getSeconds();
    return "[" + ts + "]" + " " + msg;
}


var Module = {"CORE_MODULE":0,"CLIENT_MODULE":1,"LOADER_MODULE":2,"AGENT_MODULE":3,"HOST_MODULE":4};
var HidOpcode = {"KEYUP":0,"KEYDOWN":1,"MOUSE_WHEEL":2,"MOUSE_MOVE":3,"MOUSE_UP":4,"MOUSE_DOWN":5};
var Opcode = {"SESSION_INFORMATION":0,"REGISTER_SLAVE":1,"SLAVE_ACCEPTED":2,"DENY_SLAVE":3,"REJECT_SLAVE":4,"SESSION_INITIALIZE":5,"SESSION_TERMINATE":6,"RECONNECT_REMOTE_CONTROL":7,"DISCONNECT_REMOTE_CONTROL":8,"QOE_REPORT":9,"RESET_QOE":10,"COMMAND_LINE_FORWARD":11,"SESSION_CORE_EXIT":12,"ERROR_REPORT":13,"NEW_COMMAND_LINE_SESSION":14,"END_COMMAND_LINE_SESSION":15,"FILE_TRANSFER_SERVICE":16,"CLIPBOARD_SERVICE":17};
var QoEMode = {"ULTRA_LOW_CONST":1,"LOW_CONST":2,"MEDIUM_CONST":3,"HIGH_CONST":4,"VERY_HIGH_CONST":5,"ULTRA_HIGH_CONST":6,"SEGMENTED_ADAPTIVE_BITRATE":7,"NON_OVER_SAMPLING_ADAPTIVE_BITRATE":8,"OVER_SAMPLING_ADAPTIVE_BITRATE":9,"PREDICTIVE_ADAPTIVE_BITRATE":10};
var Codec = {"CODEC_H265":0,"CODEC_H264":1,"CODEC_VP9":2,"OPUS_ENC":3,"AAC_ENC":4};