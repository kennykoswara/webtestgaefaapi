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

String.prototype.toProperCase = function () {
    return this.replace(/\w\S*/g, function (txt) { return txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase(); });
};

var enabledDates = [];
var allowedDaysOfWeek = [];
var maxPassSelected = [];

var details;
var bookCodeNumber;
var bookStatus;
var method;
var date;

var passCount = 0;
var adultCount = 0;
var childCount = 0;
var childNoBedCount = 0;
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
//var pricePerPax;
var priceAdult;
var priceChild;
var priceChildNoBed;
var discFlag = 0;
var couponDetailType;
var promoDetailType;

$('input[type=number][name=adult]').on('change', function () {
    adultCount = parseInt($(this).val());
    updatePriceAndCount();
});

$('input[type=number][name=child]').on('change', function () {
    childCount = parseInt($(this).val());
    updatePriceAndCount();
});

$('input[type=number][name=childNoBed]').on('change', function () {
    childNoBedCount = parseInt($(this).val());
    updatePriceAndCount();
});

function updatePriceAndCount() {
    passCount = adultCount + childCount + childNoBedCount;
    priceBeforeTax = parseFloat((parseFloat(adultCount) * priceAdult) + (parseFloat(childCount) * priceChild) + (parseFloat(childNoBedCount) * priceChildNoBed)).toFixed(2);
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
        if (discPriceBeforeTax >= priceBeforeTax) {
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
    $('.dateEnabled').each(function () {
        enabledDates.push($(this).val());
    });

    if($('#isSunday').val() == "True"){
        allowedDaysOfWeek.push(0);
    }
    if($('#isMonday').val() == "True"){
        allowedDaysOfWeek.push(1);
    }
    if($('#isTuesday').val() == "True"){
        allowedDaysOfWeek.push(2);
    }
    if($('#isWednesday').val() == "True"){
        allowedDaysOfWeek.push(3);
    }
    if($('#isThursday').val() == "True"){
        allowedDaysOfWeek.push(4);
    }
    if($('#isFriday').val() == "True"){
        allowedDaysOfWeek.push(5);
    }
    if($('#isSaturday').val() == "True"){
        allowedDaysOfWeek.push(6);
    }

    $('.maxPassAllowed').each(function () {
        maxPassSelected.push($(this).val());
    });

    $('#tourDate').val('');

    priceAdult = parseFloat($("#priceAdult").val());
    priceChild = parseFloat($("#priceChild").val());
    priceChildNoBed = parseFloat($("#priceChildNoBed").val());

    $('input[name="payopt"]').filter('[value=Transfer]').prop('checked', true);
    $('input[name="adult"]').val(0);
    $('input[name="child"]').val(0);

    
    $('#addCoupon').prop('disabled', true);

    /*
    $("#ticketPrice").text((parseFloat(passCount) * parseFloat($("#pricePerPax").text())).toFixed(2));
    $("#discPrice").text((parseFloat(passCount) * parseFloat($("#pricePerPax").text())).toFixed(2));
    $("#taxPrice").text((parseFloat(passCount) * parseFloat($("#pricePerPax").text())).toFixed(2));
    $("#totalPrice").text((parseFloat(passCount) * parseFloat($("#pricePerPax").text())).toFixed(2));
    $("#peopleCount b").text(passCount);
    $('input[type=hidden][name=peopleCount]').val(passCount);
    $('#additionalFee').text(parseFloat((parseFloat(passCount) * parseFloat($("#pricePerPax").text())).toFixed(2)));
    */
    $("#ticketPrice").text(0.00);
    $("#discPrice").text(0.00);
    $("#taxPrice").text(0.00);
    $("#totalPrice").text(0.00);
    $("#peopleCount b").text(passCount);
    $('input[type=hidden][name=peopleCount]').val(passCount);
    $('#additionalFee').text(0.00);

    priceBeforeTax = parseFloat(0.00);
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
                    $('#alertCancelBooking').html("<div class='alert alert-success' role='alert'><span class='glyphicon glyphicon-ok' aria-hidden='true'></span><span class='sr-only'>Error: </span> <b>Error: </b> An error has occured.</div>");
                }
            },
            error: function (error) {
                console.log(error);
                $('#alertCancelBooking').html("<div class='alert alert-success' role='alert'><span class='glyphicon glyphicon-ok' aria-hidden='true'></span><span class='sr-only'>Error: </span> <b>Error: </b> An error has occured.</div>");
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
                $('#approveAlert').html("<div class='alert alert-danger' role='alert'><span class='glyphicon glyphicon-ok' aria-hidden='true'></span><span class='sr-only'>Error: </span><b>Error: </b> An error has occured while the system is trying to connect to Gaefa database. Please try again later or contact our admin.</div>");
                $('.approveChoice').show();
            }
        },
        error: function (error) {
            console.log(error);
            $('#approveAlert').html("<div class='alert alert-danger' role='alert'><span class='glyphicon glyphicon-ok' aria-hidden='true'></span><span class='sr-only'>Error: </span> <b>Error: </b> An error has occured.</div>");
            $('.approveChoice').show();
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
                $('#showBookingPreview').html("<div class='alert alert-danger'><span class='glyphicon glyphicon-exclamation-sign' aria-hidden='true'></span><span class='sr-only'>Error: </span> An error has occured.</div>");
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
    $('#bookCodePreview').val(bookCodeNumber);
    if (bookStatus == "Unpaid" || bookStatus == "Disapproved") {
        $('#toDetailForm').attr("action", "/Gaefa/DetailWithoutPayment/");
    }
    else {
        if (method == "Transfer") {
            $('#toDetailForm').attr("action", "/Gaefa/DetailWithPayment/");
        }
        else if (method == "Zero") {
            $('#toDetailForm').attr("action", "/Gaefa/ZeroPaymentDetail/");
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

$('.input-group-btn').on('click', '#addCoupon', function (e) {
    e.preventDefault();
    var code = $('input[type=text][name=couponDisc]').val();

    $.ajax({
        type: 'POST',
        url: '/Gaefa/CheckCodeType/',
        data: AddAntiForgeryToken({
            code: code,
        }),
        dataType: "json",
        beforeSend: function () {
            $('#couponLogo').html("<span class='text-info'><i class='fa fa-refresh fa-spin'></i></span>");
        },
        success: function (data) {
            if (data.type == 0) {
                if (data.status == "success") {
                    discFlag = data.flag;
                    if (discFlag == 0) {
                        discPercentage = data.discPercentage;
                        discNotPercentage = 0;
                        couponDetailType = discPercentage + "%";
                    }
                    else {
                        discPercentage = 0;
                        discNotPercentage = data.discPrice;
                        couponDetailType = "US$ " + discNotPercentage;
                    }

                    if (data.packMin != null && data.priceMin != null) {
                        $('#couponDetail').html("Coupon discount: " + couponDetailType + " (min " + data.packMin + " pax <i>or</i> min US$ " + data.priceMin + ")");
                        packMin = data.packMin;
                        priceMin = data.priceMin;
                        couponStat = 3;
                    }
                    else if (data.packMin != null && data.priceMin == null) {
                        $('#couponDetail').html("Coupon discount: " + couponDetailType + " (min " + data.packMin + " pax)");
                        packMin = data.packMin;
                        couponStat = 1;
                    }
                    else {
                        $('#couponDetail').html("Coupon discount: " + couponDetailType + " (min US$ " + data.priceMin + ")");
                        priceMin = data.priceMin;
                        couponStat = 2;
                    }
                    $('#couponLogo').html("<span class='text-success'><i class='fa fa-check-circle'></i></span>");
                    $('input[type=text][name=couponDisc]').prop('readonly', true);
                    $('#dynamicCouponButton').html('<button id="cancelCoupon" class="btn btn-primary">Cancel Coupon</button>');
                    updatePriceAndCount();
                    //$('input[type=hidden][name=couponCode]').val(coupon);
                }
                else if (data.status == "not found") {
                    $('#couponDetail').html("Invalid coupon code");
                    $('#couponLogo').html("<span class='text-danger'><i class='fa fa-times-circle'></i></span>");
                }
                else if (data.status == "date error") {
                    $('#couponDetail').html("Coupon has expired or not yet available");
                    $('#couponLogo').html("<span class='text-danger'><i class='fa fa-times-circle'></i></span>");
                }
                else if (data.status == "discount amount error") {
                    $('#couponDetail').html("Coupon error.");
                    $('#couponLogo').html("<span class='text-danger'><i class='fa fa-times-circle'></i></span>");
                }
                else {
                    $('#couponDetail').html("Coupon has been used before");
                    $('#couponLogo').html("<span class='text-danger'><i class='fa fa-times-circle'></i></span>");
                }
            }
            else if (data.type == 1) {
                if (data.status == "success") {
                    discFlag = data.flag;
                    if (discFlag == 0) {
                        discPercentage = data.discPercentage;
                        discNotPercentage = 0;
                        promoDetailType = discPercentage + "%";
                    }
                    else {
                        discPercentage = 0;
                        discNotPercentage = data.discPrice;
                        promoDetailType = "US$ " + discNotPercentage;
                    }

                    if (data.packMin != null && data.priceMin != null) {
                        $('#couponDetail').html("Promo discount: " + promoDetailType + " (min " + data.packMin + " pax <i>or</i> min US$ " + data.priceMin + ")");
                        packMin = data.packMin;
                        priceMin = data.priceMin;
                        couponStat = 3;
                    }
                    else if (data.packMin != null && data.priceMin == null) {
                        $('#couponDetail').html("Promo discount: " + promoDetailType + " (min " + data.packMin + " pax)");
                        packMin = data.packMin;
                        couponStat = 1;
                    }
                    else {
                        $('#couponDetail').html("Promo discount: " + promoDetailType + " (min US$ " + data.priceMin + ")");
                        priceMin = data.priceMin;
                        couponStat = 2;
                    }
                    $('#couponLogo').html("<span class='text-success'><i class='fa fa-check-circle'></i></span>");
                    $('input[type=text][name=couponDisc]').prop('readonly', true);
                    $('#dynamicCouponButton').html('<button id="cancelCoupon" class="btn btn-primary">Cancel Promo</button>');
                    updatePriceAndCount();
                    //$('input[type=hidden][name=couponCode]').val(coupon);
                }
                else if (data.status == "not found") {
                    $('#couponDetail').html("Invalid promo code");
                    $('#couponLogo').html("<span class='text-danger'><i class='fa fa-times-circle'></i></span>");
                }
                else if (data.status == "date error") {
                    $('#couponDetail').html("Promo code has expired or not yet available");
                    $('#couponLogo').html("<span class='text-danger'><i class='fa fa-times-circle'></i></span>");
                }
                else if (data.status == "discount amount error") {
                    $('#couponDetail').html("Promo code error.");
                    $('#couponLogo').html("<span class='text-danger'><i class='fa fa-times-circle'></i></span>");
                }
                else {
                    $('#couponDetail').html("All promo code has been used. You are late.");
                    $('#couponLogo').html("<span class='text-danger'><i class='fa fa-times-circle'></i></span>");
                }
            }
            else {
                $('#couponDetail').html("Invalid code");
                $('#couponLogo').html("<span class='text-danger'><i class='fa fa-times-circle'></i></span>");
            }
            
        },
        error: function (error) {
            console.log(error);
            $('#couponDetail').html("An error has occured");
            $('#couponLogo').html("<span class='text-danger'><i class='fa fa-times-circle'></i></span>");
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
    $('#dynamicCouponButton').html('<button id="addCoupon" class="btn btn-primary">Add Code</button>');
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

    tourDate = document.getElementById("tourDate");

    if (tourDate.value == "" || tourDate.value == null) {
        tourDate.setCustomValidity("Date is required");
    }
    else {
        var splitter = tourDate.value.split('-');
        var tourDay = parseInt(splitter[2], 10);
        var tourMonth = parseInt(splitter[1], 10);
        var tourYear = parseInt(splitter[0], 10);
        var date = new Date(tourYear, tourMonth - 1, tourDay);

        if (date.getFullYear() == tourYear && date.getMonth() + 1 == tourMonth && date.getDate() == tourDay) {
            if (enabledDates.length > 0) {
                if ($.inArray(tourDate.value, enabledDates) == -1) {
                    tourDate.setCustomValidity("Date is not available for this ticket");
                }
                else {
                    if (new Date(tourDate.value).setHours(0,0,0,0) < new Date().setHours(0,0,0,0)) {
                        console.log(new Date(tourDate.value));
                        console.log(new Date());
                        tourDate.setCustomValidity("Date is not available for this ticket");
                    }
                    else {
                        tourDate.setCustomValidity("");
                    }
                }

                if (passCount > maxPassSelected[enabledDates.indexOf(tourDate.value)]) {
                    e.preventDefault();
                    $('#maximumPackAlert').html("<div class='alert alert-danger' role='alert'><span class='glyphicon glyphicon-exclamation-sign' aria-hidden='true'></span><span class='sr-only'>Error: </span> You can only buy maximum of " + maxPassSelected[enabledDates.indexOf(tourDate.value)] + " packs !</div>");
                }
                else {
                    $('#maximumPackAlert').html("");
                }

                if (passCount < 1) {
                    e.preventDefault();
                    $('#minimumPackAlert').html("<div class='alert alert-danger' role='alert'><span class='glyphicon glyphicon-exclamation-sign' aria-hidden='true'></span><span class='sr-only'>Error: </span> You have to buy minimum of 1 pack!</div>");
                }
                else {
                    $('#minimumPackAlert').html("");
                }
            }
            else {
                if (new Date(tourDate.value).setHours(0, 0, 0, 0) < new Date($('#rangedStartDate').val()).setHours(0, 0, 0, 0) || new Date(tourDate.value).setHours(0, 0, 0, 0) > new Date($('#rangedEndDate').val()).setHours(0, 0, 0, 0) || new Date(tourDate.value).setHours(0, 0, 0, 0) < new Date().setHours(0, 0, 0, 0)) {
                    tourDate.setCustomValidity("Date is not available for this ticket");
                }
                else {
                    if ($.inArray(new Date(tourDate.value).setHours(0,0,0,0).getDay(), allowedDaysOfWeek) == -1) {
                        tourDate.setCustomValidity("Date is not available for this ticket");
                    }
                    else {
                        tourDate.setCustomValidity("");
                    }
                }

                if (passCount < parseInt($('#rangedMinimumPack').val())) {
                    e.preventDefault();
                    $('#minimumPackAlert').html("<div class='alert alert-danger' role='alert'><span class='glyphicon glyphicon-exclamation-sign' aria-hidden='true'></span><span class='sr-only'>Error: </span> You have to buy minimum of " + parseInt($('#rangedMinimumPack').val()) + " pack(s)!</div>");
                }
                else {
                    $('#minimumPackAlert').html("");
                }

                if (passCount > parseInt($('#rangedMaximumPack').val())) {
                    e.preventDefault();
                    $('#maximumPackAlert').html("<div class='alert alert-danger' role='alert'><span class='glyphicon glyphicon-exclamation-sign' aria-hidden='true'></span><span class='sr-only'>Error: </span> You can only buy maximum of " + parseInt($('#rangedMaximumPack').val()) + " packs !</div>");
                }
                else {
                    $('#maximumPackAlert').html("");
                }
            }
        }
        else {
            tourDate.setCustomValidity("Please specify a valid date in dd-MM-yyyy format");
        }

        
    }

});

function EnableSpecificDates(date) {
    if (enabledDates.length > 0) {
        var string = jQuery.datepicker.formatDate('yy-mm-dd', date);
        if ($.inArray(string, enabledDates) != -1) {
            if (new Date(string).setHours(0, 0, 0, 0) < new Date().setHours(0, 0, 0, 0)) {
                return [false, "", "Not Available"];
            }
            else {
                return [true, "", "Available"];
            }
        }
        else {
            return [false, "", "Not Available"];
        }
    }
    else {
        var day = date.getDay();
        var date = $.datepicker.formatDate('yy-mm-dd', date);
        if (date < $('#rangedStartDate').val()) {
            return [false, "", "Not Available"];
        }

        if (new Date(date).setHours(0, 0, 0, 0) < new Date().setHours(0, 0, 0, 0)) {
            return [false, "", "Not Available"];
        }

        if (date > $('#rangedEndDate').val()) {
            return [false, "", "Not Available"];
        }

        if ($.inArray(day, allowedDaysOfWeek) != -1) {
            return [true, "", "Available"];
        }
        else {
            return [false, "", "Not Available"];
        }
    }
}


$('#tourDate').datepicker({
    beforeShowDay: EnableSpecificDates,
    dateFormat: 'yy-mm-dd',
}).on("change", function () {
    var splitter = $(this).val().split('-');
    var tourDay = parseInt(splitter[2], 10);
    var tourMonth = parseInt(splitter[1], 10);
    var tourYear = parseInt(splitter[0], 10);
    var date = new Date(tourYear, tourMonth - 1, tourDay);
    console.log(date);
    if (date.getFullYear() == tourYear && date.getMonth() + 1 == tourMonth && date.getDate() == tourDay) {
        $('#showDateToGo').text(date.getDate() + " " + date.toLocaleString("en-us", { month: "short" }) + " " + date.getFullYear());
    }
    else {
        $('#showDateToGo').text("Invalid Date");
    }
    $('#selectedMaximumPack').text(maxPassSelected[enabledDates.indexOf(tourDate.value)]);
});


$('#orderReferencesList').on('change', function () {
    var orderReference = $("#orderReferencesList option:selected").val();
    getOrderDetail(orderReference);
});

function getOrderDetail(orderReference) {
    if (orderReference == "" || orderReference == null) {
    }
    else{
        $.ajax({
            type: 'POST',
            url: '/Gaefa/GetDetailByReference/',
            data: AddAntiForgeryToken({
                orderReference: orderReference,
            }),
            dataType: "json",
            beforeSend: function () {
                $('#orderReferenceAlert').html("<div class='alert alert-info'><span aria-hidden='true'><i class='fa fa-refresh fa-pulse'></i></span><span class='sr-only'>Loading: </span> Please Wait...</div>");
            },
            success: function (data) {
                if (data.status == "error") {
                    $('#orderReferenceAlert').html("<div class='alert alert-danger' role='alert'><span class='glyphicon glyphicon-exclamation-sign' aria-hidden='true'></span><span class='sr-only'>Error: </span> Order details not found.</div>");
                    $('#detailOrderID').text("");
                    $('#detailPackageID').text("");
                    $('#detailOrderReference').text("");
                    $('#detailDate').text("");
                    $('#detailPackAdult').text("");
                    $('#detailPackChild').text("");
                    $('#detailPackChildNoBed').text("");
                    $('#detailPriceAdult').text("");
                    $('#detailPriceChild').text("");
                    $('#detailPriceChildNoBed').text("");
                    $('#detailNote').text("");
                }
                else {
                    $('#orderReferenceAlert').html("<div class='alert alert-success'><span class='glyphicon glyphicon-ok' aria-hidden='true'></span><span class='sr-only'>Success: </span> Order details found.</div>");
                    $('#detailOrderID').text(data.detail.orderId);
                    $('#detailPackageID').text(data.detail.package_publish_id);
                    $('#detailOrderReference').text(data.detail.orderReference);
                    $('#detailDate').text(data.detail.date.substring(0, 10));
                    $('#detailPackAdult').text(data.detail.pack_adult + " pax");
                    $('#detailPackChild').text(data.detail.pack_child + " pax");
                    $('#detailPackChildNoBed').text(data.detail.pack_child_nobed + " pax");
                    $('#detailPriceAdult').text("US$ " + data.detail.price_adult);
                    $('#detailPriceChild').text("US$ " + data.detail.price_child);
                    $('#detailPriceChildNoBed').text("US$ " + data.detail.price_child_nobed);
                    $('#detailNote').text(data.detail.note);
                }
            },
            error: function (error) {
                console.log(error);
                $('#orderReferenceAlert').html("<div class='alert alert-danger' role='alert'><span class='glyphicon glyphicon-exclamation-sign' aria-hidden='true'></span><span class='sr-only'>Error: </span> Order details not found.</div>");
            }
        });
    }
}


$('#repeatGetOrderDetail').on('click', function () {
    getOrderDetail($("#orderReferencesList option:selected").val());
});

$('#ticketList').on('change', function () {
    if ($("#ticketList option:selected").val() == "" || $("#ticketList option:selected").val() == null) {
        $('#ticketName').text('');
        $('#destination').text('');
        getTicketDetail(null, true);
    }
    else {
        var id = $("#ticketList option:selected").val();
        getTicketDetail(id, true);
    }
});

function getTicketDetail(id, alert) {
    var tagList = [];
    $.ajax({
        type: 'POST',
        url: '/Gaefa/getTicketList/',
        data: AddAntiForgeryToken({
            id: id,
        }),
        dataType: "json",
        beforeSend: function () {
            if(alert) $('#ticketListAlert').html("<div class='alert alert-info'><span aria-hidden='true'><i class='fa fa-refresh fa-pulse'></i></span><span class='sr-only'>Loading: </span> Please Wait...</div>");
        },
        success: function (data) {
            if (data.status == "error") {
                if(alert) $('#ticketListAlert').html("<div class='alert alert-danger' role='alert'><span class='glyphicon glyphicon-exclamation-sign' aria-hidden='true'></span><span class='sr-only'>Error: </span> An error has occured.</div>");
                $('#ticketName').text('');
                $('#destination').text('');
                $.each(data.allTag, function (k, v) {
                    tagList.push('<div class="input-group input-group-sm col-md-6" style="margin-top:1%"><span class="input-group-addon"><input type="checkbox" name="checkboxTag" value="' + v + '"/></span><input type="text" class="form-control tagText" readonly value="' + v + '"/><span class="input-group-btn"><button class="btn btn-danger removeTag">X</button></span></div>');
                });
            }
            else {
                if(alert) $('#ticketListAlert').html("<div class='alert alert-success'><span class='glyphicon glyphicon-ok' aria-hidden='true'></span><span class='sr-only'>Success: </span> Details found.</div>");
                $('#ticketName').text(data.detail.name);
                $('#destination').text(data.detail.location);
                if (data.tagList == "null") {
                    $.each(data.allTag, function (k, v) {
                        tagList.push('<div class="input-group input-group-sm col-md-6" style="margin-top:1%"><span class="input-group-addon"><input type="checkbox" name="checkboxTag" value="' + v + '"/></span><input type="text" class="form-control tagText" readonly value="' + v + '"/><span class="input-group-btn"><button class="btn btn-danger removeTag">X</button></span></div>');
                    });
                }
                else {
                    $.each(data.allTag, function (k, v) {
                        if ($.inArray(v, data.tagList) == -1) {
                            tagList.push('<div class="input-group input-group-sm col-md-6" style="margin-top:1%"><span class="input-group-addon"><input type="checkbox" name="checkboxTag" value="' + v + '"/></span><input type="text" class="form-control tagText" readonly value="' + v + '"/><span class="input-group-btn"><button class="btn btn-danger removeTag">X</button></span></div>');
                        }
                        else {
                            tagList.push('<div class="input-group input-group-sm col-md-6" style="margin-top:1%"><span class="input-group-addon"><input type="checkbox" name="checkboxTag" checked value="' + v + '"/></span><input type="text" class="form-control tagText" readonly value="' + v + '"/><span class="input-group-btn"><button class="btn btn-danger removeTag">X</button></span></div>');
                        }
                    });
                }
            }
            $('#tagList').html(tagList);
        },
        error: function (error) {
            console.log(error);
            $('#ticketListAlert').html("<div class='alert alert-danger' role='alert'><span class='glyphicon glyphicon-exclamation-sign' aria-hidden='true'></span><span class='sr-only'>Error: </span> An error has occured.</div>");
        }
    });
}

$('#addTagButton').on('click', function () {
    var tagName = $('#tagName').val();
    var id = $("#ticketList option:selected").val();
    if (tagName == "" || tagName == null) {
        $('#ticketListAlert').html("<div class='alert alert-danger' role='alert'><span class='glyphicon glyphicon-exclamation-sign' aria-hidden='true'></span><span class='sr-only'>Error: </span> Please specify the tag name.</div>");
    }
    else {
        $.ajax({
            type: 'POST',
            url: '/Gaefa/addTag/',
            data: AddAntiForgeryToken({
                tagName: tagName,
            }),
            dataType: "json",
            beforeSend: function () {
                $('#ticketListAlert').html("<div class='alert alert-info'><span aria-hidden='true'><i class='fa fa-refresh fa-pulse'></i></span><span class='sr-only'>Loading: </span> Please Wait...</div>");
            },
            success: function (data) {
                if (data.status == "exist") {
                    $('#ticketListAlert').html("<div class='alert alert-danger' role='alert'><span class='glyphicon glyphicon-exclamation-sign' aria-hidden='true'></span><span class='sr-only'>Error: </span> The tag is already existed.</div>");
                }
                else {
                    $('#ticketListAlert').html("<div class='alert alert-success'><span class='glyphicon glyphicon-ok' aria-hidden='true'></span><span class='sr-only'>Success: </span> Tag is added.</div>");
                    $('#tagList').append('<div class="input-group input-group-sm col-md-6" style="margin-top:1%"><span class="input-group-addon"><input type="checkbox" name="checkboxTag" value="' + tagName.toProperCase() + '"/></span><input type="text" class="form-control tagText" readonly value="' + tagName.toProperCase() + '"/><span class="input-group-btn"><button class="btn btn-danger removeTag">X</button></span></div>');
                }
                $('#tagName').val('');
            },
            error: function (error) {
                console.log(error);
                $('#ticketListAlert').html("<div class='alert alert-danger' role='alert'><span class='glyphicon glyphicon-exclamation-sign' aria-hidden='true'></span><span class='sr-only'>Error: </span> An error has occured.</div>");
            }
        });
    }
});

/*
function getTagList() {
    var tagList = [];
    $.ajax({
        type: 'POST',
        url: '/Gaefa/GetTagList/',
        data: AddAntiForgeryToken({
        }),
        dataType: "json",
        success: function (data) {
            $.each(data.tagList, function (k, v) {
                tagList.push('<div class="input-group input-group-sm col-md-6"><span class="input-group-addon"><input type="checkbox" name="checkboxTag" value="' + v + '"/></span><input type="text" class="form-control tagText" readonly value="' + v + '"/><span class="input-group-btn"><button class="btn btn-danger removeTag">X</button></span></div><br/>');
            });
            $('#tagList').html(tagList);
        },
        error: function (error) {
            console.log(error);
            $('#ticketListAlert').html("<div class='alert alert-danger' role='alert'><span class='glyphicon glyphicon-exclamation-sign' aria-hidden='true'></span><span class='sr-only'>Error: </span> An error has occured.</div>");
        }
    });
}
*/

$('.panel-body').on('click', '.removeTag', function () {
    var tagName = $(this).parent().siblings('.tagText').val();
    var removedDiv = $(this).parents('.input-group');
    $.ajax({
        type: 'POST',
        url: '/Gaefa/removeTag/',
        data: AddAntiForgeryToken({
            tagName: tagName,
        }),
        dataType: "json",
        beforeSend: function () {
            $('#ticketListAlert').html("<div class='alert alert-info'><span aria-hidden='true'><i class='fa fa-refresh fa-pulse'></i></span><span class='sr-only'>Loading: </span> Please Wait...</div>");
        },
        success: function (data) {
            if (data.status == "error") {
                $('#ticketListAlert').html("<div class='alert alert-danger' role='alert'><span class='glyphicon glyphicon-exclamation-sign' aria-hidden='true'></span><span class='sr-only'>Error: </span> An error has occured.</div>");
            }
            else {
                $('#ticketListAlert').html("<div class='alert alert-success'><span class='glyphicon glyphicon-ok' aria-hidden='true'></span><span class='sr-only'>Success: </span> Tag is removed.</div>");
                removedDiv.remove();
            }
        },
        error: function (error) {
            console.log(error);
            $('#ticketListAlert').html("<div class='alert alert-danger' role='alert'><span class='glyphicon glyphicon-exclamation-sign' aria-hidden='true'></span><span class='sr-only'>Error: </span> An error has occured.</div>");
        }
    });
});

$('#bindTag').on('click', function () {
    var tags = "";
    var id = $("#ticketList option:selected").val();
    if (id == "" || id == null) return;
    var total = $('input[type=checkbox][name=checkboxTag]:checked').length;
    $('input[type=checkbox][name=checkboxTag]:checked').each(function (index) {
        tags += $(this).val();
        if (index != total - 1) {
            tags += ",";
        }
    });

    $.ajax({
        type: 'POST',
        url: '/Gaefa/BindTag/',
        data: AddAntiForgeryToken({
            id: id,
            tagName: tags,
        }),
        dataType: "json",
        beforeSend: function () {
            $('#ticketListAlert').html("<div class='alert alert-info'><span aria-hidden='true'><i class='fa fa-refresh fa-pulse'></i></span><span class='sr-only'>Loading: </span> Please Wait...</div>");
        },
        success: function (data) {
            if (data.status == "error") {
                $('#ticketListAlert').html("<div class='alert alert-danger' role='alert'><span class='glyphicon glyphicon-exclamation-sign' aria-hidden='true'></span><span class='sr-only'>Error: </span> An error has occured.</div>");
            }
            else {
                $('#ticketListAlert').html("<div class='alert alert-success'><span class='glyphicon glyphicon-ok' aria-hidden='true'></span><span class='sr-only'>Success: </span> Binding successful.</div>");
            }
        },
        error: function (error) {
            console.log(error);
            $('#ticketListAlert').html("<div class='alert alert-danger' role='alert'><span class='glyphicon glyphicon-exclamation-sign' aria-hidden='true'></span><span class='sr-only'>Error: </span> An error has occured.</div>");
        }
    });
});