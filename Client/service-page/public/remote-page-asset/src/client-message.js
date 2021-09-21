function
onClientMessage(event)
{
    //TODO : more features using message handler
    var msg;
    try {
        msg = JSON.parse(event.data);
    } catch (e) {
        if (e instanceof SyntaxError) {
            this._setError("error parsing data channel message as JSON: " + event.data);
        } else {
            this._setError("failed to parse data channel message: " + event.data);
        }
        return;
    }

    var from = msg.From;
    var To = msg.To;
    var Opcode = msg.Opcode;
    var Data = msg.Data;

    if(from === Module.AGENT_MODULE)
    {
        if(Opcode == Opcode.CLIPBOARD)
        {
            if (Data != null) {
                app.onClipboard(Data);
            }
        }else if(Opcode == Opcode.FILE_TRANSFER)
        {
            if (Data != null)
            {
                app.onFileTransfer(Data);
            }
        }
    }


}