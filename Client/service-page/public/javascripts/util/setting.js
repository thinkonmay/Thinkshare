import {setCookie,getCookie} from "./cookie.js"




export function Mode(mode) {
	
	var cap = JSON.parse(getCookie("cap"));
	
	switch (mode) {
	case "ultra low":
			cap.mode= 1
	case "low":
			cap.mode= 2
	case "medium":
			cap.mode= 3
	case "high":
			cap.mode= 4
	case "very high":
			cap.mode= 5
	case "ultra high":
			cap.mode= 6
	}

	setCookie("cap", JSON.stringify(cap), 999999)
	console.log("set default device capability to " + getCookie("cap"));

}

export function VideoCodec(codec) {
	
	var cap = JSON.parse(getCookie("cap"));
	
		
	switch (codec) {
	case "h264":
			cap.videoCodec= 1
	case "h265":
			cap.videoCodec= 0
	case "vp9":
			cap.videoCodec= 3
	}
	setCookie("cap", JSON.stringify(cap), 999999)
	console.log("set default device capability to " + getCookie("cap"));
}

export function AudioCodec(codec) {
	
	var cap = JSON.parse(getCookie("cap"));

	
	
	switch (codec) {
	case "opus":
			cap.audioCodec= 4
	case "aac":
			cap.audioCodec= 5
	}
	setCookie("cap", JSON.stringify(cap), 999999)
	console.log("set default device capability to " + getCookie("cap"));
}



export function
map_video_resolution(resolution)
{
	var cap = JSON.parse(getCookie("cap"));
	switch(resolution)
	{
		case "4K":
			cap.screenHeight=21600;
			cap.screenWidth=3840;
		case "2K":
			cap.screenHeight=2560;
			cap.screenWidth=1440;
		case "FullHD":
			cap.screenHeight=1920;
			cap.screenWidth=1080;
	}
	setCookie("cap", JSON.stringify(cap), 999999)
	console.log("set default device capability to " + getCookie("cap"));
}