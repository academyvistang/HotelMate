$(function () {
    alert("BBBB");

    $('.datepicker').datepicker({ defaultDate: null, onClose: function () { $(this).valid(); }, dateFormat: "dd/mm/yy" });

    var dateFormat = "dd/mm/yy"; // en-gb date format, substitute your own
    jQuery.validator.addMethod(
        'date',
        function (value, element, params) {

            if (this.optional(element)) {
                return true;
            };
            var result = false;
            try {
                $.datepicker.parseDate(dateFormat, value);
                result = true;
            } catch (err) {
                result = false;
            }
            return result;
        },
        ''
    );
});
