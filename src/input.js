/**
 * Copyright 2019 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

/*global GamepadManager*/
/*eslint no-unused-vars: ["error", { "vars": "local" }]*/


class Input {
    /**
     * Input handling for WebRTC web app
     *
     * @constructor
     * @param {Element} [element]
     *    Video element to attach events to
     * @param {function} [send]
     *    Function used to send input events to server.
     */
    constructor(element, sendHid, sendControl) {
        /**
         * @type {Element}
         */
        this.element = element;

        /**
         * @type {function}
         */
        this.sendHid = sendHid;

        /**
         * @type {function}
         */
        this.sendControl = sendControl

        /**
         * @type {boolean}
         */
        this.mouseouseRelative = false;

        /**
         * @type {Object}
         */
        this.mouse = null;

        /**
         * @type {Integer}
         */
        this.buttonMask = 0;

        /**
         * @type {Integer}
         */
        this.x = 0;

        /**
         * @type {Integer}
         */
        this.y = 0;

        /**
         * @type {function}
         */
        this.onmenuhotkey = null;

        /**
         * @type {function}
         */
        this.onfullscreenhotkey = null;

        /**
         * @type {function}
         */
        this.onofferclienthotkey = null;

        /**
         * @type {function}
         */
        this.onconnectserverhotkey = null;

        /**
         * @type {function}
         */
        this.ongetstatusserverhotkey = null;

        /**
         * List of attached listeners, record keeping used to detach all.
         * @type {Array}
         */
        this.listeners = [];

        /**
         * @type {function}
         */
        this.onresizeend = null;

        this.windowdict = {};

        this._initWindowKeyCode();

        // internal variables used by resize start/end functions.
        this._rtime = null;
        this._rtimeout = false;
        this._rdelta = 200;
    }

    /**
     * Handles mouse button and motion events and sends them to WebRTC app.
     * @param {MouseEvent} event
     */
    _mouseButtonMovement(event) {
        const down = (event.type === 'mousedown' ? 1:0);
        var mtype = 'm';


        if (event.type === 'mousedown' ? 1 : 0)
        var mtype = "m";

        if (event.type === 'mousemove' && !this.mouse) return;

        if (!document.pointerLockElement) {
            if (this.mouseouseRelative)
                event.target.requestPointerLock();
        }

        // Hotkey to enable pointer lock, CTRL-SHIFT-LeftButton
        if (down && event.button === 0 && event.ctrlKey && event.shiftKey) {
            event.target.requestPointerLock();
            return;
        }

        if (document.pointerLockElement) {
            mtype = "m2";
            this.x = event.movementX;
            this.y = event.movementY;
        } else if (event.type === 'mousemove') {
            this.x = this._clientToServerX(event.clientX);
            this.y = this._clientToServerY(event.clientY);
        }

        if (event.type === 'mousedown' || event.type === 'mouseup') 
        {
            var mask = 1 << event.button;
            if (down) {
                this.buttonMask |= mask;
            } else {
                this.buttonMask &= ~mask;
            }
        }
        
        var dwFlags = 0;
        var Relative = (mtype === "m2" ? true : false);               

        if(this.buttonMask == 0){if(event.type === 'mousedown'){dwFlags = 2}}
        if(this.buttonMask == 0){if(event.type === 'mouseup'){dwFlags = 4}}
        if(this.buttonMask == 1){if(event.type === 'mousedown'){dwFlags = 20}}
        if(this.buttonMask == 1){if(event.type === 'mouseup'){dwFlags = 40}}
        if(this.buttonMask == 2){if(event.type === 'mousedown'){dwFlags = 8}}
        if(this.buttonMask == 2){if(event.type === 'mouseup'){dwFlags = 10}}

        var INPUT =
        {
            "Opcode":HidOpcode.MOUSE, //mouse opcode
            "dX":this.x,    //x position of pointer on SLAVE screeen
            "dY":this.y,    //y axis of pointer of SLAVE screen
            "mouseData":0, // do not touch this parameter
            "dwFlags":dwFlags, //this too
            "Relative":Relative //true if this.x and this.y is relative position (position after - position before)
        }
        console.log(JSON.stringify(INPUT));

        this.sendHid(JSON.stringify(INPUT));
    }



    /**
     * Handles mouse wheel events and sends them to WebRTC app.
     * @param {MouseWheelEvent} event
     */
    _mouseWheel(event) 
    {
        var mtype = (document.pointerLockElement ? "m2" : "m");
            
        var dwFlags = 800;
        var Relative = (mtype === "m2" ? true : false);               

        var INPUT =
        {
            "Opcode":HidOpcode.MOUSE,
            "dX":this.x,
            "dY":this.y,
            "mouseData":event.deltaY,
            "dwFlags":dwFlags,
            "Relative":Relative
        }
        //scroll 
        this.sendHid(JSON.stringify(INPUT));
    }

    /**
     * Captures mouse context menu (right-click) event and prevents event propagation.
     * @param {MouseEvent} event
     */
    _contextMenu(event) {
        event.preventDefault();
    }

    /**
     * Captures keyboard events to detect pressing of CTRL-SHIFT hotkey.
     * @param {KeyboardEvent} event
     */
    _key(event) {

        // disable problematic browser shortcuts
        if (event.code === 'F5' && event.ctrlKey ||
            event.code === 'KeyI' && event.ctrlKey && event.shiftKey ||
            event.code === 'F11') {
            event.preventDefault();
            return;
        }

        // capture menu hotkey
        if (event.type === 'keydown' && event.code === 'KeyM' && event.ctrlKey && event.shiftKey) {
            if (document.fullscreenElement === null && this.onmenuhotkey !== null) {
                this.onmenuhotkey();
                event.preventDefault();
            }

            return;
        }

        // capture fullscreen hotkey
        if (event.type === 'keydown' && event.code === 'KeyF' && event.ctrlKey && event.shiftKey) {
            if (document.fullscreenElement === null && this.onfullscreenhotkey !== null) {
                this.onfullscreenhotkey();
                event.preventDefault();
            }
            return;
        }
        
        var IsUp;
        if(event.type === 'keyup'){IsUp = true;}
        if(event.type === 'keydown'){IsUp = false;}

        var Keyboard =
        {
            "Opcode":HidOpcode.KEYBOARD,
            "wVk":this.convertJavaScriptKeyToWindowKey(event.code),
            "IsUp":IsUp,
        }
        console.log(JSON.stringify(Keyboard));
        this.sendHid(JSON.stringify(Keyboard));

    }

    /**
     * Sends WebRTC app command to toggle display of the remote mouse pointer.
     */
    _pointerLock() 
    {
        var Toggle = 
        {
            "Opcode":HidOpcode.DISPLAY_POINTER,
            "IsLock":document.pointerLockElement
        } 
        this.sendControl(JSON.stringify(Toggle));
    }


    /**
     * Sends WebRTC app command to hide the remote pointer when exiting pointer lock.
     */
    _exitPointerLock() {
        document.exitPointerLock();
        // hide the pointer.
        this._pointerLock();
    }

    /**
     * Captures display and video dimensions required for computing mouse pointer position.
     * This should be fired whenever the window size changes.
     */
    _windowMath() {
        const windowW = this.element.offsetWidth;
        const windowH = this.element.offsetHeight;
        const frameW = this.element.videoWidth;
        const frameH = this.element.videoHeight;

        const multi = Math.min(windowW / frameW, windowH / frameH);
        const vpWidth = frameW * multi;
        const vpHeight = (frameH * multi);

        this.mouse = {
            mouseMultiX: frameW / vpWidth,
            mouseMultiY: frameH / vpHeight,
            mouseOffsetX: Math.max((windowW - vpWidth) / 2.0, 0),
            mouseOffsetY: Math.max((windowH - vpHeight) / 2.0, 0),
            centerOffsetX: (document.documentElement.clientWidth - this.element.offsetWidth) / 2.0,
            centerOffsetY: (document.documentElement.clientHeight - this.element.offsetHeight) / 2.0,
            scrollX: window.scrollX,
            scrollY: window.scrollY,
            frameW,
            frameH,
        };
    }

    /**
     * Translates pointer position X based on current window math.
     * @param {Integer} clientX
     */
    _clientToServerX(clientX) {
        let serverX = Math.round((clientX - this.mouse.mouseOffsetX - this.mouse.centerOffsetX + this.mouse.scrollX) * this.mouse.mouseMultiX);

        if (serverX === this.mouse.frameW - 1) serverX = this.mouse.frameW;
        if (serverX > this.mouse.frameW) serverX = this.mouse.frameW;
        if (serverX < 0) serverX = 0;

        return serverX;
    }

    /**
     * Translates pointer position Y based on current window math.
     * @param {Integer} clientY
     */
    _clientToServerY(clientY) {
        let serverY = Math.round((clientY - this.mouse.mouseOffsetY - this.mouse.centerOffsetY + this.mouse.scrollY) * this.mouse.mouseMultiY);

        if (serverY === this.mouse.frameH - 1) serverY = this.mouse.frameH;
        if (serverY > this.mouse.frameH) serverY = this.mouse.frameH;
        if (serverY < 0) serverY = 0;

        return serverY;
    }


    /**
     * When fullscreen is entered, request keyboard and pointer lock.
     */
    _onFullscreenChange() 
     {
        if (document.fullscreenElement !== null) {
            // Enter fullscreen
            this.requestKeyboardLock();
            this.element.requestPointerLock();
        }

        // Reset stuck keys on server side.
        this.sendControl("kr");
    }

    /**
     * Called when window is being resized, used to detect when resize
     * ends so new resolution can be sent.
     */
    _resizeStart() {
        this._rtime = new Date();
        if (this._rtimeout === false) {
            this._rtimeout = true;
            setTimeout(() => { this._resizeEnd() }, this._rdelta);
        }
    }

    /**
     * Called in setTimeout loop to detect if window is done being resized.
     */
    _resizeEnd() {
        if (new Date() - this._rtime < this._rdelta) {
            setTimeout(() => { this._resizeEnd() }, this._rdelta);
        } else {
            this._rtimeout = false;
            if (this.onresizeend !== null) {
                this.onresizeend();
            }
        }
    }

    /**
     * Attaches input event handles to docuemnt, window and element.
     */
    attach() 
    {
        this.listeners.push(addListener(this.element, 'resize', this._windowMath, this));
        this.listeners.push(addListener(this.element, 'mousewheel', this._mouseWheel, this));
        this.listeners.push(addListener(this.element, 'contextmenu', this._contextMenu, this));
        this.listeners.push(addListener(this.element.parentElement, 'fullscreenchange', this._onFullscreenChange, this));
        this.listeners.push(addListener(this.element, 'mousemove', this._mouseButtonMovement, this));
        this.listeners.push(addListener(this.element, 'mousedown', this._mouseButtonMovement, this));
        this.listeners.push(addListener(this.element, 'mouseup', this._mouseButtonMovement, this));


        this.listeners.push(addListener(document, 'pointerlockchange', this._pointerLock, this));

        this.listeners.push(addListener(window, 'keydown', this._key, this));
        this.listeners.push(addListener(window, 'keyup', this._key, this));
        this.listeners.push(addListener(window, 'resize', this._windowMath, this));
        this.listeners.push(addListener(window, 'resize', this._resizeStart, this));


        // Adjust for scroll offset
        this.listeners.push(addListener(window, 'scroll', () => {
            this.mouse.scrollX = window.scrollX;
            this.mouse.scrollY = window.scrollY;
        }, this));

    }

    detach() {
        removeListeners(this.listeners);
        this._exitPointerLock();
    }

    /**
     * Request keyboard lock, must be in fullscreen mode to work.
     */
    requestKeyboardLock() {
        // event codes: https://www.w3.org/TR/uievents-code/#key-alphanumeric-writing-system
        const keys = [
            "AltLeft",
            "AltRight",
            "Tab",
            "Escape",
            "ContextMenu",
            "MetaLeft",
            "MetaRight"
        ];
        console.log("requesting keyboard lock");
        navigator.keyboard.lock(keys).then(
            () => {
                console.log("keyboard lock success");
            }
        ).catch(
            (e) => {
                console.log("keyboard lock failed: ", e);
            }
        )
    }

    getWindowResolution() {
        return [
            parseInt(this.element.offsetWidth * window.devicePixelRatio),
            parseInt(this.element.offsetHeight * window.devicePixelRatio)
        ];
    }

        
    _initWindowKeyCode()
    {
    //   windowdict[""] = parseInt(0x01);
    //   windowdict[""] = parseInt(0x02);
    //   windowdict[""] = parseInt(0x03);
    //   windowdict[""] = parseInt(0x04);
    //   windowdict[""] = parseInt(0x05);
    //   windowdict[""] = parseInt(0x06);
    //   windowdict[""] = parseInt(0x07);
    //   windowdict[""] = parseInt(0x08);
    this.windowdict["Tab"] = parseInt(0x09);
    //   windowdict[""] = parseInt(0x0A);
    //   windowdict[""] = parseInt(0x0B);
    //   windowdict[""] = parseInt(0x0C);
    this.windowdict["Enter"] = parseInt(0X0D);
    //   windowdict[""] = parseInt(0X0E);
    //   windowdict[""] = parseInt(0X0F);
    //   windowdict[""] = parseInt(0X10);
    //   windowdict[""] = parseInt(0X11);
    //   windowdict[""] = parseInt(0X12);
    this.windowdict["Pause"] = parseInt(0X13);
    this.windowdict["CapsLock"] = parseInt(0X14);
    //   windowdict[""] = parseInt(0X15);
    //   windowdict[""] = parseInt(0X16);
    //   windowdict[""] = parseInt(0X17);
    //   windowdict[""] = parseInt(0X18);
    //  windowdict[""] = parseInt(0x19);
    //   windowdict[""] = parseInt(0x1A);
    this.windowdict["Escape"] = parseInt(0x1B);
    //   windowdict[""] = parseInt(0x1C);
    //   windowdict[""] = parseInt(0x1D);
    //   windowdict[""] = parseInt(0x1E);
    //   windowdict[""] = parseInt(0x1F);
    this.windowdict["Space"] = parseInt(0x20);
    this.windowdict["PageUp"] = parseInt(0x21);
    this.windowdict["PageDown"] = parseInt(0x22);
    this.windowdict["End"] = parseInt(0x23);
    this.windowdict["Home"] = parseInt(0x24);
    this.windowdict["ArrowLeft"] = parseInt(0x25);
    this.windowdict["ArrowUp"] = parseInt(0x26);
    this.windowdict["ArrowRight"] = parseInt(0x27);
    this.windowdict["ArrowDown"] = parseInt(0x28);
    //   windowdict[""] = parseInt(0x29);
    //   windowdict[""] = parseInt(0x2A);
    //   windowdict[""] = parseInt(0x2B);
    //   windowdict[""] = parseInt(0x2C);
    this.windowdict["Insert"] = parseInt(0x2D);
    this.windowdict["Delete"] = parseInt(0x2E);
    //  windowdict[""] = parseInt(0x2F);
    this.windowdict["Digit0"] = parseInt(0x30);
    this.windowdict["Digit1"] = parseInt(0x31);
    this.windowdict["Digit2"] = parseInt(0x32);
    this.windowdict["Digit3"] = parseInt(0x33);
    this.windowdict["Digit4"] = parseInt(0x34);
    this.windowdict["Digit5"] = parseInt(0x35);
    this.windowdict["Digit6"] = parseInt(0x36);
    this.windowdict["Digit7"] = parseInt(0x37);
    this.windowdict["Digit8"] = parseInt(0x38);
    this.windowdict["Digit9"] = parseInt(0x39);
    this.windowdict["KeyA"] = parseInt(0x41);
    this.windowdict["KeyB"] = parseInt(0x42);
    this.windowdict["KeyC"] = parseInt(0x43);
    this.windowdict["KeyD"] = parseInt(0x44);
    this.windowdict["KeyE"] = parseInt(0x45);
    this.windowdict["KeyF"] = parseInt(0x46);
    this.windowdict["KeyG"] = parseInt(0x47);
    this.windowdict["KeyH"] = parseInt(0x48);
    this.windowdict["KeyI"] = parseInt(0x49);
    this.windowdict["KeyJ"] = parseInt(0x4A);
    this.windowdict["KeyK"] = parseInt(0x4B);
    this.windowdict["KeyL"] = parseInt(0x4C);
    this.windowdict["KeyM"] = parseInt(0x4D);
    this.windowdict["KeyN"] = parseInt(0x4E);
    this.windowdict["KeyO"] = parseInt(0x4F);
    this.windowdict["KeyP"] = parseInt(0x50);
    this.windowdict["KeyQ"] = parseInt(0x51);
    this.windowdict["KeyR"] = parseInt(0x52);
    this.windowdict["KeyS"] = parseInt(0x53);
    this.windowdict["KeyT"] = parseInt(0x54);
    this.windowdict["KeyU"] = parseInt(0x55);
    this.windowdict["KeyV"] = parseInt(0x56);
    this.windowdict["KeyW"] = parseInt(0x57);
    this.windowdict["KeyX"] = parseInt(0x58);
    this.windowdict["KeyY"] = parseInt(0x59);
    this.windowdict["KeyZ"] = parseInt(0x5A);
    this.windowdict["MetaLeft"] = parseInt(0x5B);
    //  windowdict[""] = parseInt(0x5C);
    //  ..............................
    //  windowdict[""] = parseInt(0x91);
    this.windowdict["ShiftLeft"] = parseInt(0xA0);
    this.windowdict["ShiftRight"] = parseInt(0xA1);
    this.windowdict["ControlLeft"] = parseInt(0xA2);
    this.windowdict["ControlRight"] = parseInt(0xA3);
    this.windowdict["ContextMenu"] = parseInt(0xA4);
    this.windowdict["Semicolon"] = parseInt(0xBA);
    this.windowdict["Equal"] = parseInt(0xBB);
    this.windowdict["Comma"] = parseInt(0xBC);
    this.windowdict["Minus"] = parseInt(0xBD);
    this.windowdict["Period"] = parseInt(0xBE);
    this.windowdict["Slash"] = parseInt(0xBF);
    this.windowdict["Backquote"] = parseInt(0xC0);
    this.windowdict["BracketLeft"] = parseInt(0xDB);
    this.windowdict["Backslash"] = parseInt(0xDC);
    this.windowdict["BracketRight"] = parseInt(0xDD);
    //  windowdict[""] = parseInt(0xDE);
    }

     
    convertJavaScriptKeyToWindowKey(javaKey)
    {
        return this.windowdict[javaKey];
    }
}

/**
 * Helper function to keep track of attached event listeners.
 * @param {Object} obj
 * @param {string} name
 * @param {function} func
 * @param {Object} ctx //context
 */
function addListener(obj, name, func, ctx) 
{
    const newFunc = ctx ? func.bind(ctx) : func;
    obj.addEventListener(name, newFunc);

    return [obj, name, newFunc];
}

/**
 * Helper function to remove all attached event listeners.
 * @param {Array} listeners
 */
function removeListeners(listeners) {
    for (const listener of listeners)
        listener[0].removeEventListener(listener[1], listener[2]);
}