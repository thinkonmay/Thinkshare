/**
 * * In this function, I will 
 * ? - Receive state Login with Google in Electron
 */
import { GoogleLogin } from "./login-register.js";
document.getElementById('googleLogin').addEventListener('click', function() {
    GoogleLogin(true)
})