import { getCookie } from "../util/cookie.js";

var token = await getCookie("RemoteToken");
app.SetupSession(sessionClient);
app.connectServer();