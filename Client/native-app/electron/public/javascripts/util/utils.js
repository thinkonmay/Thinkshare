// alert message from https://sweetalert2.github.io/#icons

const newSwal = Swal.mixin({
	heightAuto: false,
	allowOutsideClick: false,
	allowEscapeKey: false
})

function responseError(title, msg, icon) {
	newSwal.fire({
		title: title,
		text: msg,
		icon: icon,  // success, error, warning, info, question
	})
}

function responseErrorHandler(response) {
	const keys = Object.keys(response.errors)
	const errors = keys.map(key => response.errors[key])
	const msg = keys.map((key, index) => `${key}: ${errors[index]}`).join(", ")
	newSwal.fire({
		title: "Lỗi!",
		text: msg,
		icon: "error"
	})
}

function fetchErrorHandler(error) {
	newSwal.fire({
		title: "Lỗi!",
		text: error.message,
		icon: "error"
	})
}