/**
 * Handles messages from the peer data channel.
 * @param {MessageEvent} event
 */
 function onHidDC(event) 
 {
    // Attempt to parse message as JSON
    var msg;
    try {
        msg = JSON.parse(event.data);
    } catch (e) {
        if (e instanceof SyntaxError) {
            app.setError("error parsing data channel message as JSON: " + event.data);
        } else {
            app.setError("failed to parse data channel message: " + event.data);
        }
        return;
    }
 

    app.setStatus("HID data channel message: " + event.data);
 }