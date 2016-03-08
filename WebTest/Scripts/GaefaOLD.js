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

var passCount = parseInt($('#peopleCount').text());
var priceBeforeTax;
var priceAfterTax;
//var appendPassengerName = '<div class="row appended" style="margin-top:1%"><div class="col-md-7"><input type="text" style="width: 100%" class="form-control" name="name' + passCount + '" placeholder="Passenger Name" /></div><div class="col-md-2" style="margin-top:2%"><a href="javascript:void(0)"><span class="glyphicon glyphicon-minus-sign"></span></a></div></div>';
$(".glyphicon-plus-sign").click(function () {
    $(this).parents('.panel-body').append('<div class="row appendedDiv" style="margin-top:1%"><div class="col-md-7"><input type="text" style="width: 100%" class="form-control passenger" pattern="^[a-zA-Z]{3,}?([ ][a-zA-Z \.]{1,})?$" title="Minimum 3 letters for your first name. Only letters and space(s) are allowed" required id="passengerName' + boxCount + '" name="passenger" placeholder="Passenger Name" /></div><div class="col-md-2" style="margin-top:2%"><a href="javascript:void(0)"><span class="glyphicon glyphicon-minus-sign"></span></a></div></div>');
    
    passCount += 1;
    updateTotalPriceAndPeopleCount();
});

$(".panel-body").on('click', '.glyphicon-minus-sign', function () {
    $(this).parents('.appendedDiv').remove();
    passCount -= 1;
    updateTotalPriceAndPeopleCount();
});


function updateTotalPriceAndPeopleCount() {
    priceBeforeTax = (parseFloat(passCount) * parseFloat($("#pricePerPax").text())).toFixed(2);
    priceAfterTax = (((parseFloat(passCount) * parseFloat($("#pricePerPax").text())) + 0.3) * 1000 / 961).toFixed(2);

    if ($('input[name="payopt"]:checked').val() == "Transfer") {
        $("#totalPrice").text(priceBeforeTax);
        $('#hiddenPrice').val(priceBeforeTax);
    }
    else {
        $("#totalPrice").text(priceAfterTax);
        $('#hiddenPrice').val(priceAfterTax);
        $('#additionalFee').text((priceAfterTax - priceBeforeTax).toFixed(2));
    }
    
    $("#peopleCount b").text(passCount);
    
    /*
    $('.passenger').each(function () {
        $(this).rules("add",
            {
                required: true,
                letterswithbasicpunc: true
            })
    });
    */
}

$('input[name="payopt"]').change(function () {
    //$('#additionalFee').text(((((parseFloat($('#totalPrice').text())) + 0.3) * 1000 / 961) - parseFloat($('#totalPrice').text())).toFixed(2));
    $(this).tab('show');
    if ($('input[name="payopt"]:checked').val() == "Transfer") {
        $('#iconPayOpt').html("<i class='fa fa-credit-card' title='Pay by transfer'></i>");
        $("#totalPrice").text(priceBeforeTax);
        $('#hiddenPrice').val(priceBeforeTax);
    }
    else {
        $('#iconPayOpt').html("<i class='fa fa-paypal' title='Pay by PayPal'></i>");
        $("#totalPrice").text(priceAfterTax);
        $('#hiddenPrice').val(priceAfterTax);
        $('#additionalFee').text((priceAfterTax - priceBeforeTax).toFixed(2));
    }
});

/*
$('#checkoutForm').validate({
    rules: {
        passenger: {
            required: true,
            letterswithbasicpunc: true
        },
    },
});*/

/*
$('#checkoutCart').on('click', function (e) {
    if (!$("#checkoutForm").valid() || !$('.passenger').valid()) {
        e.preventDefault();
    }
});
*/

/*
$.validator.addMethod("money", function (value, element) {
    var isValidMoney = /^\d{0,10}(\.\d{0,2})?$/.test(value);
    return this.optional(element) || isValidMoney;
},
    "Please enter money amount with 2 decimals point"
);

jQuery.validator.addMethod("greaterThanOrEqual", function (value, element, params) {
    return this.optional(element) || (parseFloat(value) >= parseFloat($(params[0]).text()));
}, 'Must be greater than or equal to {1}.');
*/

var country_list = ["Afghanistan", "Albania", "Algeria", "Andorra", "Angola", "Anguilla", "Antigua and Barbuda", "Argentina", "Armenia", "Aruba", "Australia", "Austria", "Azerbaijan", "Bahamas", "Bahrain", "Bangladesh", "Barbados", "Belarus", "Belgium", "Belize", "Benin", "Bermuda", "Bhutan", "Bolivia", "Bosnia and Herzegovina", "Botswana", "Brazil", "British Virgin Islands", "Brunei", "Bulgaria", "Burkina Faso", "Burundi", "Cambodia", "Cameroon", "Cape Verde", "Cayman Islands", "Chad", "Chile", "China", "Colombia", "Congo", "Cook Islands", "Costa Rica", "Cote D Ivoire", "Croatia", "Cruise Ship", "Cuba", "Cyprus", "Czech Republic", "Denmark", "Djibouti", "Dominica", "Dominican Republic", "Ecuador", "Egypt", "El Salvador", "Equatorial Guinea", "Estonia", "Ethiopia", "Falkland Islands", "Faroe Islands", "Fiji", "Finland", "France", "French Polynesia", "French West Indies", "Gabon", "Gambia", "Georgia", "Germany", "Ghana", "Gibraltar", "Greece", "Greenland", "Grenada", "Guam", "Guatemala", "Guernsey", "Guinea", "Guinea Bissau", "Guyana", "Haiti", "Honduras", "Hong Kong", "Hungary", "Iceland", "India", "Indonesia", "Iran", "Iraq", "Ireland", "Isle of Man", "Israel", "Italy", "Jamaica", "Japan", "Jersey", "Jordan", "Kazakhstan", "Kenya", "Kuwait", "Kyrgyz Republic", "Laos", "Latvia", "Lebanon", "Lesotho", "Liberia", "Libya", "Liechtenstein", "Lithuania", "Luxembourg", "Macau", "Macedonia", "Madagascar", "Malawi", "Malaysia", "Maldives", "Mali", "Malta", "Mauritania", "Mauritius", "Mexico", "Moldova", "Monaco", "Mongolia", "Montenegro", "Montserrat", "Morocco", "Mozambique", "Namibia", "Nepal", "Netherlands", "Netherlands Antilles", "New Caledonia", "New Zealand", "Nicaragua", "Niger", "Nigeria", "Norway", "Oman", "Pakistan", "Palestine", "Panama", "Papua New Guinea", "Paraguay", "Peru", "Philippines", "Poland", "Portugal", "Puerto Rico", "Qatar", "Reunion", "Romania", "Russia", "Rwanda", "Saint Pierre and Miquelon", "Samoa", "San Marino", "Satellite", "Saudi Arabia", "Senegal", "Serbia", "Seychelles", "Sierra Leone", "Singapore", "Slovakia", "Slovenia", "South Africa", "South Korea", "Spain", "Sri Lanka", "St Kitts and Nevis", "St Lucia", "St Vincent", "St. Lucia", "Sudan", "Suriname", "Swaziland", "Sweden", "Switzerland", "Syria", "Taiwan", "Tajikistan", "Tanzania", "Thailand", "Timor L'Este", "Togo", "Tonga", "Trinidad and Tobago", "Tunisia", "Turkey", "Turkmenistan", "Turks and Caicos", "Uganda", "Ukraine", "United Arab Emirates", "United Kingdom", "Uruguay", "Uzbekistan", "Venezuela", "Vietnam", "Virgin Islands (US)", "Yemen", "Zambia", "Zimbabwe"];


$(document).ready(function () {
    $('input[name="payopt"]').filter('[value=Transfer]').prop('checked', true);

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

    /*$.each(country_list, function (key, value) {
        $("#countryList").append($("<option></option>").attr("value", value).text(value));
    });*/
    /*
    $('#transferForm').validate({
        rules: {
            date: {
                required: true,
                date: true
            },
            sender: {
                required: true,
                letterswithbasicpunc: true
            },
            price: {
                required: true,
                money: true,
                greaterThanOrEqual: ["#priceTotal", "the total amount of price"]
            },
            bank: {
                required: true,
            },
            bankFrom: {
                required: true,
            },
            account: {
                required: true,
                minlength: 10
            },
            email: {
                required: true,
                email: true
            },
        },
    });
    */


    for (i = 1; i < passCount; i++) {
        $(".glyphicon-plus-sign").parents('.panel-body').append('<div class="row appendedDiv" style="margin-top:1%"><div class="col-md-7"><input type="text" style="width: 100%" class="form-control passenger" id="passengerName' + boxCount + '" name="passenger" placeholder="Passenger Name" /></div></div>');
        
        /*
        $('.passenger').each(function () {
            $(this).rules("add",
                {
                    required: true,
                    letterswithbasicpunc: true
                })
        });
        */
    }

    $("#totalPrice").text((parseFloat(passCount) * parseFloat($("#pricePerPax").text())).toFixed(2));
    $("#peopleCount b").text(passCount);
    $('#additionalFee').text(((((parseFloat($('#totalPrice').text())) + 0.3) * 1000 / 961) - parseFloat($('#totalPrice').text())).toFixed(2));

    priceBeforeTax = (parseFloat(passCount) * parseFloat($("#pricePerPax").text())).toFixed(2);
    priceAfterTax = (((parseFloat(passCount) * parseFloat($("#pricePerPax").text())) + 0.3) * 1000 / 961).toFixed(2);

    $('#pagination').twbsPagination({
        startPage: 1,
        totalPages: ($('#PackageAmount').val() != null) ? Math.ceil(parseInt($('#PackageAmount').val()) / parseInt($('#LimitPerPage').val())) : 1,
        visiblePages: 3,
        href: document.location.pathname + addUrlParam(document.location.search, 'page', '{{number}}'),
    });
    
    
})


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
            url: '/Gaefa/CancelBooking',
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
        url: '/Gaefa/ApprovePayment/',
        data: AddAntiForgeryToken({
            bookCode: bookCode,
            approveChoice: approveChoice,
        }),
        dataType: "json",
        beforeSend: function () {
            $('#approveAlert').html("<div class='alert alert-info'><span aria-hidden='true'><i class='fa fa-refresh fa-pulse'></i></span><span class='sr-only'>Loading: </span> Please Wait...</div>");
            $('.approveChoice').hide();
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
    $('#showLocation').html(details.location);
    $('#tourDate').html(date);
    $('input[type=hidden][name=bookingCode]').val(bookCodeNumber);
    if (bookStatus == "Unpaid") {
        $('#toDetailForm').attr("action", "/Gaefa/DetailWithoutPayment/");
    }
    else if (bookStatus == "Disapproved") {
        $('#toDetailForm').attr("action", "/Gaefa/DisapprovedTransfer/");
    }
    else {
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

/*
$('.buttonToCheckout').on('click', function (e) {
    //console.log($("#forgotEmail").val());
    var session = $("#session").val();
    if (session == null || session == "") {
        e.preventDefault();
        $('#loginBeforeCheckoutAlert').html("<div class='alert alert-danger' role='alert'><span class='glyphicon glyphicon-exclamation-sign' aria-hidden='true'></span><span class='sr-only'>Error: </span> You have to login first before you can checkout!</div>");
    }
});
*/
