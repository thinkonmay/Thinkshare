const { app, BrowserWindow } = require('electron')
const child_process = require('child_process');
function createWindow() {
  const mainWindow = new BrowserWindow({
    show: false,
    icon: 'assets/logo.ico',
  });
  mainWindow.maximize();
  mainWindow.show();
  mainWindow.loadURL("https://service.thinkmay.net/")
}

app.whenReady().then(() => {
  createWindow()
})


app.on('window-all-closed', function () {
  if (process.platform !== 'darwin') app.quit()
})



