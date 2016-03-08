AddAntiForgeryToken = function (data) {
    data.__RequestVerificationToken = $('#__AjaxAntiForgeryForm input[name=__RequestVerificationToken]').val();
    return data;
};


$('.areYouSure').on('click', '.cancel-booking', function (e) {
    e.preventDefault();
    $(this).parent('.areYouSure').html("Are you sure? " + "<input type='submit' name='cancelDecision' class='btn btn-xs btn-success' value='Yes'/>" + " " + "<input type='submit' name='cancelDecision' class='btn btn-xs btn-danger' value='No'/>");
});



$('.areYouSure').on('click', "input[name='cancelDecision']", function (e) {
    e.preventDefault();
    var bookCode = $(this).parent().siblings(".bookingCode").val();
    //var bookCode = $('#bookingCode').val();
    //alert(bookCode);
    if ($(this).val() == "No") {
        $(this).parent('.areYouSure').html("<input type='submit' class='btn btn-warning btn-xs cancel-booking' value='Cancel booking' />");
    }
    else {
        $.ajax({
            type: 'POST',
            url: '/Booking/CancelBooking',
            data: AddAntiForgeryToken({
                bookingCode: bookCode,
            }),
            success: function (data) {
                //alert("success");
                window.location.reload(true);
            },
            error: function (error) {
                //alert("error");
            },
        });
    }
});

$('.approveChoice').on('click', "input[type='submit']", function (e) {
    //console.log($("#forgotEmail").val());
    e.preventDefault();

    var bookCode = $(this).parent().siblings(".bookingCode").val();
    var approveChoice = $(this).val();
    
    
    $.ajax({
        type: 'POST',
        url: '/Transfer/ApprovePayment/',
        data: AddAntiForgeryToken({
            bookCode: bookCode,
            approveChoice: approveChoice,
        }),
        dataType: "json",
        beforeSend: function () {
            $('#approveAlert').html("<div class='alert alert-info'><span aria-hidden='true'><i class='fa fa-refresh fa-pulse'></i></span><span class='sr-only'>Loading: </span> Please Wait...</div>");
        },
        success: function (data) {
            if (data.status == "approve") {
                $('#approveAlert').html("<div class='alert alert-success' role='alert'><span class='glyphicon glyphicon-ok' aria-hidden='true'></span><span class='sr-only'>Success: </span> <b>Approve</b> payment successful.</div>");
            }
            else {
                $('#approveAlert').html("<div class='alert alert-success' role='alert'><span class='glyphicon glyphicon-ok' aria-hidden='true'></span><span class='sr-only'>Success: </span> <b>Disapprove</b> payment successful.</div>");
            }
            $('.approveChoice').remove();
        },
        error: function (error) {
            alert(error);
        }
    });
});