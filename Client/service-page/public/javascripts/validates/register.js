$(document).ready(function(){

	jQuery.validator.addMethod("noSpace", function(value, element) { 
	return value.indexOf(" ") < 0 && value != ""; 
  }, "No space please and don't leave it empty");
})
  
export default {
	rules: {
		fullname: {
			required: true,
			minlength: 5,
			maxlength: 50
		},
		dob: {
			required: true
		},
		username: {
			required: true,
			minlength: 3,
			maxlength: 50,
			noSpace: true,
		},
		phonenumber: {
			required: true,
			minlength: 10,
			maxlength: 15
		},
		jobs: {
			required: true,
			minlength: 5,
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
		dob: {
			required: "Your bỉthday is require field"
		},
		userName: {
			required: "Your username is require field",
			minlength: "Minimum character is 7",
			maxlength: "Maximum character is 50",
			noSpace: true,
		},
		phoneNumber: {
			required: "Your phone number is require field",
			minlength: "Minimum character is 7",
			maxlength: "Maximum character is 15",
			isNaN: "Your numberphone just only number"
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
