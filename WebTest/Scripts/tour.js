
//Tour List
$(".panel-body > .pull-right").click(function () {
    if ($(this).parent().nextUntil('.panel-collapse .collapse').is(":visible")) {
        $(this).text("Show More...");
    }
    else {
        $(this).text("Show Less...");
    }
    $(this).parent().nextUntil('.panel-collapse .collapse').slideToggle(500);
});


var passCount = 1;
var boxCount = 1;
//var appendPassengerName = '<div class="row appended" style="margin-top:1%"><div class="col-md-7"><input type="text" style="width: 100%" class="form-control" name="name' + passCount + '" placeholder="Passenger Name" /></div><div class="col-md-2" style="margin-top:2%"><a href="javascript:void(0)"><span class="glyphicon glyphicon-minus-sign"></span></a></div></div>';
$(".glyphicon-plus-sign").click(function () {
    $(this).parents('.panel-body').append('<div class="row appendedDiv" style="margin-top:1%"><div class="col-md-7"><input type="text" style="width: 100%" class="form-control passenger" id="passengerName' + boxCount + '" name="passenger" placeholder="Passenger Name" /></div><div class="col-md-2" style="margin-top:2%"><a href="javascript:void(0)"><span class="glyphicon glyphicon-minus-sign"></span></a></div></div>');
    boxCount += 1;
    passCount += 1;
    updateTotalPriceAndPeopleCount();
});

$(".panel-body").on('click', '.glyphicon-minus-sign', function () {
    $(this).parents('.appendedDiv').remove();
    passCount -= 1;
    updateTotalPriceAndPeopleCount();
});

function updateTotalPriceAndPeopleCount() {
    $("#totalPrice").text((parseFloat(passCount) * parseFloat($("#pricePerPax").text())).toFixed(2));
    $('#peopleCount b').text(passCount);
    $('.passenger').each(function () {
        $(this).rules("add",
            {
                required: true,
                letterswithbasicpunc: true
            })
    });
}



$('input[name="payopt"]').click(function () {
    $(this).tab('show');
});

$.validator.addMethod("greaterThanEqualDate", function (value, element, params) {

    if (!/Invalid|NaN/.test(new Date(value))) {
        return new Date(value) >= new Date($(params).val());
    }

    return isNaN(value) && isNaN($(params).val())
        || (Number(value) > Number($(params).val()));
}, 'Must be greater than or equal to Date From.');

$.validator.addMethod("money", function (value, element) {
        var isValidMoney = /^\d{0,10}(\.\d{0,2})?$/.test(value);
        return this.optional(element) || isValidMoney;
    },
    "Please enter money amount with 2 decimals point"
);


var country_list = ["Afghanistan", "Albania", "Algeria", "Andorra", "Angola", "Anguilla", "Antigua and Barbuda", "Argentina", "Armenia", "Aruba", "Australia", "Austria", "Azerbaijan", "Bahamas", "Bahrain", "Bangladesh", "Barbados", "Belarus", "Belgium", "Belize", "Benin", "Bermuda", "Bhutan", "Bolivia", "Bosnia and Herzegovina", "Botswana", "Brazil", "British Virgin Islands", "Brunei", "Bulgaria", "Burkina Faso", "Burundi", "Cambodia", "Cameroon", "Cape Verde", "Cayman Islands", "Chad", "Chile", "China", "Colombia", "Congo", "Cook Islands", "Costa Rica", "Cote D Ivoire", "Croatia", "Cruise Ship", "Cuba", "Cyprus", "Czech Republic", "Denmark", "Djibouti", "Dominica", "Dominican Republic", "Ecuador", "Egypt", "El Salvador", "Equatorial Guinea", "Estonia", "Ethiopia", "Falkland Islands", "Faroe Islands", "Fiji", "Finland", "France", "French Polynesia", "French West Indies", "Gabon", "Gambia", "Georgia", "Germany", "Ghana", "Gibraltar", "Greece", "Greenland", "Grenada", "Guam", "Guatemala", "Guernsey", "Guinea", "Guinea Bissau", "Guyana", "Haiti", "Honduras", "Hong Kong", "Hungary", "Iceland", "India", "Indonesia", "Iran", "Iraq", "Ireland", "Isle of Man", "Israel", "Italy", "Jamaica", "Japan", "Jersey", "Jordan", "Kazakhstan", "Kenya", "Kuwait", "Kyrgyz Republic", "Laos", "Latvia", "Lebanon", "Lesotho", "Liberia", "Libya", "Liechtenstein", "Lithuania", "Luxembourg", "Macau", "Macedonia", "Madagascar", "Malawi", "Malaysia", "Maldives", "Mali", "Malta", "Mauritania", "Mauritius", "Mexico", "Moldova", "Monaco", "Mongolia", "Montenegro", "Montserrat", "Morocco", "Mozambique", "Namibia", "Nepal", "Netherlands", "Netherlands Antilles", "New Caledonia", "New Zealand", "Nicaragua", "Niger", "Nigeria", "Norway", "Oman", "Pakistan", "Palestine", "Panama", "Papua New Guinea", "Paraguay", "Peru", "Philippines", "Poland", "Portugal", "Puerto Rico", "Qatar", "Reunion", "Romania", "Russia", "Rwanda", "Saint Pierre and Miquelon", "Samoa", "San Marino", "Satellite", "Saudi Arabia", "Senegal", "Serbia", "Seychelles", "Sierra Leone", "Singapore", "Slovakia", "Slovenia", "South Africa", "South Korea", "Spain", "Sri Lanka", "St Kitts and Nevis", "St Lucia", "St Vincent", "St. Lucia", "Sudan", "Suriname", "Swaziland", "Sweden", "Switzerland", "Syria", "Taiwan", "Tajikistan", "Tanzania", "Thailand", "Timor L'Este", "Togo", "Tonga", "Trinidad and Tobago", "Tunisia", "Turkey", "Turkmenistan", "Turks and Caicos", "Uganda", "Ukraine", "United Arab Emirates", "United Kingdom", "Uruguay", "Uzbekistan", "Venezuela", "Vietnam", "Virgin Islands (US)", "Yemen", "Zambia", "Zimbabwe"];


$(document).ready(function () {


    $('#searchBook').validate({
        rules: {
            lastName: {
                required: true,
            },
            bookingNumber: {
                required: true,
                minlength: 5 //length may vary
            },
        },
    });

    //Create Form
    $('#createForm').validate({
        rules: {
            destination: {
                required: true,
            },
            dateFrom: {
                required: true,
                date: true
            },
            dateUntil: {
                required: true,
                date: true,
                greaterThanEqualDate: dateFrom
            },
            price: {
                required: true,
                money: true
            }
        },

    });

   

    $.each(country_list, function (key, value) {
        $("#countryList").append($("<option></option>").attr("value", value).text(value));
    });

    
});

$('#checkoutCart').on('click', function (e) {
    if (!$("#checkoutForm").valid() || !$('.passenger').valid()) {
        e.preventDefault();
    }
});

/*$('#searchForm').submit(function (e) {
    e.preventDefault();
    var formData = JSON.stringify($("#searchForm").serializeObject());
    //console.log(formData);
    $('html').append(formData);
});*/

$.fn.serializeObject = function () {
    var o = {};
    var a = this.serializeArray();
    $.each(a, function () {
        if (o[this.name] !== undefined) {
            if (!o[this.name].push) {
                o[this.name] = [o[this.name]];
            }
            o[this.name].push(this.value || '');
        } else {
            o[this.name] = this.value || '';
        }
    });
    return o;
};

var formData = JSON.stringify($("#searchForm").serializeObject());


//var formData = JSON.stringify($("#createForm").serializeArray());
/*$('#searchForm').submit(function (e) {
    e.preventDefault();
    var formData = JSON.stringify($("#searchForm").serializeObject());
    $.ajax({
        type: 'POST',
        url: '/Tour/List/',
        data: formData,
        dataType: "json",
        success: function (data) {
            alert(data);
        },
        error: function (error) {
            alert(error);
        }
    });
})*/

/*$("#countryList").change(function () {
    alert($(this).find(':selected').val());
});*/

$('#checkoutForm').validate({
    rules: {
        passenger: {
            required: true,
            letterswithbasicpunc: true
        },
    },
});


$('.buttonToCheckout').on('click', function (e) {
    //console.log($("#forgotEmail").val());
    var session = $("#session").val();
    if (session == null || session == "") {
        e.preventDefault();
        $('#loginBeforeCheckoutAlert').html("<div class='alert alert-danger' role='alert'><span class='glyphicon glyphicon-exclamation-sign' aria-hidden='true'></span><span class='sr-only'>Error: </span> You have to login first before you can checkout!</div>");
    }
});