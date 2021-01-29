
// get Client ID from URL
var url = new URL(window.location.href);
var query_string = url.search;
var search_params = new URLSearchParams(query_string);
var userId = search_params.get('userId');
var token = search_params.get('token');
$("#UserId").val(userId);
$("#Token").val(token);

// Reset Password Form
$("#reset_password_form").submit(function (e) {
    e.preventDefault(); // Stop form from submitting normally

    // to delete all Success or Error messages from Ajax
    $("#message").html("");

    $.ajax({
        url: "/api/Account/SetResetPasswordConfirmation",
        type: "POST",
        data: $('#reset_password_form').serialize(),
        success: function (data) {
            document.querySelector('#message').insertAdjacentHTML('afterbegin', data);
        },
        error: function (error) {
            // Error Object
            var errors = error.responseJSON.errors;
            for (const [key, value] of Object.entries(errors)) {
                // insert all Errors in Message div
                document.querySelector('#message').insertAdjacentHTML('afterbegin', '<div>' + value + '</div>');
                //console.log(`${key}: ${value}`);
            }
        }
    });
});

