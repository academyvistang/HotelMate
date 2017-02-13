
var closethisGameCalledByMe = true;

$(function () {


    //window.onbeforeunload = function (e)
    //{

    //    e = e || window.event;
    //    var y = e.pageY || e.clientY;
    //    if (closethisGameCalledByMe) {
    //        if (y < 0) {
    //            return "You are involved in an active game, closing/refreshing this window will result in you forfeiting the game and being charged Do you want to continue? "
    //        }
    //        else {
    //            return "You are involved in an active game, closing/refreshing this window will result in you forfeiting the game and being charged Do you want to continue?";
    //        }
    //    }
    //}

    CloseChatBox();

    function CloseChatBox() {
        $('.wrap_box1').hide();
        $('.main_chat1').addClass('hide_wrap_box');
    }

    function OpenChatBox() {
        $('.wrap_box1').show();
        $('.main_chat1').removeClass('hide_wrap_box');
    }

    $('.closed1').click(function () {
        $('.wrap_box1').hide();
        $('.main_chat1').addClass('hide_wrap_box');
    });

    //Open
    $('.open_chat1').click(function () {
        $('.wrap_box1').show();
        $('.main_chat1').removeClass('hide_wrap_box');
    });

    // Prepare
    $(window).bind('popstate', function (e) {
        // the library returns the normal URL from the event object
        var cLocation = history.location || document.location;
        loadContent(cLocation.pathname + cLocation.search + cLocation.hash);
    });

    var myMessages = ['info', 'warning', 'error', 'success', 'mutu', 'otherplayermutu', 'nomutu'];

    function hideAllMessages() {
        var messagesHeights = new Array(); // this array will store height for each
        for (i = 0; i < myMessages.length; i++) {
            messagesHeights[i] = $('.' + myMessages[i]).outerHeight(); // fill array
            $('.' + myMessages[i]).css('top', -messagesHeights[i]); //move element outside viewport
        }
    }

    function showMessage(type)
    {    
        $('.' + type + '-trigger').click(function () {
            hideAllMessages();
            $('.' + type).animate({ top: "0" }, 500);
        });
    }

    function showSexyMessage(type) {
        hideAllMessages();
        $('.' + type).animate({ top: "110" }, 500);
    }


    hideAllMessages();

    // Show message
    for (var i = 0; i < myMessages.length; i++) {
        showMessage(myMessages[i]);
    }

    // When message is clicked, hide it
    $('.message').click(function () {
        $(this).animate({ top: -$(this).outerHeight() }, 500);
    });

    var agboGameId = $("#NewGameId").val();

    document.onkeydown = function (e) {
        if (e.keyCode === 116) {
            return false;
        }
    };

    //    function DetectBrowserExit() {
    //        alert('Execute task which do you want before exit');
    //    }

    //    window.onbeforeunload = function () { DetectBrowserExit(); }
    // Proxy created on the fly          
    var hub = $.connection.agbo;

    SetupPagination();

    var userCount = $("#UserCount").val();

    // Declare a function on the chat hub so the server can invoke it          
    hub.client.addMessage = function (message) {
        ReloadMyScreen(message);
        PlayerHasPlayedSound();
    };

    // Declare a function on the chat hub so the server can invoke it          
    hub.client.closeGame = function (message) {
        closethisGameCalledByMe = false;
        alert("We are sorry, a user has left the game, as the game was active the game prize will be deducted from his account, you will now be redirected to the home page to choose another game.");
        location.assign("/home/Index/");
    };

    // Declare a function on the chat hub so the server can invoke it          
    hub.client.closeGameRedirect = function (message) {
        closethisGameCalledByMe = true;
        alert("You have choosen to close an active game, Your account have been now been charged as having lost the game, this practice is forbidden and you can be barred.");
        location.assign("/home/Index/");
    };

    hub.client.broadcastMessage = function (name, message) {
        // Html encode display name and message.

        var strMsg = "<div class='message_post'><img src='../../Images/" + name + "' width='35' class='left user_img' alt= '' /><div class='message_text left' style=' width:140px; color:Red;'>" + message + "</div><div class='clear'></div></div>";

        $('#reply_output').append(strMsg);
        OpenChatBox();
        PlayerHasSentMessageSound();
    };


    function PlayerHasPlayedSound() {
        $.playSound('../../Sounds/gameover.wav');
    }

    function PlayerHasSentMessageSound() {
        $.playSound('../../Sounds/jump.wav');
    }

    // Start the connection
    $.connection.hub.start().done(function ()
    {          
        SetupPagination();

        if (userCount == 1) {
            hub.server.join(agboGameId);
        }

        if (userCount > 1) {
            var gameId = $("#GameId").val();
            hub.server.send(gameId, agboGameId);
            hub.server.join(agboGameId);
        }
    });

    function ReloadMyScreen(Id) {
        var url = $("#GlobalUrlAction").val();
        $.get(url, { start: 1 }, function (data) {
            $('#agbo21gamecontainer').empty();
            $('#agbo21gamecontainer').append(data.TopView);
            if (data.TopMessage.length > 0) {
                showSexyMessage(data.TopMessage);
            }
            SetupPagination();
        });
    }

    function SetupPagination()
    {
        $('#gmail_loading').hide();

        setTimeout(function () {
            $('.messageBoxDissapearing').fadeOut('fast');
        }, 4000); // <-- time in milliseconds 

        $(".clickmessage").click(function (e) {

            var msg = $("#search_field").val();
            var name = $("#CurrentUserName").val();

            if (msg.length > 0) {
                hub.server.sendmessage(name, msg, agboGameId);
            }

            $('#search_field').val('').focus();

        });

        $(".clickablecard").click(function (e)
        {            
            $('#gmail_loading').show();

            closethisGameCalledByMe = false;

            //Get the new view and broadcast to call
            hideAllMessages();

            e.preventDefault();

            var url = $(this).attr("href");

            history.pushState(null, null, url);

            var gameId = $("#GameId").val();           

            $.get(url, { start: 1 }, function (data)
            {
                $('#agbo21gamecontainer').empty();
                $('#agbo21gamecontainer').append(data.TopView);
                if (data.TopMessage.length > 0) {
                    showSexyMessage(data.TopMessage);
                }
                SetupPagination();
                hub.server.send(gameId, agboGameId);
                
            }).done(function () {
                $('#gmail_loading').hide();
            });
        });
    }
});





