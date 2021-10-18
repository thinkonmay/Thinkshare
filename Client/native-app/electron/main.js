const { app, BrowserWindow } = require('electron')
const child_process = require('child_process');
function createWindow () {
  const win = new BrowserWindow({
    width: 800,
    height: 600
  })

}


app.whenReady().then(() => {
  createWindow()
})


app.on('window-all-closed', function () {
  if (process.platform !== 'darwin') app.quit()
})


app.whenReady().then(() => {
  createWindow()

  app.on('activate', function () {
    if (BrowserWindow.getAllWindows().length === 0) createWindow()
  })
})
