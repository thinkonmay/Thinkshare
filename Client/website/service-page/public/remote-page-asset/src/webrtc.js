
// /**
//  * Sets connection state
//  * @param {String} state
//  */
// function 
// setConnectionState(state) {
//     if (app.onconnectionstatechange !== null) {
//         app.onconnectionstatechange(state);
//     }
// }

// ICE candidate received from peer, add it to the peer connection
function onIncomingICE(ice) {
    var candidate = new RTCIceCandidate(ice);
    app.Webrtc.addIceCandidate(candidate).catch(app.setError);
}









/**
 * Handles incoming SDP from signalling server.
 * Sets the remote description on the peer connection,
 * creates an answer with a local description and sends that to the peer.
 *
 * @param {RTCSessionDescription} sdp
 */
 function onIncomingSDP(sdp) {
    app.Webrtc.setRemoteDescription(sdp).then(() => {
        app.setStatus("Remote SDP set");
        if (sdp.type != "offer")
            return;
        app.setStatus("Got SDP offer");        
        app.Webrtc.createAnswer()
            .then(onLocalDescription).catch(app.setError);
        
    }).catch(app.setError);
}


/**
 * Handles local description creation from createAnswer.
 *
 * @param {RTCSessionDescription} local_sdp
 */
function onLocalDescription(desc) {
    app.Webrtc.setLocalDescription(desc).then(function() {
        app.setStatus("Sending SDP " + desc.type);
        sdp = {'sdp': app.Webrtc.localDescription}
    
    console.log("[Send SDP]: " + JSON.stringify(desc));
    SignallingSend("OFFER_SDP",JSON.stringify(sdp));
    });
}




/**
 * Handles incoming track event from peer connection.
 *
 * @param {Event} event - Track event: https://developer.mozilla.org/en-US/docs/Web/API/RTCTrackEvent
 */
 function onRemoteTrack(event) {
    if (app.VideoElement.srcObject !== event.streams[0]) {
        console.log('Incoming stream');
        app.VideoElement.srcObject = event.streams[0];
    }
}


    






/**
 * Sends message to peer data channel HID. 
 * @param {String} message
 */
function  
SendHID(message) 
{
    if (app.HidDC.readyState === 'open') 
    {
        app.HidDC.send(message);
    } else {
        app.setError("attempt to send data channel message before channel was open.");
    }
}

/**
 * Sends message to peer data channel control.
 */
function    
sendControlDC(opcode,to, message) 
{
    var message = 
    {
        "Opcode":opcode,
        "From":Module.CLIENT_MODULE,
        "To":to,
        "Data":message
    }
    app.ControlDC.send(JSON.stringify(message));
}



/**
 * Returns promise that resolves with connection stats.
 */
function    
getConnectionStats() 
{
    var pc = app.Webrtc;

    var connectionDetails = {};   // the final result object.

    if (window.chrome) {  // checking if chrome

        var reqFields = [
            'googLocalCandidateType',
            'googRemoteCandidateType',
            'packetsReceived',
            'packetsLost',
            'bytesReceived',
            'googFrameRateReceived',
            'googFrameRateOutput',
            'googCurrentDelayMs',
            'googFrameHeightReceived',
            'googFrameWidthReceived',
            'codecImplementationName',
            'googCodecName',
            'googAvailableReceiveBandwidth'
        ];

        return new Promise(function (resolve, reject) {
            pc.getStats(function (stats) {
                var filteredVideo = stats.result().filter(function (e) {
                    if ((e.id.indexOf('Conn-video') === 0 && e.stat('googActiveConnection') === 'true') ||
                        (e.id.indexOf('ssrc_') === 0 && e.stat('mediaType') === 'video') ||
                        (e.id == 'bweforvideo')) return true;
                });
                if (!filteredVideo) return reject('Something is wrong...');
                filteredVideo.forEach((f) => {
                    reqFields.forEach((e) => {
                        var statValue = f.stat(e);
                        if (statValue != "") {
                            connectionDetails['video' + e.replace('goog', '')] = statValue;
                        }
                    });
                });
                var filteredAudio = stats.result().filter(function (e) {
                    if ((e.id.indexOf('Conn-audio') === 0 && e.stat('googActiveConnection') === 'true') ||
                        (e.id.indexOf('ssrc_') === 0 && e.stat('mediaType') === 'audio') ||
                        (e.id == 'bweforaudio')) return true;
                });
                if (!filteredAudio) return reject('Something is wrong...');
                filteredAudio.forEach((f) => {
                    reqFields.forEach((e) => {
                        var statValue = f.stat(e);
                        if (statValue != "") {
                            connectionDetails['audio' + e.replace('goog', '')] = statValue;
                        }
                    });
                });
                resolve(connectionDetails);
            });
        });

    } else {
        app.setError("unable to fetch connection stats for brower, only Chrome is supported.");
    }
}




function
ondatachannel(event)
{
    if(event.channel.label === "HID"){
        app.HidDC = event.channel;
    }else if(event.channel.label === "Control"){
        onControlDataChannel(event);
    }
}


/**
 * Initiate connection to signalling server. 
 * invoke after request sdp signal has been replied
 */
function 
WebrtcConnect(msg) 
{
    console.log('Creating RTCPeerConnection');

    app.Webrtc = new RTCPeerConnection(app.RTPconfig);
    app.Webrtc.ondatachannel = ondatachannel;    

    app.Webrtc.ontrack = onRemoteTrack;

    if (msg != null && !msg.sdp) 
    {
        app.setError("Empty sdp");
    }

    //send ice dandidate to slave whenever icecandidate has been triggered on client
    app.Webrtc.onicecandidate = (event) => {
        // We have a candidate, send it to the remote party with the
        // same uuid
        if (event.candidate == null) {
                console.log("ICE Candidate was null, done");
                return;
        }
        app.setDebug("OFFER_ICE" + JSON.stringify({'ice': event.candidate}));
        SignallingSend("OFFER_ICE",JSON.stringify({'ice': event.candidate}));
    };

    if (msg != null)
        app.setStatus("Created peer connection for call, waiting for SDP");     
}
