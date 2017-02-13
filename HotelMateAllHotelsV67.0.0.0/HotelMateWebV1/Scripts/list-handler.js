$(function () {
    
    $('#add').click(function () {
        var newList = $("#current");
        $('#all option:selected').each(function (index, value) {
            newList.append(value);
        });
    });

    $('#remove').click(function () {

        if (confirm('Are you sure you want to remove videos?')) {
            var newList = $("#all");
            $('#current option:selected').each(function (index, value) {
                newList.append(value);
            });
        }

    });


    $('#edit').click(function () {
        $('#current').find('option').attr('selected', 'selected');
    });
});
