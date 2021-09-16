import { getCookie } from "../util/cookie.js";


var sessionClient = JSON.parse(getCookie("sessionClient"))
app.SetupSession(sessionClient);
app.connectServer();








