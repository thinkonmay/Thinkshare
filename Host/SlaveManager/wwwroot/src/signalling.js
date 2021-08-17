
class Signalling
{
    constructor(server,             // URL of signalling server
                SessionClientID,    // SessionClientID to register with server 
                clientoffer)        // TRUE if client offer SDP to server
    {
        this._server = server;

        this.SessionClientID = SessionClientID;

        this.ClientOfferer = clientoffer;

        this._ws_conn = null;

        this.WebRTCconnect = null;

        this.RTCpeerConnection = null;

        this.onstatus = null;

        this.onerror = null;

        this.ondebug = null;

        this.onice = null;

        this.onsdp = null;

        this.state = 'disconnected';
    }

    /**
     * Sets status message.
     * @param {String} message
     */
    _setStatus(message) {
        if (this.onstatus !== null) {
            this.onstatus(message);
        }
    }

    /**
     * Sets a debug message.
     * @param {String} message
     */
    _setDebug(message) {
        if (this.ondebug !== null) {
            this.ondebug(message);
        }
    }

    /**
     * Sets error message.
     * @param {String} message
     */
    _setError(message) {
        if (this.onerror !== null) {
            this.onerror(message);
        }
    }

    /**
     * Sets SDP
     */
    _setSDP(sdp) {
        if (this.onsdp !== null) {
            this.onsdp(sdp);
        }
        else
        {
            this._setDebug("SDP signal handler not settle up")
        }
    }

    /**
     * Sets ICE
     * @param {RTCIceCandidate} icecandidate
     */
    _setICE(icecandidate) {
        if (this.onice !== null){
            this.onice(icecandidate);
        }
        else {
            this._setDebug("ICE handler not settled up")
        }
    }

    /**
     * Fired whenever the signalling websocket is opened.
     * Sends the peer id to the signalling server.
     */
    _onServerOpen(){
        this.state = 'connected';
        this.SignallingSend("CLIENTREQUEST",null)            
        this._setStatus("Registering with server, CLientID: " + this.SessionClientID);
    }

    SignallingSend(request_type, content)
    {
        var json_message = {"RequestType":request_type,
                            "SubjectId": this.SessionClientID,
                            "Content":content,
                            "Result":"SESSION_ACCEPTED"}

        this._ws_conn.send(JSON.stringify(json_message));
    }

    /**
     * Fired whenever the signalling websocket emits and error.
     * Reconnects after 3 seconds.
     */
    _onServerError() {
        this._setDebug("Connection error, retry in 3 seconds.");
        if (this._ws_conn.readyState === this._ws_conn.CLOSED) {
            setTimeout(() => {
                this.connect();
            }, 3000);
        }
    }

    /**
     * Fired whenever a message is received from the signalling server.
     * Message types:
     *   HELLO: response from server indicating peer is registered.
     *   ERROR*: error messages from server.
     *   {"sdp": ...}: JSON SDP message
     *   {"ice": ...}: JSON ICE message
     *
     * @private
     * @event
     * @param {Event} event The event: https://developer.mozilla.org/en-US/docs/Web/API/MessageEvent
     */
    _onServerMessage(event) {
        try {
        var message_json = JSON.parse(event.data);
        } catch (e) {
            if (e instanceof SyntaxError) {
                this.ondebug("Error parsing incoming JSON: " + event.data);
            } else {
                this.ondebug("Unknown error parsing response: " + event.data);
            }
            return;
        }


        if (message_json.Result == "SESSION_REJECTED" || message_json.Result == "SESSION_TIMEOUT")
        {
            this.ws_conn.close();
            this._setStatus("Session Denied");
        }


        if(message_json.RequestType === "CLIENTREQUEST")
        {
            this._setStatus("Registered with server.");
            this._setDebug("[signalling] " + message_json.Result)

            if(this.ClientOfferer) {
                //TODO : in case client want to offer fist
            }
            else {
                this._setStatus("Requesting for video stream.");
                var sdp = { "type":"request" }
                this.sendSDP(sdp);
            }
        }

        if(this.RTCpeerConnection == null)
        {
            this.WebRTCconnect();
        }

        if(message_json.RequestType === "OFFER_SDP")
        {
            this._setDebug("[SDP RECEIVED]"+JSON.stringify(message_json.Content));
            this._setSDP(JSON.parse(message_json.Content).sdp);
        }
        else if(message_json.RequestType === "OFFER_ICE")
        {
            this._setDebug("[ICE RECEIVED]"+message_json.Content);
            this._setICE(JSON.parse(message_json.Content).ice);
        }   
        else
        {
            print("unknown message", event.data);
        } 
        
    }

    /**
     * Fired whenever the signalling websocket is closed.
     * Reconnects after 1 second.
     */
    _onServerClose() {
        this.state = 'disconnected';
        this._setError("Server disconnected");

    }

    /**
     * Initiates the connection to the signalling server.
     */
    connect() {
        this.state = 'connecting';
        this._setStatus("Connecting to server.");

        this._ws_conn = new WebSocket(this._server);

        // Bind event handlers.
        this._ws_conn.addEventListener('open', this._onServerOpen.bind(this));
        this._ws_conn.addEventListener('error', this._onServerError.bind(this));
        this._ws_conn.addEventListener('message', this._onServerMessage.bind(this));
        this._ws_conn.addEventListener('close', this._onServerClose.bind(this));
    }

    /**
     * Closes connection to signalling server.
     * Triggers onServerClose event.
     */
    disconnect() {
        this._ws_conn.close();
    }

    /**
     * Send ICE candidate.
     * @param {RTCIceCandidate} ice
     */
    sendICE(ice){
        var data = 
        {
            "ice":ice
        }
        this._setDebug("ICEOUT"+JSON.stringify(data))
        this.SignallingSend("OFFER_ICE",JSON.stringify(data));
    }

    /**
     * Send local session description.
     * @param {RTCSessionDescription} sdp
     */
    sendSDP(sdp){
        var data = 
        {
            "sdp":sdp
        }
        this._setDebug("SDPOUT"+JSON.stringify(data))
        this.SignallingSend("OFFER_SDP",JSON.stringify(data));
    }
}