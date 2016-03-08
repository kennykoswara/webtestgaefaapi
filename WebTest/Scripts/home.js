$(document).ready(function () {
    $('input:checkbox').removeAttr('checked');

    AddAntiForgeryToken = function (data) {
        data.__RequestVerificationToken = $('#__AjaxAntiForgeryForm input[name=__RequestVerificationToken]').val();
        return data;
    };

    if ($('#unPostedPackage').text() == "0") {
        $('#syncToGaefa').prop('disabled', true);
    }
    else {
        $('#syncToGaefa').prop('disabled', false);
    }

    $('#generateCouponButton').prop('disabled', true);
    $('#priceDisc').prop('disabled', true);
    $('#packDisc').prop('disabled', true);
    $('#ticketAmount').prop('disabled', true);
    $('#expiryDate').prop('disabled', true);
    $('#availableDate').prop('disabled', true);
    $('#discPercentage').prop('disabled', true);
    $('#discPrice').prop('disabled', true);

    /*
    $.validator.addMethod("sameTo", function (value, element, param) {
        var target = $(param);
        if (value) return value == target.val();
        else return this.optional(element);
    }, "Password mismatch");


    jQuery.validator.addMethod("lettersonly", function (value, element) {
        return this.optional(element) || /^[a-z\s]+$/i.test(value);
    }, "Only alphabetical characters");


    $('#changeFullNameForm').validate({
        rules: {
            newFullName: {
                required: true,
                minlength: 3,
                lettersonly: true
            },
        },
    });

    $('#changePassword').validate({
        rules: {
            currPass: {
                required: true,
                minlength: 6,
            },
            newPass: {
                required: true,
                minlength: 6,
            },
            confirmNewPass: {
                required: true,
                minlength: 6,
                sameTo : "#newPass",
            },
        },
    });

    $('#forgotPasswordForm').validate({
        rules: {
            email: {
                required: true,
                email: true
            }
        }
    });

    $('#createNewPassword').validate({
        rules: {
            password: {
                required: true,
                minlength: 6,
            },
            confirmPassword: {
                required: true,
                minlength: 6,
                sameTo: "#newPassword"
            },
        }
    });
    */
});

$('#loginButton').on('click', function (e) {
    //console.log($("#forgotEmail").val());
    //e.preventDefault();
    if ($('#loginForm')[0].checkValidity() == true) {
        e.preventDefault();
        var email = $("#email").val();
        var pass = $("#password").val();
        $.ajax({
            type: 'POST',
            url: '/Home/Login/',
            data: AddAntiForgeryToken({
                loginEmail: email,
                loginPassword: pass,
            }),
            dataType: "json",
            beforeSend: function () {
                $('#loginAlert').html("<div class='alert alert-info'><span aria-hidden='true'><i class='fa fa-refresh fa-pulse'></i></span><span class='sr-only'>Loading: </span> Please Wait...</div>");
            },
            success: function (data) {
                if (data.status == "Incorrect") {
                    $('#loginAlert').html("<div class='alert alert-danger' role='alert'><span class='glyphicon glyphicon-exclamation-sign' aria-hidden='true'></span><span class='sr-only'>Error: </span> Incorrect email address or password. Please try again.</div>");
                }
                /*
                else if (data.status == "Unconfirmed") {
                    $('#loginAlert').html("<div class='alert alert-warning' role='alert'><span class='glyphicon glyphicon-warning-sign' aria-hidden='true'></span><span class='sr-only'>Warning: </span> Please confirm your email address first.</div>");
                    $("#email").val('');
                    $("#email").blur();
                    $("#loginPassword").val('');
                    $("#loginPassword").blur();
                }
                */
                else {
                    //window.location.replace('/Home/LoginSuccessful?urlReferrer=' + data.url);
                    window.location.replace('/Home/Index/');
                }
            },
            error: function (error) {
                alert.log("error");
                console.log(error);
            }
        });
    }
});


$('#submitForgotPasswordEmail').on('click', function (e) {
    //console.log($("#forgotEmail").val());
    if ($("#forgotPasswordForm")[0].checkValidity() == true) {
        e.preventDefault();
        var email = $("#forgotEmail").val();
        $.ajax({
            type: 'POST',
            url: '/Home/SendForgotPasswordEmail/',
            data: AddAntiForgeryToken({
                email: email
            }),
            dataType: "json",
            beforeSend: function () {
                $('#sendForgotPasswordEmail').html("<div class='alert alert-info'><span aria-hidden='true'><i class='fa fa-refresh fa-pulse'></i></span><span class='sr-only'>Loading: </span> Please Wait...</div>");
            },
            success: function (data) {
                if (data.status == "success") {
                    $('#sendForgotPasswordEmail').html("<div class='alert alert-success'><span class='glyphicon glyphicon-ok' aria-hidden='true'></span><span class='sr-only'>Success: </span> An email contains a link to reset your password has been sent to your email address.</div>");
                }
                else {
                    $('#sendForgotPasswordEmail').html("<div class='alert alert-danger'><span class='glyphicon glyphicon-exclamation-sign' aria-hidden='true'></span><span class='sr-only'>Error: </span> The email you submitted is not registered in our database. Please try again.</div>");

                }
                $("#forgotEmail").val('');
                $("#forgotEmail").blur();
            },
            error: function (error) {
                alert(error);
            }
        });
    }
});


$('#submitChangePassword').on('click', function (e) {
    //console.log($("#forgotEmail").val());
    //e.preventDefault();
    if ($("#changePassword")[0].checkValidity() == true) {
        e.preventDefault();
        var currPass = $("#currPass").val();
        var newPass = $("#newPass").val();
        $.ajax({
            type: 'POST',
            url: '/Home/SubmitChangePassword/',
            data: AddAntiForgeryToken({
                currPass: currPass,
                newPass: newPass,
            }),
            dataType: "json",
            beforeSend: function () {
                $('#alertCurrentPassword').html("<div class='alert alert-info'><span aria-hidden='true'><i class='fa fa-refresh fa-pulse'></i></span><span class='sr-only'>Loading: </span> Please Wait...</div>");
            },
            success: function (data) {
                if (data.currentPassword == "true") {
                    $('#alertCurrentPassword').html("<div class='alert alert-success' role='alert'><span class='glyphicon glyphicon-ok' aria-hidden='true'></span><span class='sr-only'>Success: </span> Password changed successfully.</div>");
                }
                else {
                    $('#alertCurrentPassword').html("<div class='alert alert-danger' role='alert'><span class='glyphicon glyphicon-exclamation-sign' aria-hidden='true'></span><span class='sr-only'>Error: </span> Your current password is incorrect. Please try again.</div>");

                }
                $("#currPass").val('');
                $("#currPass").blur();
                $("#newPass").val('');
                $("#newPass").blur();
                $("#confirmNewPass").val('');
                $("#confirmNewPass").blur();
            },
            error: function (error) {
                alert(error);
            }
        });
    }
});


$('#submitChangeFullName').on('click', function (e) {
    //console.log($("#forgotEmail").val());
    e.preventDefault();
    if (!$("#changeFullNameForm").valid()) return;
    var newName = $("#newFullName").val();
    $.ajax({
        type: 'POST',
        url: '/Home/SubmitChangeFullName/',
        data: AddAntiForgeryToken({
            newFullName: newName,
        }),
        dataType: "json",
        beforeSend: function () {
            $('#changeFullNameAlert').html("<div class='alert alert-info'><span aria-hidden='true'><i class='fa fa-refresh fa-pulse'></i></span><span class='sr-only'>Loading: </span> Please Wait...</div>");
        },
        success: function (data) {
            if (data.status == "success") {
                $('#changeFullNameAlert').html("<div class='alert alert-success' role='alert'><span class='glyphicon glyphicon-ok' aria-hidden='true'></span><span class='sr-only'>Success: </span> Full name changed successfully.</div>");
                $("#newFullName").val('');
                $("#newFullName").blur();
                $('#currFullName').load(document.URL + ' #currFullName');
                $('#sessionName').load(document.URL + ' #sessionName');
            }
            else {
                $('#changeFullNameAlert').html("<div class='alert alert-danger' role='alert'><span class='glyphicon glyphicon-exclamation-sign' aria-hidden='true'></span><span class='sr-only'>Error: </span> An error has occured. Please try again later or contact our admin.</div>");
            }
        },
        error: function (error) {
            alert(error);
        }
    });
});


$('#submitForgotNewPassword').on('click', function (e) {
    //console.log($("#forgotEmail").val());
    var newPass = $("#newPassword").val();
    var confirmNewPass = $('#confirmNewPassword').val();
    
    if ($("#createNewPassword")[0].checkValidity() == true) {
        if (newPass != confirmNewPass) {
            $('#confirmNewPassword').setCustomValidity("Password does not match");
            e.preventDefault();
        }
        else {
            e.preventDefault();
            var email = $("#emailToChangePassword").val();
            var token = $("#token").val();

            $.ajax({
                type: 'POST',
                url: '/Home/ConfirmNewPassword/',
                data: AddAntiForgeryToken({
                    email: email,
                    token: token,
                    password: newPass,
                }),
                dataType: "json",
                beforeSend: function () {
                    $('#changeForgotNewPasswordAlert').html("<div class='alert alert-info'><span aria-hidden='true'><i class='fa fa-refresh fa-pulse'></i></span><span class='sr-only'>Loading: </span> Please Wait...</div>");
                },
                success: function (data) {
                    if (data.status == "success") {
                        $('#changeForgotNewPasswordAlert').html("<div class='alert alert-success' role='alert'><span class='glyphicon glyphicon-ok' aria-hidden='true'></span><span class='sr-only'>Success: </span> Password changed successfully. Please go to the <a href='/Home/Login'>login page</a> to login.</div>");
                        $("#newPassword").prop('disabled', true);
                        $("#confirmNewPassword").prop('disabled', true);
                    }
                    else {
                        $('#changeForgotNewPasswordAlert').html("<div class='alert alert-danger' role='alert'><span class='glyphicon glyphicon-exclamation-sign' aria-hidden='true'></span><span class='sr-only'>Error: </span> An error has occured. Please try again later or contact our admin.</div>");
                    }
                },
                error: function (error) {
                    alert(error);
                }
            });
        }
    }
});


$('#syncToGaefa').on('click', function (e) {
    $.ajax({
        type: 'POST',
        url: '/Gaefa/SyncWithGaefa/',
        data: AddAntiForgeryToken({
        }),
        dataType: "json",
        beforeSend: function () {
            $('#alertSync').html("<div class='alert alert-info'><span aria-hidden='true'><i class='fa fa-refresh fa-pulse'></i></span><span class='sr-only'>Loading: </span> Please Wait...</div>");
            $('#syncToGaefa').prop('disabled', true);
        },
        success: function (data) {
            if (data.status == "success") {
                $('#alertSync').html("<div class='alert alert-success' role='alert'><span class='glyphicon glyphicon-ok' aria-hidden='true'></span><span class='sr-only'>Success: </span> All packages sync successfully.</div>");
            }
            else if (data.status == "good") {
                $('#alertSync').html("<div class='alert alert-warning'><span class='glyphicon glyphicon-exclamation-sign' aria-hidden='true'></span><span class='sr-only'>Warning: </span> Half of the packages sync successfully.</div>");
                $('#syncToGaefa').prop('disabled', false);
            }
            else {
                $('#alertSync').html("<div class='alert alert-danger'><span class='glyphicon glyphicon-exclamation-sign' aria-hidden='true'></span><span class='sr-only'>Error: </span> Sync has failed. Please try again or contact our admin.</div>");
                $('#syncToGaefa').prop('disabled', false);
            }
            $('#unPostedPackage').text(parseInt($('#unPostedPackage').text()) - parseInt(data.count));
        },
        error: function (error) {
            console.log(error);
            $('#syncToGaefa').prop('disabled', false);
        },
    });
});


$('input[type=checkbox]').on('change', function () {
    /*
    if ($('input[type=checkbox][name=enablePackDisc]').is(':checked') || $('input[type=checkbox][name=enablePriceDisc]').is(':checked')) {
        $('#generateCouponButton').prop('disabled', false);
        $('#ticketAmount').prop('disabled', false);
        $('#expiryDate').prop('disabled', false);
        $('#availableDate').prop('disabled', false);
        if ($("#ticketList").val() == "" || $("#ticketList").val() == null) {
            $('#generateCouponButton').prop('disabled', true);
        }
        else{
            $('#generateCouponButton').prop('disabled', false);
        }
    }
    else {
        $('#generateCouponButton').prop('disabled', true);
        $('#ticketAmount').prop('disabled', true);
        $('#expiryDate').prop('disabled', true);
        $('#availableDate').prop('disabled', true);
    }*/

    if ($('input[type=checkbox][name=enablePackDisc]').is(':checked') || $('input[type=checkbox][name=enablePriceDisc]').is(':checked')) {
        $('#ticketAmount').prop('disabled', false);
        $('#expiryDate').prop('disabled', false);
        $('#availableDate').prop('disabled', false);
        $('#generateCouponButton').prop('disabled', false);
        if ($('input[name="discType"]:checked').val() == "percentage") {
            $('#discPercentage').prop('disabled', false);
            $('#discPrice').prop('disabled', true);
        }
        else if ($('input[name="discType"]:checked').val() == "price") {
            $('#discPercentage').prop('disabled', true);
            $('#discPrice').prop('disabled', false);
        }
    }
    else {
        $('#generateCouponButton').prop('disabled', true);
        $('#ticketAmount').prop('disabled', true);
        $('#expiryDate').prop('disabled', true);
        $('#availableDate').prop('disabled', true);
        $('#discPercentage').prop('disabled', true);
        $('#discPrice').prop('disabled', true);
    }
});

$('input[name="discType"]').on('change', function () {
    if ($('input[type=checkbox][name=enablePackDisc]').is(':checked') || $('input[type=checkbox][name=enablePriceDisc]').is(':checked')) {
        if ($('input[name="discType"]:checked').val() == "percentage") {
            $('#discPercentage').prop('disabled', false);
            $('#discPrice').prop('disabled', true);
        }
        else if ($('input[name="discType"]:checked').val() == "price") {
            $('#discPercentage').prop('disabled', true);
            $('#discPrice').prop('disabled', false);
        }
    }
});

$('input[type=checkbox][name=enablePackDisc]').on('change', function () {
    if ($(this).is(':checked')) {
        $('#packDisc').prop('disabled', false);
    }
    else {
        $('#packDisc').prop('disabled', true);
    }
});

$('input[type=checkbox][name=enablePriceDisc]').on('change', function () {
    if ($(this).is(':checked')) {
        $('#priceDisc').prop('disabled', false);
    }
    else {
        $('#priceDisc').prop('disabled', true);
    }
});


$('#ticketList').on('change', function (e) {
    if ($("#ticketList").val() == "" || $("#ticketList").val() == null) {
        $('#generateCouponButton').prop('disabled', true);
    }
    else {
        var id = parseInt($("#ticketList option:selected").text());
        $.ajax({
            type: 'POST',
            url: '/Gaefa/GetPackage/',
            data: AddAntiForgeryToken({
                id: id,
            }),
            dataType: "json",
            beforeSend: function () {
                $('#alertCoupon').html("<div class='alert alert-info'><span aria-hidden='true'><i class='fa fa-refresh fa-pulse'></i></span><span class='sr-only'>Loading: </span> Please Wait...</div>");
            },
            success: function (data) {
                if (data.detail == "error") {
                    $('#alertCoupon').html("<div class='alert alert-danger' role='alert'><span class='glyphicon glyphicon-exclamation-sign' aria-hidden='true'></span><span class='sr-only'>Error: </span> Package not found.</div>");
                }
                else {
                    $('#alertCoupon').html("<div class='alert alert-success'><span class='glyphicon glyphicon-ok' aria-hidden='true'></span><span class='sr-only'>Success: </span> Ticket detail successfully retrieved.</div>");
                    $('#ticketName').text(data.detail.name);
                    $('#startDate').text(data.detail.startDate.substring(0,10));
                    $('#endDate').text(data.detail.endDate);
                    $('#minPack').text(data.detail.minimumPack);
                    $('#pricePerPack').text(data.detail.pricePerPack);

                    if ($('input[type=checkbox][name=enablePackDisc]').is(':checked') || $('input[type=checkbox][name=enablePriceDisc]').is(':checked')) {
                        $('#generateCouponButton').prop('disabled', false);
                    }
                    else {
                        $('#generateCouponButton').prop('disabled', true);
                    }
                }
            },
            error: function (error) {
                alert(error);
            }
        });
    }
});

$('#generateCouponButton').on('click', function (e) {
    if ($("#couponForm")[0].checkValidity() == true) {
        e.preventDefault();
        var id = parseInt($("#ticketList option:selected").text());
        var minPack;
        var minPrice;
        var ticketAmount;
        var expiryDate;
        var availableDate;
        var discPercentage;

        if ($('#packDisc').prop('disabled')) {
            minPack = null;
        }
        else {
            minPack = $('#packDisc').val();
        }
        
        if ($('#priceDisc').prop('disabled')) {
            minPrice = null;
        }
        else {
            minPrice = $('#priceDisc').val();
        }

        discType = $('input[name="discType"]:checked').val();
        ticketAmount = $('#ticketAmount').val();
        availableDate = $('#availableDate').val();
        expiryDate = $('#expiryDate').val();
        discPercentage = $('#discPercentage').val()
        discPrice = $('#discPrice').val();

        $.ajax({
            type: 'POST',
            url: '/Home/GenerateCoupon/',
            data: AddAntiForgeryToken({
                discType: discType,
                ticketID: id,
                minPack: minPack,
                minPrice: minPrice,
                ticketAmount: ticketAmount,
                expiryDate: expiryDate,
                availableDate: availableDate,
                discPercentage: discPercentage,
                discPrice: discPrice,
            }),
            dataType: "json",
            beforeSend: function () {
                $('#alertCoupon').html("<div class='alert alert-info'><span aria-hidden='true'><i class='fa fa-refresh fa-pulse'></i></span><span class='sr-only'>Loading: </span> Please Wait...</div>");
            },
            success: function (data) {
                if (data.status == "success") {
                    $('#alertCoupon').html("<div class='alert alert-success'><span class='glyphicon glyphicon-ok' aria-hidden='true'></span><span class='sr-only'>Success: </span> " + data.couponCount + " coupon(s) successfully created.</div>");
                }
                else {
                    $('#alertCoupon').html("<div class='alert alert-danger'><span class='glyphicon glyphicon-exclamation-sign' aria-hidden='true'></span><span class='sr-only'>Error: </span> An error has occured.</div>");
                }                
            },
            error: function (error) {
                alert(error);
            }
        });
    }
});

$('#availableDate').on('change', function () {
    $('#expiryDate').prop('min', $('#availableDate').val());
});

$('#sendEmailToGaefa').on('click', function () {
    $('#mailDiv').toggle();
});

$('#sendMailToAdminButton').on('click', function (e) {
    //console.log($("#forgotEmail").val());
    //e.preventDefault();
    if ($('#sendMailToAdmin')[0].checkValidity() == true) {
        e.preventDefault();
        var subject = $("#emailSubject").val();
        var body = $("#emailBody").val();
        $.ajax({
            type: 'POST',
            url: '/Home/ReportIssueToGaefaAdmin/',
            data: AddAntiForgeryToken({
                emailSubject: subject,
                emailBody: body,
            }),
            dataType: "json",
            beforeSend: function () {
                $('#alertMail').html("<div class='alert alert-info'><span aria-hidden='true'><i class='fa fa-refresh fa-pulse'></i></span><span class='sr-only'>Loading: </span> Please Wait...</div>");
            },
            success: function (data) {
                if (data.status == "success") {
                    $('#alertMail').html("<div class='alert alert-success'><span class='glyphicon glyphicon-ok' aria-hidden='true'></span><span class='sr-only'>Success: </span> Email sent.</div>");
                    $('#emailSubject').val('');
                    $('#emailBody').val('');
                }
                else{
                    $('#alertMail').html("<div class='alert alert-danger' role='alert'><span class='glyphicon glyphicon-exclamation-sign' aria-hidden='true'></span><span class='sr-only'>Error: </span> An error has occured.</div>");
                }
            },
            error: function (error) {
                $('#alertMail').html("<div class='alert alert-danger' role='alert'><span class='glyphicon glyphicon-exclamation-sign' aria-hidden='true'></span><span class='sr-only'>Error: </span> An error has occured.</div>");
                console.log(error);
            }
        });
    }
});