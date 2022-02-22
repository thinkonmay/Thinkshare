/**
 * * In this component, I will:
 * ? - Check type of device user
 */

//* Check windows application
export function isElectron() {
	// Renderer process
	if (typeof window !== 'undefined' && typeof window.process === 'object' && window.process.type === 'renderer') {
		return true;
	}

	// Main process
	if (typeof process !== 'undefined' && typeof process.versions === 'object' && !!process.versions.electron) {
		return true;
	}

	// Detect the user agent when the `nodeIntegration` option is set to true
	if (typeof navigator === 'object' && typeof navigator.userAgent === 'string' && navigator.userAgent.indexOf('Electron') >= 0) {
		return true;
	}

	return false;
}

//* Check MacOS
export function isMacintosh(){
    return navigator.platform.indexOf('Mac') > -1;
}

export function isWindows(){
    return navigator.platform.indexOf('Win') > -1;
}