// // alert message from https://sweetalert2.github.io/#icons
// class Utils{

// }
// const newSwal = Swal.mixin({
// 	heightAuto: false,
// 	allowOutsideClick: false,
// 	allowEscapeKey: false
// })

// Utils.prototype.responseError(title, msg, icon) = function(title, msg, icon)	 {
// 	newSwal.fire({
// 		title: title,
// 		text: msg,
// 		icon: icon,  // success, error, warning, info, question
// 	})
// }

// Utils.prototype.responseErrorHandler = function(response) {
// 	const keys = Object.keys(response.errors)
// 	const errors = keys.map(key => response.errors[key])
// 	const msg = keys.map((key, index) => `${key}: ${errors[index]}`).join(", ")
// 	newSwal.fire({
// 		title: "Lỗi!",
// 		text: msg,
// 		icon: "error"
// 	})
// }

// Utils.prototype.fetchErrorHandler = function(error) {
// 	newSwal.fire({
// 		title: "Lỗi!",
// 		text: error.message,
// 		icon: "error"
// 	})
// }

// module.exports = Utils