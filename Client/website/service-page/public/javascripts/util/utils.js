// alert message from https://sweetalert2.github.io/#icons

export const newSwal = Swal.mixin({
	heightAuto: false,
	allowOutsideClick: false,
	allowEscapeKey: false
})

export function responseError(title, msg, icon) {
	newSwal.fire({
		title: title,
		text: msg,
		icon: icon,  // success, error, warning, info, question
	})
}

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

export function fetchErrorHandler(error) {
	newSwal.fire({
		title: "Error!",
		text: error.message,
		icon: "error"
	})
}

export function append(id, html) { $(`#${id}`).append(html) }