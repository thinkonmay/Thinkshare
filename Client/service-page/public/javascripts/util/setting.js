
export function Mode(mode) {
	switch (mode) {
	case "ultra low":
		return {
			qoEMode: 1
		}
	case "low":
		return {
			qoEMode: 2
		}
	case "medium":
		return {
			qoEMode: 3
		}
	case "high":
		return {
			qoEMode: 4
		}	
	case "very high":
		return {
			qoEMode: 5
		}
	case "ultra high":
		return {
			qoEMode: 6
		}
	}
}

export function VideoCodec(codec) {
	switch (codec) {
	case "h264":
		return {
			videoCodec: 1
		}
	case "h265":
		return {
			videoCodec: 0
		}
	case "vp9":
		return {
			videoCodec: 3
		}
	}
}

export function AudioCodec(codec) {
	switch (codec) {
	case "opus":
		return {
			audioCodec: 4
		}
	case "aac":
		return {
			audioCodec: 5
		}
	}
}