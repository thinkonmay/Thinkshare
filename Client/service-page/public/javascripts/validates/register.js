export default {
	rules: {
		fullName: {
			required: true,
			minlength: 7,
			maxlength: 50
		},
		userName: {
			required: true,
			minlength: 7,
			maxlength: 50
		},
		phoneNumber: {
			required: true,
			minlength: 7,
			maxlength: 50
		},
		jobs: {
			required: true,
			minlength: 7,
			maxlength: 50
		},

		email: {
			required: true,
			email: true,
			minlength: 10
		},
		password: {
			required: true,
			minlength: 5,
			maxlength: 50
		},
		repassword: {
			required: true,
			minlength: 5,
			maxlength: 50,
			equalTo: "#password"
		},
		agree: "required"
	},
	messages: {
		fullName: {
			required: "Your full name is require field",
			minlength: "Minimum character is 7",
			maxlength: "Maximum character is 50"
		},
		userName: {
			required: "Your username is require field",
			minlength: "Minimum character is 7",
			maxlength: "Maximum character is 50"
		},
		phoneNumber: {
			required: "Your phone number is require field",
			minlength: "Minimum character is 7",
			maxlength: "Maximum character is 15"
		},
		jobs: {
			required: "Your jobs is require field",
			minlength: "Minimum character is 7",
			maxlength: "Maximum character is 50"
		},
		email: {
			required: "Your email address is require field",
			email: "Invalid email",
			minlength: "Minimum character is 10"
		},
		password: {
			required: "Your password is required field",
			minlength: "Minimum character is 5",
			maxlength: "Maximum character is 50"
		},
		repassword: {
			required: "You need to retype your password field!",
			minlength: "Minimum character is 5",
			maxlength: "Maximum character is 50",
			equalTo: "Your retype-password do not match with the original"
		},
		agree: "You need to accept our tems"
	}
}
