$(document).ready(function () {
    $('input[type=text]').val('');
    $('input[type=email]').val('');
    $('input[type=date]').val('');
    $('input[type=number]').val('');
    $('input:checkbox').removeAttr('checked');
});