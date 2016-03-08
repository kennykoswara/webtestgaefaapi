var addUrlParam = function (search, key, val) {
    var newParam = key + '=' + val,
        params = '?' + newParam;

    // If the "search" string exists, then build params from it
    if (search) {
        // Try to replace an existance instance
        params = search.replace(new RegExp('([\?&])' + key + '[^&]*'), '$1' + newParam);

        // If nothing was replaced, then add the new param to the end
        if (params === search) {
            params += '&' + newParam;
        }
    }

    return params;
};

var details;
var bookCodeNumber;
var bookStatus;
var method;
var date;

var passCount = 0;
var adultCount = 0;
var childCount = 0;
var priceBeforeTax;
var priceAfterTax;
var packMin = 0;
var priceMin = 0;
var couponStat = 0;

var totalPrice = 0;
var discPercentage = 0;
var discNotPercentage = 0;
//var discPriceAfterTax = 0;
var discPriceBeforeTax = 0;
var ticketPrice = 0;
var taxPrice = 0;
var pricePerPax;
var discFlag = 0;
var couponDetailType;

$('input[type=number][name=adult]').on('change', function () {
    adultCount = parseInt($(this).val());
    updatePriceAndCount();
});

$('input[type=number][name=child]').on('change', function () {
    childCount = parseInt($(this).val());
    updatePriceAndCount();
});

function updatePriceAndCount() {
    passCount = adultCount + childCount;
    priceBeforeTax = parseFloat((parseFloat(passCount) * pricePerPax).toFixed(2));
    ticketPrice = priceBeforeTax;
    //discPriceAfterTax = ((discPercentage / 100) * priceAfterTax).toFixed(3);
    //discPriceAfterTax = parseFloat(discPriceAfterTax.toString().substring(0, discPriceAfterTax.length-1)).toFixed(2);
    if (discFlag == 0) {
        discPriceBeforeTax = parseFloat(((discPercentage / 100) * priceBeforeTax).toFixed(2));
    }
    else {
        discPriceBeforeTax = parseFloat(discNotPercentage.toFixed(2));
    }
    priceAfterTax = (((priceBeforeTax - discPriceBeforeTax) + 0.3) * 1000 / 961).toFixed(2);
    taxPrice = priceAfterTax - (priceBeforeTax - discPriceBeforeTax);

    if ($('input[name="payopt"]:checked').val() == "Transfer") {
        $("#ticketPrice").text(priceBeforeTax);
        $('input[type=hidden][name=totalPrice]').val(priceBeforeTax);
        $('#discPrice').text(discPriceBeforeTax);
        $('#taxPrice').text("0.00");
        if (discPriceBeforeTax > priceBeforeTax) {
            $("#totalPrice").text(((priceBeforeTax - discPriceBeforeTax)).toFixed(2) + " -> US$ 0.00");
        }
        else {
            $("#totalPrice").text(((priceBeforeTax - discPriceBeforeTax)).toFixed(2));
        }
    }
    else {
        $("#ticketPrice").text(priceBeforeTax);
        $('input[type=hidden][name=totalPrice]').val(priceAfterTax);
        $('#discPrice').text(discPriceBeforeTax);
        if (discPriceBeforeTax > priceBeforeTax) {
            $("#totalPrice").text(((priceBeforeTax - discPriceBeforeTax) + 0.00).toFixed(2) + " -> US$ 0.00");
            $('#additionalFee').text("0.00");
            $('#taxPrice').text("0.00");
        }
        else {
            $("#totalPrice").text(((priceBeforeTax - discPriceBeforeTax) + taxPrice).toFixed(2));
            $('#additionalFee').text((taxPrice).toFixed(2));
            $('#taxPrice').text((taxPrice).toFixed(2));
        }
    }

    $('#peopleCount').text(passCount);
    $('input[type=hidden][name=peopleCount]').val(passCount);
};


$('input[name="payopt"]').change(function () {
    priceBeforeTax = (parseFloat(passCount) * parseFloat($("#pricePerPax").text())).toFixed(2);
    priceAfterTax = (((parseFloat(passCount) * parseFloat($("#pricePerPax").text())) + 0.3) * 1000 / 961).toFixed(2);
    //$('#additionalFee').text(((((parseFloat($('#totalPrice').text())) + 0.3) * 1000 / 961) - parseFloat($('#totalPrice').text())).toFixed(2));
    $(this).tab('show');
    if ($('input[name="payopt"]:checked').val() == "Transfer") {
        $('#iconPayOpt').html("<i class='fa fa-credit-card' title='Pay by transfer'></i>");
        updatePriceAndCount();
    }
    else {
        $('#iconPayOpt').html("<i class='fa fa-paypal' title='Pay by PayPal'></i>");
        updatePriceAndCount();
    }
});

$(document).ready(function () {
    pricePerPax = parseFloat($("#pricePerPax").text());

    $('input[name="payopt"]').filter('[value=Transfer]').prop('checked', true);
    $('input[name="adult"]').val(0);
    $('input[name="child"]').val(0);

    $('#tourDate').on('change', function () {
        var date = new Date($(this).val());
        if (isNaN(date)) {
            $('#showDateToGo').text("INVALID DATE");
        }
        else {
            $('#showDateToGo').text(date.getDate() + " " + date.toLocaleString("en-us", { month: "short" }) + " " + date.getFullYear());
        }
        //console.log(date.getDate() + "/" + parseInt(date.getMonth()+1) + "/" + date.getFullYear());
    });

    $('#addCoupon').prop('disabled', true);


    $("#ticketPrice").text((parseFloat(passCount) * parseFloat($("#pricePerPax").text())).toFixed(2));
    $("#discPrice").text((parseFloat(passCount) * parseFloat($("#pricePerPax").text())).toFixed(2));
    $("#taxPrice").text((parseFloat(passCount) * parseFloat($("#pricePerPax").text())).toFixed(2));
    $("#totalPrice").text((parseFloat(passCount) * parseFloat($("#pricePerPax").text())).toFixed(2));
    $("#peopleCount b").text(passCount);
    $('input[type=hidden][name=peopleCount]').val(passCount);
    $('#additionalFee').text(parseFloat((parseFloat(passCount) * parseFloat($("#pricePerPax").text())).toFixed(2)));

    priceBeforeTax = parseFloat((parseFloat(passCount) * parseFloat($("#pricePerPax").text())).toFixed(2));
    priceAfterTax = parseFloat((((priceBeforeTax - discPriceBeforeTax) + 0.3) * 1000 / 961).toFixed(2));

    $('#pagination').twbsPagination({
        startPage: 1,
        totalPages: ($('#PackageAmount').val() != null) ? Math.ceil(parseInt($('#PackageAmount').val()) / parseInt($('#LimitPerPage').val())) : 1,
        visiblePages: 3,
        href: document.location.pathname + addUrlParam(document.location.search, 'page', '{{number}}'),
    });
    
})


$('.areYouSure').on('click', '.cancel-booking', function (e) {
    e.preventDefault();
    $(this).parent('.areYouSure').html("Are you sure? " + "<input type='submit' name='cancelDecision' class='btn btn-success' value='Yes'/>" + " " + "<input type='submit' name='cancelDecision' class='btn btn-danger' value='No'/>");
});



$('.areYouSure').on('click', "input[name='cancelDecision']", function (e) {
    e.preventDefault();
    var bookCode = $("#bookingCode").text();
    //var bookCode = $('#bookingCode').val();
    //alert(bookCode);
    if ($(this).val() == "No") {
        $(this).parent('.areYouSure').html("<input type='submit' class='btn btn-warning cancel-booking' value='Cancel booking' />");
    }
    else {
        $.ajax({
            type: 'POST',
            url: '/Gaefa/CancelBooking',
            data: AddAntiForgeryToken({
                bookingCode: bookCode,
            }),
            success: function (data) {
                if (data.status == "success") {
                    $('.areYouSure').remove();
                    $('#confirmButton').remove();
                    $('#alertCancelBooking').html("<div class='alert alert-success' role='alert'><span class='glyphicon glyphicon-ok' aria-hidden='true'></span><span class='sr-only'>Success: </span> Your booking has been canceled.</div>");
                    $('#bookingStatus').text("Canceled");
                }
                else {
                    $('#alertCancelBooking').html("<div class='alert alert-success' role='alert'><span class='glyphicon glyphicon-ok' aria-hidden='true'></span><span class='sr-only'>Error: </span> <b>Error: </b> An error has occured while the system is trying to connect to Gaefa database. Please try again later or contact our admin.</div>");
                }
            },
            error: function (error) {
                console.log(error);
                alert("An error has occured");
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
        url: '/Gaefa/ApprovePayment/',
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
                $('.approveChoice').remove();
                $('#approveStatus').html("Approved");
            }
            else if (data.status == "disapprove") {
                $('#approveAlert').html("<div class='alert alert-success' role='alert'><span class='glyphicon glyphicon-ok' aria-hidden='true'></span><span class='sr-only'>Success: </span> <b>Disapprove</b> payment successful.</div>");
                $('.approveChoice').remove();
                $('#approveStatus').html("Disapproved");
            }
            else {
                $('#approveAlert').html("<div class='alert alert-success' role='alert'><span class='glyphicon glyphicon-ok' aria-hidden='true'></span><span class='sr-only'>Error: </span> <b>Error: </b> An error has occured while the system is trying to connect to Gaefa database. Please try again later or contact our admin.</div>");
                $('.approveChoice').show();
            }
        },
        error: function (error) {
            alert(error);
        }
    });
});


$('#findBookingButton').on('click', function (e) {
    //console.log($("#forgotEmail").val());
    //e.preventDefault();
    if ($('#findBookingForm')[0].checkValidity() == true) {
        e.preventDefault();
        var email = $("#emailToFind").val();
        var bookingCode = $("#bookingCodeToFind").val();
        
        $.ajax({
            type: 'POST',
            url: '/Gaefa/FindBooking/',
            data: AddAntiForgeryToken({
                email: email,
                bookingCode: bookingCode,
            }),
            dataType: "json",
            beforeSend: function () {
                $('#showBookingPreview').html("<div class='alert alert-info'><span aria-hidden='true'><i class='fa fa-refresh fa-pulse'></i></span><span class='sr-only'>Loading: </span> Please Wait...</div>");
            },
            success: function (data) {
                if (data.status == "success") {
                    $('#showBookingPreview').load("/Gaefa/GetModule?partialName=BookingPreview.cshtml");
                    details = data.detail;
                    bookCodeNumber = data.bookCode;
                    bookStatus = data.bookStatus;
                    method = data.method;
                    date = data.date;
                }
                else {
                    $('#showBookingPreview').html("<div class='alert alert-danger'><span class='glyphicon glyphicon-exclamation-sign' aria-hidden='true'></span><span class='sr-only'>Error: </span> Tour package not found. Please provide the correct details.</div>");
                }
                
            },
            error: function (error) {
                console.log(error);
            },
        });
        
    }
});


$(document).ajaxSuccess(function (event, request, settings) {
    $('#showBookCode').html(bookCodeNumber);
    if (details != null) {
        $('#showLocation').html(details.location);
    }
    $('#tourDate').html(date);
    $('input[type=hidden][name=bookingCode]').val(bookCodeNumber);
    $('#toDetailForm').attr("action", "/Gaefa/DetailWithoutPayment/");
    if(bookStatus == "Paid") {
        if (method == "Transfer") {
            $('#toDetailForm').attr("action", "/Gaefa/DetailWithPayment/");
        }
        else {
            $('#toDetailForm').attr("action", "/Gaefa/PayPalDetail/");
        }
    }
    $('#showPaymentStatus').html(bookStatus);
});


$("#showNote").click(function () {
    if ($(this).parents('.panel-heading').nextUntil('.panel-collapse .collapse').is(":visible")) {
        $(this).html("<i class='fa fa-chevron-down'></i>");
    }
    else {
        $(this).html("<i class='fa fa-chevron-up'></i>");
    }
    $(this).parents('.panel-heading').nextUntil('.panel-collapse .collapse').slideToggle(500);
});


$("#showDayOnCheckout").click(function () {
    if ($(this).next().next().is(":visible")) {
        $(this).html("<i class='fa fa-chevron-down'></i>");
    }
    else {
        $(this).html("<i class='fa fa-chevron-up'></i>");
    }
    $(this).next().next().slideToggle(500);
});


$('#checkoutCart').on('click', function (e) {
    //console.log($("#forgotEmail").val());
    var minimumPack = parseInt($("#minimumPack").text());
    if (parseInt($('#peopleCount').text()) < minimumPack) {
        e.preventDefault();
        $('#minimumPackAlert').html("<div class='alert alert-danger' role='alert'><span class='glyphicon glyphicon-exclamation-sign' aria-hidden='true'></span><span class='sr-only'>Error: </span> You have to buy minimum of " + minimumPack + " pack(s)!</div>");
    }
    else {
        $('#minimumPackAlert').html('');
    }
});

$('.input-group-btn').on('click', '#addCoupon', function (e) {
    e.preventDefault();
    var coupon = $('input[type=text][name=couponDisc]').val();
    $.ajax({
        type: 'POST',
        url: '/Gaefa/CheckCoupon/',
        data: AddAntiForgeryToken({
            couponCode: coupon,
        }),
        dataType: "json",
        beforeSend: function () {
            $('#couponLogo').html("<span class='text-info'><i class='fa fa-refresh fa-spin'></i></span>");
        },
        success: function (data) {
            if (data.status == "success") {
                discFlag = data.flag;
                if (discFlag == 0) {
                    discPercentage = data.coupon.discPercentage;
                    discNotPercentage = 0;
                    couponDetailType = discPercentage + "%";
                }
                else {
                    discPercentage = 0;
                    discNotPercentage = data.coupon.discPrice;
                    couponDetailType = "US$ " + discNotPercentage;
                }

                if (data.coupon.packMin != null && data.coupon.priceMin != null) {
                    $('#couponDetail').html("Coupon discount: " + couponDetailType +" (min " + data.coupon.packMin + " pax <i>or</i> min US$ " + data.coupon.priceMin + ")");
                    packMin = data.coupon.packMin;
                    priceMin = data.coupon.priceMin;
                    couponStat = 3;
                }
                else if (data.coupon.packMin != null && data.coupon.priceMin == null) {
                    $('#couponDetail').html("Coupon discount: " + couponDetailType + " (min " + data.coupon.packMin + " pax)");
                    packMin = data.coupon.packMin;
                    couponStat = 1;
                }
                else {
                    $('#couponDetail').html("Coupon discount: " + couponDetailType + " (min US$ " + data.coupon.priceMin + ")");
                    priceMin = data.coupon.priceMin;
                    couponStat = 2;
                }
                $('#couponLogo').html("<span class='text-success'><i class='fa fa-check-circle'></i></span>");
                $('input[type=text][name=couponDisc]').prop('readonly', true);
                $('#dynamicCouponButton').html('<button id="cancelCoupon" class="btn btn-primary">Cancel Coupon</button>');
                updatePriceAndCount();
                //$('input[type=hidden][name=couponCode]').val(coupon);
            }
            else if (data.status == "not found"){
                $('#couponDetail').html("Invalid coupon code");
                $('#couponLogo').html("<span class='text-danger'><i class='fa fa-times-circle'></i></span>");
            }
            else if (data.status == "date error") {
                $('#couponDetail').html("Coupon has expired or not yet available");
                $('#couponLogo').html("<span class='text-danger'><i class='fa fa-times-circle'></i></span>");
            }
            else if(data.status == "discount amount error") {
                $('#couponDetail').html("Coupon error.");
                $('#couponLogo').html("<span class='text-danger'><i class='fa fa-times-circle'></i></span>");
            }
            else {
                $('#couponDetail').html("Coupon has been used before");
                $('#couponLogo').html("<span class='text-danger'><i class='fa fa-times-circle'></i></span>");
            }
        },
        error: function (error) {
            console.log(error);
        },
    });
});

$('input[type=text][name=couponDisc]').on('keyup', function (e) {
    if ($(this).val().length != 0) {
        $('#addCoupon').prop('disabled', false);
    }
    else {
        $('#addCoupon').prop('disabled', true);
    }
});

$('.input-group-btn').on('click', '#cancelCoupon', function () {
    $('input[type=text][name=couponDisc]').prop('readonly', false);
    $('input[type=text][name=couponDisc]').val('');
    $('#dynamicCouponButton').html('<button id="addCoupon" class="btn btn-primary">Add Coupon</button>');
    $('#couponDetail').html("");
    $('#couponLogo').html("<span class='text-info'><i class='fa fa-ticket fa-spin'></i></span>");
    discPercentage = 0;
    discNotPercentage = 0;
    discPriceBeforeTax = 0;
    updatePriceAndCount();
});

$('#checkoutCart').on('click', function (e) {
    if (couponStat == 1) {
        if (passCount < packMin) {
            e.preventDefault();
            $('#couponMinAlert').html("<div class='alert alert-danger' role='alert'><span class='glyphicon glyphicon-exclamation-sign' aria-hidden='true'></span><span class='sr-only'>Error: </span> Insufficient pack(s) to use coupon. Please cancel your coupon or add more pack(s)!</div>");
        }
        else {
            $('#couponMinAlert').html('');
        }
    }
    else if (couponStat == 2) {
        if ($('input[name=totalPrice]').val() < priceMin) {
            e.preventDefault();
            $('#couponMinAlert').html("<div class='alert alert-danger' role='alert'><span class='glyphicon glyphicon-exclamation-sign' aria-hidden='true'></span><span class='sr-only'>Error: </span> Insufficient price to use coupon. Please cancel your coupon or add more pack(s)!</div>");
        }
        else {
            $('#couponMinAlert').html('');
        }
    }
    else if (couponStat == 3) {
        if ((passCount < packMin) && ($('input[name=totalPrice]').val() < priceMin)) {
            e.preventDefault();
            $('#couponMinAlert').html("<div class='alert alert-danger' role='alert'><span class='glyphicon glyphicon-exclamation-sign' aria-hidden='true'></span><span class='sr-only'>Error: </span> Insufficient pack(s) or price to use coupon. Please cancel your coupon or add more pack(s)!</div>");
        }
        else {
            $('#couponMinAlert').html('');
        }
    }
});