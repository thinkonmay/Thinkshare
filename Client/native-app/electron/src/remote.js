const remoteBtn = document.getElementById('remoteBtn');

remoteBtn.onclick = e => {
    run_script("c: && cd ThinkmayClient/ThinkMay/bin && start remote-app.exe", null, null);
    run_script("c: && cd ThinkMayServer/ThinkMay/bin && start session-core.exe", null, null);

}

// Uses node.js process manager
const electron = require('electron');
const child_process = require('child_process');
const dialog = electron.dialog;

// This function will output the lines from the script 
// and will return the full combined output
// as well as exit code when it's done (using the callback).
function run_script(command, args, callback) {
    var child = child_process.spawn(command, args, {
        encoding: 'utf8',
        shell: true
    });
    // You can also use a variable to save the output for when the script closes later
    child.on('error', (error) => {
        dialog.showMessageBox({
            title: 'Title',
            type: 'warning',
            message: 'Error occured.\r\n' + error
        });
    });

    child.stdout.setEncoding('utf8');
    child.stdout.on('data', (data) => {
        //Here is the output
        data=data.toString();   
        console.log(data);      
    });

    child.stderr.setEncoding('utf8');
    child.stderr.on('data', (data) => {
        // Return some data to the renderer process with the mainprocess-response ID
        mainWindow.webContents.send('mainprocess-response', data);
        //Here is the output from the command
        console.log(data);  
    });

    child.on('close', (code) => {
        //Here you can get the exit code of the script  
        switch (code) {
            case 0:
                // dialog.showMessageBox({
                //     title: 'Title',
                //     type: 'info',
                //     message: 'End process.\r\n'
                // });
                break;
        }

    });
    if (typeof callback === 'function')
        callback();
}