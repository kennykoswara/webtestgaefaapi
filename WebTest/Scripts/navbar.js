AddAntiForgeryToken = function (data) {
    data.__RequestVerificationToken = $('#__AjaxAntiForgeryForm input[name=__RequestVerificationToken]').val();
    return data;
};

$(document).ready(function () {

    /*
    $.validator.addMethod("sameTo", function (value, element, param) {
        var target = $(param);
        if (value) return value == target.val();
        else return this.optional(element);
    }, "Password mismatch");

    jQuery.validator.setDefaults({
        success: "valid"
    });

    jQuery.validator.addMethod("lettersonly", function (value, element) {
        return this.optional(element) || /^[a-z\s]+$/i.test(value);
    }, "Only alphabetical characters");

    $('#registerForm').validate({
        rules: {
            fullname: {
                required: true,
                minlength: 3,
                lettersonly: true
            },
            email: {
                required: true,
                email: true
            },
            password: {
                required: true,
                minlength: 6
            },
            confirmPassword: {
                required: true,
                minlength: 6,
                sameTo: "#password"
            }
        },
    });
    */
});



$('#registerButton').on('click', function (e) {
    //console.log($("#forgotEmail").val());
    e.preventDefault();
    if (!$("#registerForm").valid()) return;
    var fullname = $("#fullName").val();
    var email = $("#registerEmail").val();
    var pass = $("#password").val();
    $.ajax({
        type: 'POST',
        url: '/Home/Register/',
        data: AddAntiForgeryToken({
            fullname: fullname,
            email: email,
            password: pass,
        }),
        dataType: "json",
        beforeSend: function () {
            $('#registerAlert').html("<div class='alert alert-info'><span aria-hidden='true'><i class='fa fa-refresh fa-pulse'></i></span><span class='sr-only'>Loading: </span> Please Wait...</div>");
        },
        success: function (data) {
            if (data.status == "success") {
                $('#registerAlert').html("<div class='alert alert-success' role='alert'><span class='glyphicon glyphicon-ok' aria-hidden='true'></span><span class='sr-only'>Success: </span> A link has been sent to your email. Please click that link to confirm your email address.</div>");
                $("#fullName").val('');
                $("#fullName").blur();
                $("#registerEmail").val('');
                $("#registerEmail").blur();
                $("#password").val('');
                $("#password").blur();
                $("#confirmPassword").val('');
                $("#confirmPassword").blur();
            }
            else if(data.status == "used") {
                $('#registerAlert').html("<div class='alert alert-danger' role='alert'><span class='glyphicon glyphicon-remove' aria-hidden='true'></span><span class='sr-only'>Error: </span> Email has been used. Please use another email.</div>");
                $("#registerEmail").val('');
                $("#registerEmail").focus();
            }
            else {
                $('#registerAlert').html("<div class='alert alert-danger' role='alert'><span class='glyphicon glyphicon-exclamation-sign' aria-hidden='true'></span><span class='sr-only'>Error: </span> An error has occured. Please try again later or contact our admin.</div>");
                $("#fullName").blur();
                $("#registerEmail").blur();
                $("#password").blur();
                $("#confirmPassword").blur();
            }
        },
        error: function (error) {
            alert(error);
        }
    });
});