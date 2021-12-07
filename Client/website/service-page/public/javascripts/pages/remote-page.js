import { getCookie } from "../util/cookie.js";
import { sessionSetting } from "../util/api.js";




var token = await getCookie("remoteToken");
const res = await sessionSetting(token);
const sessionInfor = await (res).json();
app.remoteToken = token;
app.SetupSession(sessionInfor);
WebrtcConnect();
app.connectServer();