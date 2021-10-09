
export function Mode(mode) {
	switch (mode) {
	case "ultra low":
		return {
			mode: 1
		}
	case "low":
		return {
			mode: 2
		}
	case "medium":
		return {
			mode: 3
		}
	case "high":
		return {
			mode: 4
		}	
	case "very high":
		return {
			mode: 5
		}
	case "ultra high":
		return {
			mode: 6
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