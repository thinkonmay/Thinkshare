const { app, BrowserWindow } = require('electron');
const path = require('path');
const electron = require('electron');
const child_process = require('child_process');
const dialog = electron.dialog;
// Handle creating/removing shortcuts on Windows when installing/uninstalling.
if (require('electron-squirrel-startup')) { // eslint-disable-line global-require
  app.quit();
}

// shell.openPath("C:/ThinkmayClient/ThinkMay/bin/remote-app.exe")

const createWindow = () => {
  // Create the browser window.
  const mainWindow = new BrowserWindow({
    width: 800,
    height: 600,
    webPreferences: {
      nodeIntegration: true,
      contextIsolation: false,
      enableRemoteModule: true,
    },
  });

  // and load the index.html of the app.
  //C:\Users\thien\AppData\Local\native_app\app-1.0.0\resources\app
  // run_script("c: && cd User/%username%/AppData/Local/native_app/app-1.0.0/resources/app/ && node src/www")
  run_script("node src/www")
  mainWindow.loadURL("http://localhost/")
  run_script("c: && cd ThinkmayClient/ThinkMay/bin && start remote-app.exe", null, null);
  run_script("c: && cd ThinkMayServer/ThinkMay/bin && start session-core.exe", null, null);
};

// This method will be called when Electron has finished
// initialization and is ready to create browser windows.
// Some APIs can only be used after this event occurs.
app.on('ready', createWindow);

// Quit when all windows are closed, except on macOS. There, it's common
// for applications and their menu bar to stay active until the user quits
// explicitly with Cmd + Q.
app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') {
    app.quit();
  }
});

app.on('activate', () => {
  // On OS X it's common to re-create a window in the app when the
  // dock icon is clicked and there are no other windows open.
  if (BrowserWindow.getAllWindows().length === 0) {
    createWindow();
  }
});

// In this file you can include the rest of your app's specific main process
// code. You can also put them in separate files and import them here.

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
        // mainWindow.webContents.send('mainprocess-response', data);
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