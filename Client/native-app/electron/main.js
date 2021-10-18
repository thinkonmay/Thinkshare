const { app, BrowserWindow } = require('electron')
const child_process = require('child_process');
function createWindow () {
  const win = new BrowserWindow({
    width: 1000,
    height: 800
  })

  win.loadURL('https://service.thinkmay.net')
}


app.whenReady().then(() => {
  createWindow()
})


app.on('window-all-closed', function () {
  if (process.platform !== 'darwin') app.quit()
})



