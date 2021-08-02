const Opcode =
{
    SESSION_INFORMATION : 1,

    REGISTER_SLAVE : 2,

    SLAVE_ACCEPTED : 3,
    SLAVE_DENY :4 ,

    REJECT_SLAVE : 5,

    UPDATE_SLAVE_STATE :6,

    SESSION_INITIALIZE:7,
    SESSION_TERMINATE:8,
    RECONNECT_REMOTE_CONTROL:9,
    DISCONNECT_REMOTE_CONTROL:10,

    SESSION_INFORMATION_REQUEST:11,

    COMMAND_LINE_FORWARD:12	,

    EXIT_CODE_REPORT:13,
    ERROR_REPORT:14,

    NEW_COMMAND_LINE_SESSION :15,
    END_COMMAND_LINE_SESSION :16,    

    QOE_REPORT:17
}

const HidOpcode = 
{
   KEYUP:0,
   KEYDOWN:1,

   MOUSEDOWN:2,
   MOUSEMOVE:3,
   MOUSEUP:4,
   MOUSEWHEEL:5,

   CHANGE_MEDIA_MODE:6,

   CHANGE_RESOLUTION:7,
   CHANGE_FRAMERATE:8,
   DISPLAY_POINTER:9,

   CLIPBOARD:10,
   FILE:11
}

const Module =
{
   CORE_MODULE:0,
   CLIENT_MODULE:1,
   LOADER_MODULE:2,
   AGENT_MODULE:3,
   HOST_MODULE:4,
}

const Codec =
{
   CODEC_NVH265:0,
   CODEC_H263:1,
   CODEC_VP9:2,
   OPUS_ENC:3
}


var windowdict = new Object();
var windowdict = {};


windowdict["Tab"] = parseInt(0x09);
//   windowdict[""] = parseInt(0x0A);
//   windowdict[""] = parseInt(0x0B);
//   windowdict[""] = parseInt(0x0C);
windowdict["Enter"] = parseInt(0X0D);
//   windowdict[""] = parseInt(0X0E);
//...
windowdict["Pause"] = parseInt(0X13);
windowdict["CapsLock"] = parseInt(0X14);
//   windowdict[""] = parseInt(0X15);
//   windowdict[""] = parseInt(0X16);
//   windowdict[""] = parseInt(0X17);
//   windowdict[""] = parseInt(0X18);
//  windowdict[""] = parseInt(0x19);
//   windowdict[""] = parseInt(0x1A);
windowdict["Escape"] = parseInt(0x1B);
//   windowdict[""] = parseInt(0x1C);
//   windowdict[""] = parseInt(0x1D);
//   windowdict[""] = parseInt(0x1E);
//   windowdict[""] = parseInt(0x1F);
windowdict["Space"] = parseInt(0x20);
windowdict["PageUp"] = parseInt(0x21);
windowdict["PageDown"] = parseInt(0x22);
windowdict["End"] = parseInt(0x23);
windowdict["Home"] = parseInt(0x24);
windowdict["ArrowLeft"] = parseInt(0x25);
windowdict["ArrowUp"] = parseInt(0x26);
windowdict["ArrowRight"] = parseInt(0x27);
windowdict["ArrowDown"] = parseInt(0x28);
//   windowdict[""] = parseInt(0x29);
//   windowdict[""] = parseInt(0x2A);
//   windowdict[""] = parseInt(0x2B);
//   windowdict[""] = parseInt(0x2C);
windowdict["Insert"] = parseInt(0x2D);
windowdict["Delete"] = parseInt(0x2E);
//  windowdict[""] = parseInt(0x2F);
windowdict["Digit0"] = parseInt(0x30);
windowdict["Digit1"] = parseInt(0x31);
windowdict["Digit2"] = parseInt(0x32);
windowdict["Digit3"] = parseInt(0x33);
windowdict["Digit4"] = parseInt(0x34);
windowdict["Digit5"] = parseInt(0x35);
windowdict["Digit6"] = parseInt(0x36);
windowdict["Digit7"] = parseInt(0x37);
windowdict["Digit8"] = parseInt(0x38);
windowdict["Digit9"] = parseInt(0x39);
windowdict["KeyA"] = parseInt(0x41);
windowdict["KeyB"] = parseInt(0x42);
windowdict["KeyC"] = parseInt(0x43);
windowdict["KeyD"] = parseInt(0x44);
windowdict["KeyE"] = parseInt(0x45);
windowdict["KeyF"] = parseInt(0x46);
windowdict["KeyG"] = parseInt(0x47);
windowdict["KeyH"] = parseInt(0x48);
windowdict["KeyI"] = parseInt(0x49);
windowdict["KeyJ"] = parseInt(0x4A);
windowdict["KeyK"] = parseInt(0x4B);
windowdict["KeyL"] = parseInt(0x4C);
windowdict["KeyM"] = parseInt(0x4D);
windowdict["KeyN"] = parseInt(0x4E);
windowdict["KeyO"] = parseInt(0x4F);
windowdict["KeyP"] = parseInt(0x50);
windowdict["KeyQ"] = parseInt(0x51);
windowdict["KeyR"] = parseInt(0x52);
windowdict["KeyS"] = parseInt(0x53);
windowdict["KeyT"] = parseInt(0x54);
windowdict["KeyU"] = parseInt(0x55);
windowdict["KeyV"] = parseInt(0x56);
windowdict["KeyW"] = parseInt(0x57);
windowdict["KeyX"] = parseInt(0x58);
windowdict["KeyY"] = parseInt(0x59);
windowdict["KeyZ"] = parseInt(0x5A);
windowdict["MetaLeft"] = parseInt(0x5B);
//  windowdict[""] = parseInt(0x5C);
//  windowdict[""] = parseInt(0x5D);
//  windowdict[""] = parseInt();
//  windowdict[""] = parseInt(0x5F);
// windowdict[""] = parseInt(0x60);
// windowdict[""] = parseInt(0x61);
// windowdict[""] = parseInt(0x62);
// windowdict[""] = parseInt(0x63);
// windowdict[""] = parseInt(0x64);
// windowdict[""] = parseInt(0x65);
// windowdict[""] = parseInt(0x67);
// windowdict[""] = parseInt(0x68);
// windowdict[""] = parseInt(0x69);
// windowdict[""] = parseInt(0x6A);
// windowdict[""] = parseInt(0x6B);
// windowdict[""] = parseInt(0x6C);
// windowdict[""] = parseInt(0x6D);
// windowdict[""] = parseInt(0x6E);
// windowdict[""] = parseInt(0x6F);
windowdict["F1"] = parseInt(0x70);
windowdict["F2"] = parseInt(0x71);
windowdict["F2"] = parseInt(0x72);
windowdict["F4"] = parseInt(0x73);
windowdict["F5"] = parseInt(0x74);
windowdict["F6"] = parseInt(0x75);
windowdict["F7"] = parseInt(0x76);
windowdict["F8"] = parseInt(0x77);
windowdict["F9"] = parseInt(0x78);
windowdict["F10"] = parseInt(0x79);
windowdict["F11"] = parseInt(0x7A);
windowdict["F12"] = parseInt(0x7B);
// windowdict[""] = parseInt(0x7D);
// windowdict[""] = parseInt(0x7E);
// windowdict[""] = parseInt(0x7F);
// windowdict[""] = parseInt(0x80);
// windowdict[""] = parseInt(0x81);
// windowdict[""] = parseInt(0x82);
// windowdict[""] = parseInt(0x83);
// windowdict[""] = parseInt(0x84);
// windowdict[""] = parseInt(0x85);
// windowdict[""] = parseInt(0x86);
// windowdict[""] = parseInt(0x87);
//  windowdict[""] = parseInt(0x90);
windowdict["ScrollLock"] = parseInt(0x91);
windowdict["ShiftLeft"] = parseInt(0xA0);
windowdict["ShiftRight"] = parseInt(0xA1);
windowdict["ControlLeft"] = parseInt(0xA2);
windowdict["ControlRight"] = parseInt(0xA3);
windowdict["ContextMenu"] = parseInt(0xA4);
windowdict["Semicolon"] = parseInt(0xBA);
windowdict["Equal"] = parseInt(0xBB);
windowdict["Comma"] = parseInt(0xBC);
windowdict["Minus"] = parseInt(0xBD);
windowdict["Period"] = parseInt(0xBE);
windowdict["Slash"] = parseInt(0xBF);
windowdict["Backquote"] = parseInt(0xC0);
windowdict["BracketLeft"] = parseInt(0xDB);
windowdict["Backslash"] = parseInt(0xDC);
windowdict["BracketRight"] = parseInt(0xDD);
//  windowdict[""] = parseInt(0xDE);
//  windowdict[""] = parseInt();
//  windowdict[""] = parseInt();
//  windowdict[""] = parseInt();
//  windowdict[""] = parseInt();


function 
convertJavaScriptKeyToWindowKey(javaKey)
{
    return WindowKey = windowdict[javaKey];
}

