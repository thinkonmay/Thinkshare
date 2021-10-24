import {setCookie,getCookie} from "./cookie.js"



export function Platform(platform) {
	setCookie("platform", platform, 999999)
}

export function Mode(mode) {
	
	var cap = JSON.parse(getCookie("cap"));
	
	switch (mode) {
	case "ultra low":
			cap.mode= 1;
			break;
	case "low":
			cap.mode= 2;
			break;
	case "medium":
			cap.mode= 3;
			break;
	case "high":
			cap.mode= 4;
			break;
	case "very high":
			cap.mode= 5;
			break;
	case "ultra high":
			cap.mode= 6;
			break;
	}
	setCookie("cap", JSON.stringify(cap), 999999)
	console.log("set default device capability to " + getCookie("cap"));
}

export function VideoCodec(codec) {
	
	var cap = JSON.parse(getCookie("cap"));
	
		
	switch (codec) {
	case "h264":
			cap.videoCodec= 1;
			break;
	case "h265":
			cap.videoCodec= 0;
			break;
	case "vp9":
			cap.videoCodec= 3;
			break;
	}
	setCookie("cap", JSON.stringify(cap), 999999)
	console.log("set default device capability to " + getCookie("cap"));
}

export function AudioCodec(codec) {
	
	var cap = JSON.parse(getCookie("cap"));

	
	
	switch (codec) {
	case "opus":
			cap.audioCodec= 4;
			break;
	case "aac":
			cap.audioCodec= 5;
			break;
	}
	setCookie("cap", JSON.stringify(cap), 999999)
	console.log("set default device capability to " + getCookie("cap"));
}



export function
mapVideoRes(resolution)
{
	var cap = JSON.parse(getCookie("cap"));
	switch(resolution)
	{
		case "4K":
			cap.screenHeight=2160;
			cap.screenWidth=3840;
			break;
		case "2K":
			cap.screenHeight=1440;
			cap.screenWidth=2560;
			break;
		case "FullHD":
			cap.screenHeight=1080;
			cap.screenWidth=1920;
			break;
	}
	setCookie("cap", JSON.stringify(cap), 999999)
	console.log("set default device capability to " + getCookie("cap"));
}