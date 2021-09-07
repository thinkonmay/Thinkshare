export default {
    rules: {
        email: {
            required: true,
            email: true,
            minlength: 10
        },
        password: {
            required: true,
            minlength: 5,
            maxlength: 50
        }
    },
    messages: {
        email: {
            required: "Your email address is required",
            email: "Invalid email",
            minlength: "Minimum character is 10"
        },
        password: {
            required: "Your password is required!",
            minlength: "Minimum character is 5",
            maxlength: "Maximum character is 50"
        }
    }
}