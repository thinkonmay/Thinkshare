/**
 * * In this component, I will:
 * ? - Create newSwal and create module popup response
 */
// alert message from https://sweetalert2.github.io/#icons

export const newSwal = Swal.mixin({
	heightAuto: false,
	allowOutsideClick: false,
	allowEscapeKey: false
})

/**
 * 
 * @param {String} title 
 * @param {String} msg 
 * @param {String} icon 
 */
export function responseError(title, msg, icon) {
	newSwal.fire({
		title: title,
		text: msg,
		icon: icon,  // success, error, warning, info, question
	})
}

/**
 * 
 * @param {Response} response 
 */
export function responseErrorHandler(response) {
	const keys = Object.keys(response.errors)
	const errors = keys.map(key => response.errors[key])
	const msg = keys.map((key, index) => `${key}: ${errors[index]}`).join(", ")
	newSwal.fire({
		title: "Error!",
		text: msg,
		icon: "error"
	})
}

/**
 * 
 * @param {String} error 
 */
export function fetchErrorHandler(error) {
	newSwal.fire({
		title: "Error!",
		text: error.message,
		icon: "error"
	})
}

/**
 * 
 * @param {String} id 
 * @param {String} html 
 */
export function append(id, html) { $(`#${id}`).append(html) }