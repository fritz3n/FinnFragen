$(function () {

    function updateDisabled() {
        if ($('.consentCheckbox').is(':checked')) {
            $('#submitButton').removeAttr('disabled');
            bootstrap.Tooltip.getInstance($("#askTooltip")[0]).disable();
        } else {
            $('#submitButton').attr('disabled', 'disabled');
            bootstrap.Tooltip.getInstance($("#askTooltip")[0]).enable();
        }
    }

    $('.consentCheckbox').click(function () {
        updateDisabled();
    });
    updateDisabled();
});