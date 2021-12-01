export default {
	rules: {
		username: {
			required: true,
			minlength: 5,
			maxlength: 50
		},
		password: {
			required: true,
			minlength: 5,
			maxlength: 50
		}
	},
	messages: {
		username: {
			required: "Your username is required!",
			minlength: "Minimum character is 5",
			maxlength: "Maximum character is 50"
		},
		password: {
			required: "Your password is required!",
			minlength: "Minimum character is 5",
			maxlength: "Maximum character is 50"
		}
	}
}
