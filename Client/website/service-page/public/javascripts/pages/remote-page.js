import { getCookie } from "../util/cookie.js";
import { sessionSetting } from "../util/api.js";




var token = await getCookie("remoteToken");
const sessionInfor = await (await sessionSetting(token)).json()
app.SetupSession(sessionInfor);
app.connectServer();