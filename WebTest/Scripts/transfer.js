AddAntiForgeryToken = function (data) {
    data.__RequestVerificationToken = $('#__AjaxAntiForgeryForm input[name=__RequestVerificationToken]').val();
    return data;
};

$.validator.addMethod("money", function (value, element) {
    var isValidMoney = /^\d{0,10}(\.\d{0,2})?$/.test(value);
    return this.optional(element) || isValidMoney;
},
    "Please enter money amount with 2 decimals point"
);

jQuery.validator.addMethod("greaterThanOrEqual", function (value, element, params) {
        return this.optional(element) || (parseFloat(value) >= parseFloat($(params[0]).text()));
}, 'Must be greater than or equal to {1}.');


$(document).ready(function () {
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
                greaterThanOrEqual: ["#priceTotal","the total amount of price"]
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

    
});


