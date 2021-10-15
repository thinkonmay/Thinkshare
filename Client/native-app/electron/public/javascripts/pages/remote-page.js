import { getCookie } from "../util/cookie.js";

var cookie = await getCookie("sessionClient");
var sessionClient = JSON.parse(cookie);
app.SetupSession(sessionClient);
app.connectServer();








