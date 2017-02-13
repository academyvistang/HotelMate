
$(function () {
    // Proxy created on the fly          
    var hub = $.connection.agbo;

    var userCount = $("#UserCount").val();

    // Declare a function on the chat hub so the server can invoke it          
    hub.client.addMessage = function (message) {
        $('#agbo21gamecontainer').empty();
        $('#agbo21gamecontainer').append(message);
        SetupPagination();
    };

    // Start the connection
    $.connection.hub.start().done(function ()
    {
        alert("Started");

        SetupPagination();

        if (userCount > 1) {
            var innerhtml = $("#agbo21gamecontainer").html();
            hub.server.send(innerhtml);
        }

    });


    function SetupPagination() {

        $(".clickablecard").click(function (e) {
            //Get the new view and broadcast to call
            
            e.preventDefault();
            var url = $(this).attr("href");        

            $.get(url, { start: 1 }, function (data) {
            //alert(data);
            hub.server.send(data);
           });
            // Call the chat method on the server

        });
    }





});
