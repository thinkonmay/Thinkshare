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
    constructor(element, sendHid, sendControl) {

        this.element = element;

        this.sendHid = sendHid;
        this.sendControl = sendControl;

        this.mouse = null;
        this.x = 0;
        this.y = 0;

        this.onmenuhotkey = null;
        this.onfullscreenhotkey = null;

        this.listeners = [];
        this.onresizeend = null;

        // internal variables used by resize start/end functions.
        this._rtime = null;
        this._rtimeout = false;
        this._rdelta = 200;
    }

    /**
     * Handle mouse up event and send to slave device
     * @param {Mouse up event} event 
     */
    _mouseButtonUp(event) {
        this.x = this._clientToServerX(event.clientX);
        this.y = this._clientToServerY(event.clientY);              

        var INPUT =
        {
            "Opcode":HidOpcode.MOUSE,
            "button":event.button,
            "dX":this.x,
            "dY":this.y,
        }

        this. sendHid(JSON.stringify(INPUT));
    }

    /**
     * Handle mouse down event and send to slave device
     * @param {Mouse event} event 
     */
    _mouseButtonDown(event) {
        this.x = this._clientToServerX(event.clientX);
        this.y = this._clientToServerY(event.clientY);

        var INPUT =
        {
            "Opcode":HidOpcode.MOUSEDOWN,
            "button":event.button,
            "dX":this.x,
            "dY":this.y
        }

        this.sendHid(JSON.stringify(INPUT));
    }

    /**
     * Handle mouse movement and send to slave
     * @param {Mouse movement} event 
     */
    _mouseButtonMovement(event) {
        this.x = this._clientToServerX(event.clientX);
        this.y = this._clientToServerY(event.clientY);
    

        var INPUT =
        {
            "Opcode":HidOpcode.MOUSEMOVE,
            "dX":this.x,
            "dY":this.y,
        }
        this.sendHid(JSON.stringify(INPUT));
    }

    /**
     * handle mouse wheel and send to slave
     * @param {Mouse wheel event} event 
     */
    _mouseWheel(event){             

        var INPUT =
        {
            "Opcode":HidOpcode.MOUSEWHEEL,
            "dX":event.deltaX,
            "dY":event.deltaY
        }

        this.sendHid(JSON.stringify(INPUT));
    }


    _contextMenu(event) { //disble context menu on remote control
        event.preventDefault();
    }

    _keyup(event) {  // disable problematic browser shortcuts
        if (event.code === 'F5' && event.ctrlKey ||
            event.code === 'KeyI' && event.ctrlKey && event.shiftKey ||
            event.code === 'F11') {
            event.preventDefault();
            return;
        }

        var Keyboard =
        {
            "Opcode":HidOpcode.KEYUP,
            "wVk":convertJavaScriptKeyToWindowKey(event.code),
        }

        this.sendHid(JSON.stringify(Keyboard));

    }


    _keydown(event) {

        // disable problematic browser shortcuts
        if (event.code === 'F5' && event.ctrlKey ||
            event.code === 'KeyI' && event.ctrlKey && event.shiftKey ||
            event.code === 'F11') {
            event.preventDefault();
            return;
        }

        // capture menu hotkey
        if (event.code === 'KeyM' && event.ctrlKey && event.shiftKey) {
            if (document.fullscreenElement === null && this.onmenuhotkey !== null) {
                this.onmenuhotkey();
                event.preventDefault();
            }
            return;
        }

        // capture fullscreen hotkey
        if (event.code === 'KeyF' && event.ctrlKey && event.shiftKey) {
            if (document.fullscreenElement === null && this.onfullscreenhotkey !== null) {
                this.onfullscreenhotkey();
                event.preventDefault();
            }
            return;
        }
        
        var Keyboard =
        {
            "Opcode":HidOpcode.KEYDOWN,
            "wVk":convertJavaScriptKeyToWindowKey(event.code),
        }

        this.sendHid(JSON.stringify(Keyboard));
    }

    // /**
    //  * Sends WebRTC app command to toggle display of the remote mouse pointer.
    //  */
    _pointerLock() 
    {
        var Toggle = 
        {
            "Opcode":HidOpcode.DISPLAY_POINTER,
            "IsLock":document.pointerLockElement
        } 
        this.sendHid(JSON.stringify(Toggle));
    }

    /**
     * 
     */
    _exitPointerLock() {
        document.exitPointerLock();
        // hide the pointer.
        // this._pointerLock();
    }

    /**
     * Captures display and video dimensions required for computing mouse pointer position.
     * This should be fired whenever the window size changes.
     */
    _windowMath() {
        /**
         * size of offset window (not the actual video size)
         */
        const windowW = this.element.offsetWidth;
        const windowH = this.element.offsetHeight;

        /**
         * actual video width and height
         */
        const frameW = this.element.videoWidth;
        const frameH = this.element.videoHeight;


        const multi = Math.min(windowW / frameW, windowH / frameH);


        const vpWidth = frameW * multi;
        const vpHeight = (frameH * multi);

        this.mouse = {

            /**
             * relation between frame size and actual window size
             */
            mouseMultiX: frameW / vpWidth,
            mouseMultiY: frameH / vpHeight,

            /**
             * 
             */
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
        let serverX = Math.round(
            (clientX - this.mouse.mouseOffsetX - this.mouse.centerOffsetX + this.mouse.scrollX) 
            * this.mouse.mouseMultiX);

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
        let serverY = Math.round(
            (clientY - this.mouse.mouseOffsetY - this.mouse.centerOffsetY + this.mouse.scrollY)
             * this.mouse.mouseMultiY);

        if (serverY === this.mouse.frameH - 1) serverY = this.mouse.frameH;
        if (serverY > this.mouse.frameH) serverY = this.mouse.frameH;
        if (serverY < 0) serverY = 0;

        return this.mouse.frameH - serverY;
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
    }

    /**
     * Called when window is being resized, used to detect when resize ends so new resolution can be sent.
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
        /**
         * full screen event
         */
        this.listeners.push(addListener(this.element.parentElement, 'fullscreenchange', this._onFullscreenChange, this));

        /**
         * video event
         */
        this.listeners.push(addListener(this.element, 'resize', this._windowMath, this));
        this.listeners.push(addListener(this.element, 'wheel', this._mouseWheel, this));
        this.listeners.push(addListener(this.element, 'contextmenu', this._contextMenu, this)); ///disable content menu key on remote control

        /**
         * mouse event
         */
        this.listeners.push(addListener(this.element, 'mousemove', this._mouseButtonMovement, this));
        this.listeners.push(addListener(this.element, 'mousedown', this._mouseButtonDown, this));
        this.listeners.push(addListener(this.element, 'mouseup', this._mouseButtonUp, this));


        /**
         * mouse lock event
         */
        this.listeners.push(addListener(document, 'pointerlockchange', this._pointerLock, this));
        
        /**
         * keyboard event
         */
        this.listeners.push(addListener(window, 'keydown', this._keydown, this));
        this.listeners.push(addListener(window, 'keyup', this._keyup, this));

        /**
         * window resize event
         */
        this.listeners.push(addListener(window, 'resize', this._windowMath, this));
        this.listeners.push(addListener(window, 'resize', this._resizeStart, this));


        /**
         * scroll event
         */
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
        /**
         * control key on window
         */
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


    /**
     * get window resolution
     * @returns 2 element list control screen width and height
     */
    getWindowResolution() {
        return [
            parseInt(this.element.offsetWidth * window.devicePixelRatio),
            parseInt(this.element.offsetHeight * window.devicePixelRatio)
        ];
    }


}


/**
 * Helper function to keep track of attached event listeners.
 * @param {Object} obj
 * @param {string} name
 * @param {function} func
 * @param {Object} ctx
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